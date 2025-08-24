export class App {
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
        this.services[name] = new def();
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

    async bootstrap(appElement) {
        const ctrlEls = appElement.querySelectorAll('[b-controller]');
        for (const ctrlElement of ctrlEls) { 
            const attrs = this.getAttributesObject(ctrlElement);
            const ctrlName = attrs['b-controller'];
            const CtrlClass = this.controllers[ctrlName];

            if (!CtrlClass) throw new Error(`Controller ${ctrlName} not found`);

            const resolved = this.resolveDependencies(CtrlClass);
            const controller = new CtrlClass(...resolved);

            await controller.onLoad(attrs.module, attrs.connection);

            this.detectElements(ctrlElement, controller);
        };
    }

    detectElements(root, controller) {
        const tree = this.buildTreeNode(root);

        const render = (node) => {
            this.bindAttributes(node.element, controller);
            this.scanDirectives(node.element, controller);

            if (!node.element.hasAttribute('b-if') && !node.element.hasAttribute('b-for'))
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

    scanDirectives(element, controller) {
        for (let attr of element.attributes) {
            const dirName = attr.name;
            if (this.directives[dirName]) {
                const attrs = this.getAttributesObject(element);
                const resolved = this.resolveDependencies(this.directives[dirName]);
                const defFn = this.directives[dirName].apply(null, resolved);

                defFn.compile(attrs, element, controller);
            }
        }
    }

    createReactive(parent, key, basePath) {
        const self = this;
        const dot = basePath ? '.' : '';
        const expr = `${basePath}${dot}${key}`;
        const value = parent[key];

        if (typeof value === "object" && value !== null) {
            parent[key] = new Proxy(value, {
                get(target, prop) {
                    return target[prop];
                },
                set(target, prop, val) {
                    target[prop] = val;
                    self.notify(expr);
                    return true;
                }
            });
        }
        else {
            let internalValue = value;
            Object.defineProperty(parent, key, {
                get() {
                    return internalValue;
                },
                set(val) {
                    var t = parent;
                    internalValue = val;
                    self.notify(expr);
                }
            });
        }
    }

    bindTextNodes(root, controller) {
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
                node.nodeValue = this.evalText(node.nodeValue, controller);
            };
            update();
        });
    }

    bindAttributes(element, controller) {
        for (let attr of element.attributes) {
            const matches = attr.value.match(/{{\s*([^}]+)\s*}}/g);
            if (matches) {
                matches.forEach(m => {
                    const expr = m.replace(/[{}]/g, '').trim();
                    const expressionService = this.services['expressionService'];
                    const value = attr.value.replace(/{{\s*([^}]+)\s*}}/g,
                        (_, g1) => expressionService.evaluateExpression(expr, controller));

                    element.setAttribute(attr.name, value);
                });
            }
        }
    }

    evalText(template, controller) {
        return template.replace(/{{\s*([^}]+)\s*}}/g, (_, key) => {
            try {
                return this.evalInContext(key, controller);
            } catch {
                return '';
            }
        });
    }

    evalInContext(obj, expr) {
        return Function(...Object.keys(obj), `return ${expr}`)(...Object.values(obj));
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
                watchers.push({ watchers: w });
            }
        }

        return watchers;
    }

    listenTo(obj, expr, callback, ...args) {
        const { parent, key, basePath } = this.resolvePropReference(obj, expr);
        if (!parent) return;

        // اطمینان از اینکه مسیر reactive شده
        if (!this.watchMap.has(expr)) {
            this.watchMap.set(expr, new Set());
            this.createReactive(parent, key, basePath);
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

    resolvePropReference(obj, path) {
        const keys = path.split('.');
        const lastKey = keys.pop();
        const basePath = keys.join('.'); // مسیر تا parent

        const parent = keys.reduce((acc, key) => acc[key], obj);

        return { parent, key: lastKey, basePath };
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

    cloneClassInstance(obj) {
        const clone = Object.create(Object.getPrototypeOf(obj));
        Object.assign(clone, obj);
        return clone;
    }
}
