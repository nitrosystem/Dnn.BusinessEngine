export class BusinessEngineApp {
    constructor() {
        this.services = {};
        this.controllers = {};
        this.directives = {};

        this.watchers = [];
        this.watchMap = new Map();
        this.queue = new Set();
        this.flushing = false;
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

    async bootstrap(appElement, module, moduleId, connectionId) {
        const ctrlEls = appElement.querySelectorAll('[b-controller]');
        for (const ctrlElement of ctrlEls) {
            const attrs = this.getAttributesObject(ctrlElement);
            const ctrlName = attrs['b-controller'];
            const CtrlClass = this.controllers[ctrlName];

            if (!moduleId) moduleId = attrs.module;
            if (!connectionId) connectionId = attrs.connection;
            if (!CtrlClass || !moduleId || !connectionId)
                throw new Error(`Controller ${ctrlName} not found`);

            const resolved = this.resolveDependencies(CtrlClass);
            const controller = new CtrlClass(...resolved);
            const scope = await controller.onLoad(module, moduleId, connectionId);

            this.detectElements(ctrlElement, scope, controller);
        };
    }

    detectElements(root, scope, controller) {
        const tree = this.buildTreeNode(root);

        const render = (node) => {
            this.bindAttributes(node.element, scope);
            this.bindTextNodes(node.element, scope);
            this.scanDirectives(node.element, scope, controller);

            if (!node.element.hasAttribute('bb-if') && !node.element.hasAttribute('b-for'))
                node.children.forEach(child => render(child));
        }

        render(tree);
    }

    buildTreeNode(element) {
        const nodeObject = (el, children = []) => {
            return {
                element: el,
                children: children
            };
        };

        const processNode = (element) => {
            const children = Array.from(element.children || []).map(child => processNode(child));
            return nodeObject(element, children);
        };

        return processNode(element);
    }

    scanDirectives(element, scope, controller) {
        for (let attr of element.attributes) {
            const dirName = attr.name;
            if (this.directives[dirName]) {
                const attrs = this.getAttributesObject(element);
                const resolved = this.resolveDependencies(this.directives[dirName]);
                const defFn = this.directives[dirName].apply(null, resolved);

                defFn.compile(attrs, element, scope, controller);
            }
        }
    }

    createReactive(parent, key, expr) {
        const self = this;
        const value = parent[key];

        if (typeof value === "object" && value !== null) {
            parent[key] = new Proxy(value, {
                get(target, prop) {
                    return target[prop];
                },
                set(target, prop, val) {
                    const oldVal = target[prop];
                    target[prop] = val;

                    // فقط اگر کلید جدید بود یا آبجکت کامل عوض شد notify کن
                    if (oldVal !== val && typeof val === "object") {
                        self.notify(expr + "." + prop);
                    }
                    return true;
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

    bindTextNodes(root, scope) {
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null);
        const nodes = [];

        while (walker.nextNode()) {
            const text = walker.currentNode.nodeValue;
            const matches = text.match(/{{\s*([^}]+)\s*}}/g);
            (matches ?? []).forEach(m => {
                const expr = m.replace(/[{}]/g, '').trim();
                nodes.push({ node: walker.currentNode, expr });
            });
        }

        nodes.forEach(({ node, expr }) => {
            const update = () => {
                node.nodeValue = this.evalText(node.nodeValue, scope);
            };
            update();
        });
    }

    bindAttributes(element, scope) {
        for (let attr of element.attributes) {
            const matches = attr.value.match(/{{\s*([^}]+)\s*}}/g);
            if (matches) {
                matches.forEach(m => {
                    const expr = m.replace(/[{}]/g, '').trim();
                    const expressionService = this.services['expressionService'];
                    const value = attr.value.replace(/{{\s*([^}]+)\s*}}/g,
                        (_, g1) => expressionService.evaluateExpression(expr, scope));

                    element.setAttribute(attr.name, value);
                });
            }
        }
    }

    evalText(template, scope) {
        return template.replace(/{{\s*([^}]+)\s*}}/g, (_, key) => {
            try {
                return this.evalInContext(key, scope);
            } catch {
                return '';
            }
        });
    }

    evalInContext(expr, scope) {
        return Function(...Object.keys(scope), `return ${expr}`)(...Object.values(scope));
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

        // مسیرهای بالا‌دستی
        for (let i = 1; i <= segments.length; i++) {
            const subPath = segments.slice(0, i).join('.');
            const w = this.watchMap.get(subPath);
            if (w) {
                watchers.push({ path: subPath, watchers: w });
            }
        }

        // مسیرهای پایین‌دستی
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

        // اطمینان از اینکه مسیر reactive شده
        if (!this.watchMap.has(expr)) {
            this.watchMap.set(expr, new Set());
            this.createReactive(parent, key, expr);
        }

        // مقدار اولیه
        const getter = () => parent[key];

        // اضافه کردن واچ جدید به مجموعه
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

    resolveDependencies(fn) {
        const paramNames = this.getParamNames(fn);
        const resolved = paramNames.map(name => name == 'app' ? this : this.services[name]);

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
}