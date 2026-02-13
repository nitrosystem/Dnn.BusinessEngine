export class BusinessEngineApp {
    constructor() {
        this._services = {};
        this._controllers = {};
        this._directives = {};

        this._events = new Map();
        this._watchers = [];
        this._watchMap = new WeakMap();

        this._reactiveFlag = Symbol('reactive');
    }

    //#region Registers Services & Directives & etc

    service(name, def) {
        const resolved = this._resolveDependencies(def);
        this._services[name] = new def(...resolved);
        return this;
    }

    directive(name, def) {
        this._directives[name] = def;
        return this;
    }

    filter(name, def) {
        const expressionService = this._services['expressionService'];
        expressionService.filters[name] = def;
        return this;
    }

    function(name, def) {
        const expressionService = this._services['expressionService'];
        expressionService.functions[name] = def;
        return this;
    }

    controller(name, def) {
        this._controllers[name] = def;
        return this;
    }

    //#endregion

    //#region Setup & Initializer

    async bootstrap(appElement) {
        const attrs = this._getAttributesObject(appElement);
        const ctrlName = attrs['b-controller'];
        const CtrlClass = this._controllers[ctrlName];

        const isDashboard = attrs.dashboard === 'True';
        const moduleId = attrs.module;
        const connectionId = attrs.connection;

        if (!CtrlClass || !moduleId || !connectionId)
            throw new Error(`Controller ${ctrlName} not found`);

        const isolatedApp = this.createIsolatedContext();
        const resolved = this._resolveDependencies(CtrlClass, isolatedApp);
        const controller = new CtrlClass(...resolved);
        const scope = await controller.onLoad(isDashboard, moduleId, connectionId);

        isolatedApp.scopeRoot = scope;
        isolatedApp.controller = controller;
        isolatedApp.detectElements(appElement, scope, controller, true);
    }

    createIsolatedContext() {
        const isolated = Object.create(Object.getPrototypeOf(this));

        isolated._services = this._services;
        isolated._directives = this._directives;
        isolated._directives = this._directives;
        isolated._directives = this._directives;
        isolated._controllers = this._controllers;

        isolated._watchers = [];
        isolated._watchMap = new WeakMap();
        isolated._events = new Map();

        return isolated;
    }

    //#endregion

    //#region Scan Elements 

    detectElements(element, scope, controller, isRoot = true, skipSelfDirection = "") {
        if (!element || !element.attributes)
            return;

        if (!isRoot && element.hasAttribute('b-controller'))
            return;

        const attrs = element.attributes;
        const directives = [];
        let isTerminal = false;

        // 1️⃣ collect directives
        for (let i = 0; i < attrs.length; i++) {
            const attr = attrs[i];
            if (attr.name === skipSelfDirection) continue;

            const defFactory = this._directives[attr.name];
            if (defFactory) {
                const resolved = this._resolveDependencies(defFactory);
                const def = defFactory(...resolved);
                directives.push({ name: attr.name, value: attr.value, def });
            }
        }

        // 2️⃣ compile directives (بدون mutation عمومی DOM)
        if (directives.length > 0) {
            directives.sort((a, b) => (b.def.priority || 0) - (a.def.priority || 0));

            for (const dir of directives) {
                if (typeof dir.def.compile === "function") {
                    if (!dir.def.terminal) this.bindAttributes(element, scope);

                    const stop = dir.def.compile(
                        this._getAttributesObject(element),
                        element,
                        scope,
                        controller
                    );

                    if (dir.def.terminal) {
                        isTerminal = true;
                        break; // ✅ بسیار مهم
                    }

                    if (stop === false)
                        return;
                }
            }
        }
        else
            this.bindAttributes(element, scope);

        // 3️⃣ حالا که scope معتبر است → interpolation
        this._bindTextNodes(element, scope);

        // 4️⃣ terminal فقط traversal را می‌بندد
        if (isTerminal)
            return;

        const children = Array.from(element.children);
        for (const child of children) {
            this.detectElements(child, scope, controller, false);
        }
    }

    _bindTextNodes(element, scope) {
        const expressionService = this._services['expressionService'];

        Array.from(element.childNodes).forEach(node => {
            if (node.nodeType === Node.TEXT_NODE) {
                const text = node.nodeValue;
                const matches = text.match(/{{\s*([^}]+)\s*}}/g);
                if (!matches) return;

                const newText = text.replace(/{{\s*([^}]+)\s*}}/g, (_, expr) => {
                    return expressionService.evaluateExpression(expr.trim(), scope) ?? '';
                });

                node.nodeValue = newText;
            }
        });
    }

    bindAttributes(element, scope) {
        const expressionService = this._services['expressionService'];

        for (let attr of element.attributes) {
            const matches = attr.value.match(/{{\s*([^}]+)\s*}}/g);
            if (!matches) continue;

            const newValue = attr.value.replace(/{{\s*([^}]+)\s*}}/g, (_, expr) => {
                return expressionService.evaluateExpression(expr.trim(), scope) ?? '';
            });

            element.setAttribute(attr.name, newValue);
        }
    }

    //#endregion

    //#region Reactive Data & Paths

    listenTo(expr, scope, callback, ...args) {
        const { parent, key } = this.resolvePropReference(expr, scope);
        if (!parent) return;

        let propMap = this._watchMap.get(parent);
        if (!propMap) {
            propMap = new Map();
            this._watchMap.set(parent, propMap);
        }

        if (!propMap.has(key)) {
            propMap.set(key, new Set());
            this._createReactive(parent, key);
        }

        const getter = () => parent[key];

        const watcher = this._createWatcher(
            getter,
            (n, o) => callback(n, o, ...args)
        );

        propMap.get(key).add(watcher);
    }

    resolvePropReference(path, scope) {
        if (!path) return { parent: undefined, key: undefined };

        let parent = scope;

        // Item[Something]
        const bracketMatch = path.match(/^(\w+)\[([^\[\]]+)\]$/);
        if (bracketMatch) {
            parent = scope[bracketMatch[1]];
            if (!parent || typeof parent !== 'object') return { parent: undefined, key: undefined };

            let inner = bracketMatch[2].trim();

            // Item["Name"] | Item['Name']
            if (
                (inner.startsWith('"') && inner.endsWith('"')) ||
                (inner.startsWith("'") && inner.endsWith("'"))
            ) {
                inner = inner.slice(1, -1);
            }
            // Item[Column.Name]
            else {
                const expressionService = this._services['expressionService'];
                inner = expressionService.evaluateExpression(inner, scope);

            }

            return { parent, key: inner };
        }

        // Item.Key.SubKey
        const keys = path.split('.');
        const key = keys.pop();

        for (const k of keys) {
            if (parent && typeof parent === 'object') {
                parent = parent[k];
            } else {
                return { parent: undefined, key: undefined };
            }
        }

        return { parent, key };
    }

    updateModel(expr, value, notify = true, scope = this.scopeRoot) {
        if (!expr) return;

        const { parent, key } = this.resolvePropReference(expr, scope);
        if (parent) parent[key] = value;

        if (notify) this.notifyResolved(parent, key);
    }

    notify(expr, scope = this.scopeRoot) {
        const { parent, key } = this.resolvePropReference(expr, scope);

        const propMap = this._watchMap.get(parent);
        if (!propMap) return;

        const watchers = propMap.get(key);
        if (!watchers) return;

        watchers.forEach(w => w.run());
    }

    notifyResolved(parent, key) {
        const propMap = this._watchMap.get(parent);
        if (!propMap) return;

        const watchers = propMap.get(key);
        if (!watchers) return;

        watchers.forEach(w => w.run());
    }

    //#endregion

    //#region Broadcasting Data

    on(eventName, callback) {
        if (!this._events.has(eventName))
            this._events.set(eventName, new Set());

        const listeners = this._events.get(eventName);
        listeners.add(callback);

        return () => listeners.delete(callback);
    }

    broadcast(eventName, ...args) {
        const listeners = this._events.get(eventName);
        if (!listeners) return;

        for (const cb of Array.from(listeners)) {
            try {
                cb(...args);
            } catch (err) {
                console.error(`Error in listener for "${eventName}":`, err);
            }
        }
    }

    clearListeners(eventName) {
        if (eventName)
            this._events.delete(eventName);
        else
            this._events.clear();
    }

    //#endregion

    //#region Private Methods

    _createReactive(parent, key) {
        const self = this;

        if (typeof parent !== "object" || parent === null) {
            return false;
        }

        // اگر قبلاً reactive شده، خارج شو
        const desc = Object.getOwnPropertyDescriptor(parent, key);
        if (desc && desc.get && desc.set && desc.get[this._reactiveFlag]) {
            return true;
        }

        const value = parent[key];

        if (typeof value !== "object" || value === null) {
            let internalValue = value;

            const getter = function () {
                return internalValue;
            };
            getter[this._reactiveFlag] = true;

            Object.defineProperty(parent, key, {
                configurable: true,   // ⬅ بسیار مهم
                enumerable: true,
                get: getter,
                set(val) {
                    if (internalValue !== val) {
                        internalValue = val;
                        self.notifyResolved(parent, key);
                    }
                }
            });
        }
    }

    _createWatcher(getter, callback) {
        const w = {
            getter,
            callback,
            oldValue: getter(),
            run() {
                const v = this.getter();
                this.callback(v, this.oldValue);
                this.oldValue = v;
            }
        };

        return w;
    }

    _resolveDependencies(fn, context) {
        context = context ?? this;
        const paramNames = this._getParamNames(fn);
        const resolved = paramNames.map(name => name == 'app' ? context : this._services[name]);

        return resolved;
    }

    _getParamNames(func) {
        const fnStr = func.toString().replace(/\/\*[\s\S]*?\*\//g, '');
        const result = fnStr.slice(fnStr.indexOf('(') + 1, fnStr.indexOf(')')).match(/([^\s,]+)/g);
        return result || [];
    }

    _getAttributesObject(el) {
        return Array.from(el.attributes).reduce((acc, a) => {
            acc[a.name.replace(/^data-/, '')] = a.value;
            return acc;
        }, {});
    }

    //#endregion
}