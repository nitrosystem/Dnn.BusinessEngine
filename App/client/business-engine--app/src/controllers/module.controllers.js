export class ModuleController {
    constructor(
        app,
        globalService,
        apiService,
        expressionService
    ) {
        this._app = app;
        this._globalService = globalService;
        this._apiService = apiService;
        this._expressionService = expressionService;

        window.addEventListener('beforeunload', () => {
            const payload = {
                ConnectionId: this.connectionId,
                ModuleId: this.moduleId,
            };

            const blob = new Blob(
                [JSON.stringify(payload)],
                { type: 'application/json' }
            );

            navigator.sendBeacon(
                '/API/BusinessEngineApp/Module/DisconnectUser',
                blob
            );
        });
    }

    //#region Event Methods

    async onLoad(isDashboard, moduleId, connectionId) {
        this.moduleId = moduleId;
        this.connectionId = connectionId;
        this._controllerCache = {};

        const module = await this._apiService.getApi(
            'BusinessEngineApp',
            'Module',
            'GetModule',
            {
                isDashboard: isDashboard,
                moduleId: moduleId,
                connectionId: connectionId,
                pageUrl: encodeURIComponent(document.URL)
            });

        // this._fields = this.decodeProtectedData(module.mf) ?? [];
        this._globalService.parseJsonItems(module.fields);
        this._fields = module.fields ?? [];
        this._actions = module.actions;
        this._variables = module.variables;
        this.dashboard = module.dashboard;
        this.scope = {
            form: {},
            pane: {},
            field: {},
            fn: {
                set: (...args) => {
                    for (let i = 0; i < args.length; i += 2) {
                        const key = args[i];
                        if (typeof (key) !== 'string') continue;
                        const value = args[i + 1];
                        this.set(key, value)
                    }
                }
            }
        };

        // const data=this.decodeProtectedData(data.md) ?? {};
        //Parse Variables
        for (const variable of module.variables) {
            const key = variable.VariableName;
            if (variable.VariableType === 'AppModel') {
                this.scope[key] = {};
                for (const prop of variable.Properties) {
                    this.scope[key][prop.PropertyName] = module.data[key]
                        ? module.data[key][prop.PropertyName]
                        : null;
                }
            }
            else {
                this.scope[key] = module.data[key];
            }
        }

        //Parse Fields 
        for (const field of this._fields) {
            this.scope.field[field.FieldName] = field;

            //Parse Field Data Source
            if (field.DataSource && field.DataSource.Type == 1 && field.DataSource.VariableName) {
                const items = this.get(field.DataSource.VariableName) || [];
                field.DataSource.Items = items.map(item => ({ ...item }));

                this.listenTo(field.DataSource.VariableName, (newVal, oldVal) => {
                    if (newVal !== oldVal) {
                        field.DataSource.Items = (newVal ?? []).map(item => ({ ...item }));
                        this.notify(`field.${field.FieldName}.DataSource.Items`);
                    }
                });
            }

            //Live validate
            if (field.CanHaveValue && field.IsRequired && field.FieldValueProperty)
                this.listenTo(field.FieldValueProperty, async (newVal, oldVal) => {
                    if (field.isValidated && newVal !== oldVal) await this.validateField(field);
                });

            //Initilize Field
            this._initField(field);
        }

        //Detect field display changed
        this.on('onElementDisplayChange', (args) => {
            const element = args.element;
            const isShow = args.value;
            if (isShow) {
                if (element.hasAttribute('__b')) {
                    const fieldId = element.getAttribute('__b');
                    const field = this.getFieldById(fieldId);
                    if (field && !field.__ignoreReinit)
                        this._globalService.nextAnimationFrame(this._initField(field));
                }
                else if (element.hasAttribute('data-pane')) {
                    const paneName = element.getAttribute('data-pane');
                    const fields = this._fields.filter(f => { return f.PaneName === paneName; });
                    for (const field of fields ?? []) {
                        if (!field.__ignoreReinit) this._globalService.nextAnimationFrame(() => this._initField(field));
                    }
                }
            }
        });

        //Handle perloader
        const preloader = isDashboard
            ? document.querySelector('[dashboard-preloader="true"]')
            : document.querySelector('[module-preloader="true"]');
        if (preloader)
            this._globalService.nextAnimationFrame(() => {
                preloader.remove();
            });

        return this.scope;
    }

    //#endregion

    //#region For & Fields

    //#region Field Methods

    _initField(field) {
        try {
            let controllerInstance = this._controllerCache[field.FieldType];
            if (!controllerInstance) {
                const ControllerClass = ComponentRegistry.resolve(field.FieldType);
                if (typeof ControllerClass === 'function') {
                    controllerInstance = new ControllerClass(this, this._globalService);
                    this._controllerCache[field.FieldType] = controllerInstance;
                }
            }

            if (controllerInstance && typeof controllerInstance.init === 'function') {
                const fieldelement = document.querySelector(`[__b='${field.Id}']`);
                controllerInstance.init(field, fieldelement);

            }
        } catch (error) {
            console.error(`Init "${field.FieldName}" field failed :`, error);
        }
    }

    getFieldById(fieldId) {
        return this._fields.find(f => f.Id === fieldId);
    }

    getGroupFields(groupId) {
        let fields = [];
        const findNestedFields = (groupId) => {
            const childs = this._fields.filter(f => f.ParentId === groupId);
            if (childs.length) fields.push(...childs);

            childs
                .filter(f => f.IsGroupField)
                .map(g => findNestedFields(g.Id));
        };

        findNestedFields(groupId);
        return fields;
    }

    //#endregion

    //#region Validate Methods

    async validateForm() {
        const fields = this._fields.filter(f => {
            return !f.ParentId && (f.CanHaveValue || f.IsGroupField)
        });

        this.scope.form.isValid = await this.validateFields(fields);

        if (!this.scope.form.isValid) {
            const elem = document.querySelector('[__validated-error]');
            if (elem) elem.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        return this.scope.form.isValid;
    }

    async validatePanes(paneNames) {
        if (!paneNames || !paneNames.length) return true;

        const results = await Promise.all(paneNames.map(pane => this.validatePane(pane)));
        return results.every(Boolean); // true if all are valid
    }

    async validatePane(paneName) {
        const fields = this._fields.filter(f => f.PaneName === paneName);
        fields
            .filter(f => f.IsGroupField)
            .map(g => fields.push(...this.getGroupFields(g.Id)));

        this.scope.pane[paneName] = this.scope.pane[paneName] || {};
        this.scope.pane[paneName].isValid = await this.validateFields(fields);
        return this.scope.pane[paneName].isValid;
    }

    async validateGroups(groupIds) {
        if (!groupIds?.length) return true;

        const results = await Promise.all(groupIds.map(groupId => this.validateGroup(groupId)));
        return results.every(Boolean); // true if all are valid
    }

    async validateGroup(groupId) {
        const fields = this.getGroupFields(groupId);
        return await this.validateFields(fields);
    }

    async validateFields(fields) {
        if (!fields || !fields.length) return true;

        const results = await Promise.all(fields.map(field => this.validateField(field)));
        return results.every(Boolean); // true if all are valid
    }

    async validateField(field) {
        field.isValidated = true;
        field.isValid = true;
        field.requiredError = false;
        field.patternError = false;

        if (field.HiddenConditions && this.checkConditions(field.HiddenConditions))
            return true;

        if (field.validateMethod) {
            field.isValid = await field.validateMethod(field);
        }
        else {
            if (!field.CanHaveValue || !field.IsRequired || !field.FieldValueProperty) return true;

            const value = this._expressionService.evaluateExpression(field.FieldValueProperty, this.scope);
            const isEmpty = value === null || value === undefined || value === '';
            if (isEmpty) {
                field.isValid = false;
                field.requiredError = true;
            }

            field.isValid = field.isValid && (isEmpty || this.validateFieldPattern(field, value));
        }

        const fieldelement = document.querySelector(`[__b="${field.Id}"]`);
        if (fieldelement && !field.IsGroupField) {
            fieldelement.toggleAttribute('__validated-error', !field.isValid);
        }

        if (!field.isValid) console.warn(field.FieldName);

        return field.isValid;
    }

    validateFieldPattern(field, value) {
        let patternIsValid = true;
        try {
            patternIsValid = new RegExp(field.Settings.ValidationPattern, 'su').test(value);
        } catch (error) {
            console.error(`Validate pattern "${field.FieldName}" field failed :`, error);

            patternIsValid = false;
        }

        field.patternError = !patternIsValid;
        return patternIsValid;
    }

    //#endregion

    //#endregion

    //#region Actions

    hasFieldAction(fieldId, event) {
        return this._actions.some(a => a.FieldId === fieldId && a.Event === event);
    }

    getActionByName(actionName) {
        return this._actions.find(a => a.ActionName === actionName);
    }

    getFieldActionsByEvent(fieldId, event) {
        return this._actions.filter(a => a.FieldId === fieldId && a.Event === event);
    }

    async callActionByName(fieldId, actionName, extraParams) {
        const action = this.getActionByName(actionName);
        if (action) return await this.callAction(fieldId, action.Id, action.Event, extraParams)
    }

    async callAction(fieldId, actionId, event, extraParams) {
        const postData = {};
        for (const key in this.scope) {
            const variable = this._variables.find(v => v.VariableName === key);
            if (!variable || variable.Scope == 1) continue;

            postData[key] = this.scope[key];
        }

        let isSuccess = true;
        let data;

        try {
            const payload = await this._apiService.postApi(
                'BusinessEngineApp',
                'Module',
                'ExecuteAction',
                {
                    FieldId: fieldId,
                    ActionId: actionId,
                    Event: event,
                    ConnectionId: this.connectionId,
                    ModuleId: this.moduleId,
                    PageUrl: document.URL,
                    Data: postData,
                    ExtraParams: extraParams,
                });

            if (payload.IsRequiredToUpdateData) {
                for (const key in payload.Data ?? {}) {
                    const newValue = payload.Data[key];
                    const variable = this._variables.find(v => v.VariableName === key && v.Scope === 0);
                    if (variable && this.scope[key] !== newValue) {
                        if (this.scope[key] && variable.VariableType === 'AppModel') {
                            for (const prop of variable.Properties) {
                                this.scope[key][prop.PropertyName] = payload.Data[key][prop.PropertyName];
                                this.notifyResolved(this.scope[key], prop.PropertyName);
                            }
                        }
                        else
                            this.set(key, newValue, true);
                    }
                }
            }

            for (const result of payload.Results ?? []) {
                if (result.status === 2) isSuccess = false;

                if (result.IsRedirectable && result.RedirectUrl) {
                    window.location.href = result.RedirectUrl;
                    return;
                }

                if (result.Data) data = result.Data;
            }

            return { isSuccess: isSuccess, isError: !isSuccess, data }

        } catch (error) {
            console.error(`Call action failed :`, error);

            return { isError: true, error: error?.message }
        }
    }

    //#endregion

    //#region App Methods

    on(eventName, callback) {
        this._app.on(eventName, callback)
    }

    broadcast(eventName, ...args) {
        this._app.broadcast(eventName, ...args);
    }

    listenTo(expr, callback) {
        this._app.listenTo(expr, this.scope, callback);
    }

    watch(expr, callback) {
        this._app.watch(expr, this.scope, callback);
    }

    checkConditions(expr, scope = this.scope) {
        return this._expressionService.evaluateCondition(expr, scope);
    }

    get(expr, scope = this.scope) {
        return this._expressionService.evaluateExpression(expr, scope);
    }

    set(expr, value, notify = false, scope = this.scope) {
        this._app.updateModel(expr, value, notify, scope);

        if (notify) this._app.notify(expr);
    }

    notify(expr) {
        this._app.notify(expr);
    }

    notifyResolved(parent, key) {
        this._app.notifyResolved(parent, key);
    }

    resolveByPath(expr, context) {
        return this._app.resolvePropReference(expr, context ?? this.scope);
    }

    registerDslCommand(name, handler) {
        this._dslEngineService.registerCommand(name, handler);
    }

    runDslCommand(command, context) {
        this._dslEngineService.run(command, context);
    }

    compile(html, scope = this.scope) {
        const element = document.createElement('div');
        element.setHTMLUnsafe(html);
        this._app.detectElements(element, scope, this, false);
        return element;
    }

    async getApi(moduleName, controller, methodName, params, headers = {}) {
        return await this._apiService.getApi(moduleName, controller, methodName, params, headers);
    }

    //#endregion
}