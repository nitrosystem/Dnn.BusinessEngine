export class BusinessEngineApp {
    constructor() {
        this.services = {};
        this.controllers = {};
        this.directives = {};

        this.watchers = [];
        this.watchMap = new Map();
        this.queue = new Set();
        this.flushing = false;

        this._events = new Map();
    }

    service(name, def) {
        const resolved = this.resolveDependencies(def);
        this.services[name] = new def(...resolved);
        return this;
    }

    controller(name, def) {
        this.controllers[name] = def;
        return this;
    }

    directive(name, def) {
        this.directives[name] = def;
        return this;
    }

    filter(name, def) {
        const expressionService = this.services['expressionService'];
        expressionService.filters[name] = def;
        return this;
    }

    async bootstrap(appElement) {
        const attrs = this.getAttributesObject(appElement);
        const ctrlName = attrs['b-controller'];
        const CtrlClass = this.controllers[ctrlName];

        const isDashboard = attrs.dashboard === 'True';
        const moduleId = attrs.module;
        const connectionId = attrs.connection;

        if (!CtrlClass || !moduleId || !connectionId)
            throw new Error(`Controller ${ctrlName} not found`);

        const isolatedApp = this.createIsolatedContext();
        const resolved = this.resolveDependencies(CtrlClass, isolatedApp);
        const controller = new CtrlClass(...resolved);
        const scope = await controller.onLoad(isDashboard, moduleId, connectionId);

        isolatedApp.scopeRoot = scope;
        isolatedApp.controller = controller;
        isolatedApp.detectElements(appElement, scope, controller, true);
    }

    createIsolatedContext() {
        const isolated = Object.create(Object.getPrototypeOf(this));

        isolated.services = this.services;
        isolated.controllers = this.controllers;
        isolated.directives = this.directives;

        isolated.watchers = [];
        isolated.watchMap = new Map();
        isolated.queue = new Set();
        isolated.flushing = false;

        isolated._events = new Map();

        return isolated;
    }

    // ---------------------------
    // Detect & Compile Directives
    // ---------------------------
    detectElements(element, scope, controller, isRoot = true, skipSelfDirection = "") {
        if (!element || !element.attributes)
            return;

        // جلوگیری از ورود به کنترلر جدید (رفتار AngularJS)
        if (!isRoot && element.hasAttribute('b-controller'))
            return;

        // ابتدا attributes را bind کن
        this.bindAttributes(element, scope);

        const attrs = element.attributes;
        const directives = [];

        // جمع‌آوری دایرکتیوها
        for (let i = 0; i < attrs.length; i++) {
            const attr = attrs[i];
            if (attr.name === skipSelfDirection) continue;

            const defFactory = this.directives[attr.name];
            if (defFactory) {
                const resolved = this.resolveDependencies(defFactory);
                const def = defFactory(...resolved);
                directives.push({ name: attr.name, value: attr.value, def });
            }
        }

        // اگر دایرکتیو داشت: اجرا بر اساس priority
        if (directives.length > 0) {
            directives.sort((a, b) => (b.def.priority || 0) - (a.def.priority || 0));

            for (const dir of directives) {
                if (typeof dir.def.compile === "function") {

                    const stop = dir.def.compile(
                        this.getAttributesObject(element),
                        element,
                        scope,
                        controller
                    );

                    // --- رفتار صحیح terminal (مشابه AngularJS) ---
                    if (dir.def.terminal) {
                        // فرزندان را **پردازش نکن**
                        // فقط خود دایرکتیو اگر نیاز داشت detectElements صدا می‌زند
                        return;
                    }

                    // اگر دایرکتیو گفت پردازش ادامه نده
                    if (stop === false)
                        return;
                }
            }
        }

        // حالا TextNodes را bind کن
        this.bindTextNodes(element, scope);

        // --- بسیار مهم: کپی کردن children قبل از تغییر DOM ---
        const children = Array.from(element.children);

        // ادامه پردازش فرزندان
        for (const child of children) {
            this.detectElements(child, scope, controller, false);
        }
    }

    // ---------------------------
    // Bind Text Nodes مستقیم
    // ---------------------------
    bindTextNodes(element, scope) {
        const expressionService = this.services['expressionService'];

        Array.from(element.childNodes).forEach(node => {
            if (node.nodeType === Node.TEXT_NODE) {
                const text = node.nodeValue;
                const matches = text.match(/{{\s*([^}]+)\s*}}/g);
                if (!matches) return;

                const newText = text.replace(/{{\s*([^}]+)\s*}}/g, (_, expr) => {
                    return expressionService.evaluateExpression(expr.trim(), scope);
                });

                node.nodeValue = newText;
            }
        });
    }

    // ---------------------------
    // Bind Attributes مستقیم
    // ---------------------------
    bindAttributes(element, scope) {
        const expressionService = this.services['expressionService'];

        for (let attr of element.attributes) {
            const matches = attr.value.match(/{{\s*([^}]+)\s*}}/g);
            if (!matches) continue;

            const newValue = attr.value.replace(/{{\s*([^}]+)\s*}}/g, (_, expr) => {
                return expressionService.evaluateExpression(expr.trim(), scope) ?? '';
            });

            element.setAttribute(attr.name, newValue);
        }
    }

    createReactiveSimilarVue(parent, key, expr) {
        const self = this;
        const value = parent[key];

        // جلوگیری از wrap دوباره
        if (value && value.__isReactive) return value;

        if (typeof value === "object" && value !== null) {
            const proxy = new Proxy(value, {

                get(target, prop, receiver) {
                    // محافظت از Symbol ها و props خطرناک
                    if (
                        typeof prop === "symbol" ||
                        prop === "__isReactive" ||
                        prop === "constructor"
                    ) {
                        return Reflect.get(target, prop, receiver);
                    }

                    // دسترسی به مقدار معمولی
                    const result = Reflect.get(target, prop, receiver);

                    return result;
                },

                set(target, prop, val, receiver) {
                    const oldVal = target[prop];
                    const isNew = oldVal !== val;

                    const result = Reflect.set(target, prop, val, receiver);

                    if (!isNew) return result;

                    // اگر آرایه است و متدهای تغییر ساختار را صدا زدند، notify یکبار زده شود
                    if (Array.isArray(target)) {
                        if (["push", "pop", "shift", "unshift", "splice", "sort", "reverse"].includes(prop)) {
                            self.notify(expr);
                            return result;
                        }

                        // index set → reactive update
                        if (!isNaN(prop)) {
                            self.notify(expr + "[" + prop + "]");
                            return result;
                        }
                    }

                    // object property update
                    self.notify(expr + "." + String(prop));

                    return result;
                }
            });

            // علامت برای جلوگیری از wrap دوباره
            proxy.__isReactive = true;

            parent[key] = proxy;
            return;
        }

        // -------------------------------
        // Primitive Values
        // -------------------------------
        let internalValue = value;

        Object.defineProperty(parent, key, {
            get() {
                return internalValue;
            },
            set(val) {
                if (internalValue !== val) {
                    internalValue = val;
                    self.notify(expr);
                }
            }
        });
    }

    createReactiveOld(parent, key, expr) {
        const self = this;
        const value = parent[key];

        if (value && value.__isReactive) return value;

        if (typeof value === "object" && value !== null) {
            parent[key] = new Proxy(value, {
                get(target, prop, receiver) {
                    if (typeof prop === "symbol" || prop === "constructor")
                        return Reflect.get(target, prop, receiver);

                    return target[prop];
                },
                set(target, prop, val, receiver) {
                    const oldVal = target[prop];
                    const success = Reflect.set(target, prop, val); //replaced with target[prop] = val;

                    if (success && oldVal !== val && typeof val === "object") {
                        self.notify(expr + "." + prop);
                    }

                    return success;
                }
            });
        } else {
            let internalValue = value;
            Object.defineProperty(parent, key, {
                get() {
                    return internalValue;
                },
                set(val) {
                    if (internalValue !== val) {
                        internalValue = val;
                        self.notify(expr);
                    }
                }
            });
        }
    }

    createReactive(parent, key, expr) {
        const self = this;
        const value = parent[key];

        if (typeof value !== "object" || value === null) {
            let internalValue = value;

            Object.defineProperty(parent, key, {
                get() {
                    return internalValue;
                },
                set(val) {
                    if (internalValue !== val) {
                        internalValue = val;
                        self.notify(expr);
                    }
                }
            });

            return;
        }
    }

    updateModel(expr, value) {
        if (!expr) return;

        if (value !== undefined && this.scopeRoot) {
            try {
                const segments = expr.split('.');
                const lastKey = segments.pop();
                let parent = this.scopeRoot;

                for (const key of segments) {
                    if (parent && typeof parent === 'object')
                        parent = parent[key];
                    else return;
                }

                if (parent && lastKey in parent)
                    parent[lastKey] = value;
            } catch (e) {
                console.warn('updateModel error:', e);
            }
        }

        this.notify(expr);
    }

    notify(propName) {
        this.queue.add(propName);

        if (!this.flushing) {
            this.flushing = true;
            Promise.resolve().then(() => this.digest());
        }
    }

    digest() {
        this.queue.forEach(prop => {
            const watchersbyPath = this.getWatchersForPath(prop);
            watchersbyPath.forEach(({ watchers }) => {
                for (const w of watchers ?? []) {
                    const newVal = w.getter();
                    w.callback(newVal, w.oldValue);
                    w.oldValue = newVal;
                }
            });
        });

        this.queue.clear();

        this.flushing = false;
    }

    getWatchersForPath(path) {
        const segments = path.split('.');
        const watchers = [];

        for (let i = 1; i <= segments.length; i++) {
            const subPath = segments.slice(0, i).join('.');
            const w = this.watchMap.get(subPath);
            if (w) {
                watchers.push({ path: subPath, watchers: w });
            }
        }

        for (const [key, value] of this.watchMap.entries()) {
            if (key.startsWith(path + '.') && !watchers.some(w => w.path === key)) {
                watchers.push({ path: key, watchers: value });
            }
        }

        return watchers;
    }

    listenTo(expr, scope, callback, ...args) {
        const { parent, key } = this.resolvePropReference(expr, scope);
        if (!parent) return;

        if (!this.watchMap.has(expr)) {
            this.watchMap.set(expr, new Set());
            this.createReactive(parent, key, expr);
        }

        const getter = () => parent[key];

        this.watchMap.get(expr).add({
            getter,
            callback: (newVal, oldVal) => callback(newVal, oldVal, ...args),
            oldVal: getter()
        });
    }

    resolvePropReference(path, scope) {
        const keys = (path ?? '').split('.');
        const lastKey = keys.pop();

        let parent = scope;
        for (const key of keys) {
            if (parent && typeof parent === 'object' && key in parent) {
                parent = parent[key];
            } else {
                parent = undefined;
                break;
            }
        }

        return { parent, key: lastKey };
    }

    resolveDependencies(fn, context) {
        context = context ?? this;
        const paramNames = this.getParamNames(fn);
        const resolved = paramNames.map(name => name == 'app' ? context : this.services[name]);

        return resolved;
    }

    getParamNames(func) {
        const fnStr = func.toString().replace(/\/\*[\s\S]*?\*\//g, ''); // پاک کردن کامنت
        const result = fnStr.slice(fnStr.indexOf('(') + 1, fnStr.indexOf(')')).match(/([^\s,]+)/g);
        return result || [];
    }

    getAttributesObject(el) {
        return Array.from(el.attributes).reduce((acc, a) => {
            acc[a.name.replace(/^data-/, '')] = a.value;
            return acc;
        }, {});
    }

    cloneClassInstance(scope) {
        const clone = Object.create(Object.getPrototypeOf(scope));
        Object.assign(clone, scope);
        return clone;
    }

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
}