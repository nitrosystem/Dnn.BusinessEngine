export class ModuleController {
    constructor(
        app,
        globalService,
        apiService,
        expressionService,
        dslEngineService
    ) {
        this._app = app;
        this._globalService = globalService;
        this._apiService = apiService;
        this._expressionService = expressionService;
        this._dslEngineService = dslEngineService;

        window.addEventListener("beforeunload", () => {
            const payload = {
                ConnectionId: this._connectionId,
                ModuleId: this._moduleId,
            };

            const blob = new Blob(
                [JSON.stringify(payload)],
                { type: "application/json" }
            );

            navigator.sendBeacon(
                "/API/BusinessEngine/Module/DisconnectUser",
                blob
            );
        });
    }

    //#region Event Methods

    async onLoad(isDashboard, moduleId, connectionId) {
        this._moduleId = moduleId;
        this._connectionId = connectionId;
        this._controllerCache = {};

        const module = await this._apiService.getAsync("Module", "GetModule", {
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
            field: {}
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

            //Parse Field Conditional Values
            if (field.CanHaveValue && field.FieldValueProperty) {
                for (const fv of field.ConditionalValues ?? []) {
                    if (fv.ValueExpression) {
                        const parts = this._expressionService.extractPropertyPaths(fv.ValueExpression) ?? [];
                        parts.forEach(item =>
                            this._app.listenTo(item, this.scope, () => {
                                this.setFieldConditionalValue(field.Id, fv.ValueExpression, fv.Conditions);
                            })
                        );

                        if (fv.Conditions) {
                            const parts = this._expressionService.extractPropertyPaths(fv.Conditions) ?? [];
                            parts.forEach(item =>
                                this._app.listenTo(item, this.scope, () => {
                                    this.setFieldConditionalValue(field.Id, fv.ValueExpression, fv.Conditions);
                                })
                            );
                        }
                    }
                }
            }

            //Parse Field Data Source
            if (field.DataSource && field.DataSource.Type == 2 && field.DataSource.VariableName) {
                const items = this.get(field.DataSource.VariableName) || [];
                field.DataSource.Items = items.map(item => ({ ...item }));
            }

            //Initilize Field
            let controllerInstance = this._controllerCache[field.FieldType];
            if (!controllerInstance) {
                const ControllerClass = ComponentRegistry.resolve(field.FieldType);
                if (typeof ControllerClass === 'function') {
                    controllerInstance = new ControllerClass(this, this._globalService);
                    this._controllerCache[field.FieldType] = controllerInstance;
                }
            }

            if (controllerInstance && typeof controllerInstance.init === 'function') {
                try {
                    controllerInstance.init(field);
                } catch (error) {
                    console.error(error);
                }
            }
        }

        //handle perloader
        let preloader;
        if (isDashboard)
            preloader = document.querySelector('[dashboard-preloader="true"]');
        else
            preloader = document.querySelector('[module-preloader="true"]');

        if (preloader)
            this._globalService.nextAnimationFrame(() => {
                preloader.remove();
            });

        return this.scope;
    }

    //#endregion

    //#region For & Fields

    //#region Field Methods

    getFieldById(fieldId) {
        return this._fields.find(f => f.Id === fieldId);
    }

    getGroupFields(groupId) {
        let fields = [];

        const findNestedFields = (groupId) => {
            const childs = this._fields.filter(f => { return f.ParentId == groupId; });
            fields.push(...childs);
            childs.filter(f => { return f.IsGroupField; }).map((group) => { findNestedFields(group.Id); });
        };

        findNestedFields(groupId);

        return fields;
    }

    setFieldConditionalValue(fieldId, expr, conditions) {
        const field = this.getFieldById(fieldId);
        const isTrue = this._expressionService.evaluateCondition(conditions, this.scope);
        if (isTrue) {
            const value = this._expressionService.evaluateExpression(expr, this.scope);
            this.set(field.FieldValueProperty, value, true);
        }
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
        const fields = this._fields.filter(f => { return f.PaneName == paneName; });

        fields.filter(f => { return f.IsGroupField; })
            .map(group => {
                fields.push(...this.getGroupFields(group.Id));
            });

        this.scope.pane[paneName] = this.scope.pane[paneName] || {};
        this.scope.pane[paneName].isValid = await this.validateFields(fields);

        return this.scope.pane[paneName].isValid;
    }

    async validateGroups(groupIds) {
        if (!groupIds || !groupIds.length) return true;

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

        if (field.IsGroupField && field.validateMethod)
            return await field.validateMethod(field);

        if (!field.CanHaveValue || !field.IsRequired || !field.FieldValueProperty) return true;

        const value = this._expressionService.evaluateExpression(field.FieldValueProperty, this.scope);
        const isEmpty = value === null || value === undefined || value === "";

        if (isEmpty) {
            field.isValid = false;
            field.requiredError = true;
        }

        field.isValid = field.isValid && (isEmpty || this.validateFieldPattern(field, value));

        const fieldelement = document.querySelector(`[data-fi="${field.Id}"]`);
        if (fieldelement) {
            fieldelement.toggleAttribute('__validated-error', !field.isValid);
        }

        return field.isValid;
    }

    validateFieldPattern(field, value) {
        let patternIsValid = true;

        try {
            patternIsValid = new RegExp(field.Settings.ValidationPattern).test(value);
        } catch (e) {
            patternIsValid = false;
        }

        field.patternError = !patternIsValid;

        return patternIsValid;
    }

    //#endregion

    //#endregion

    //#region Actions

    async callAction(fieldId, event, extraParams) {
        if (!this._actions.some(a => a.FieldId === fieldId)) return;

        const result = { status: 1 };
        const scope = this.scope;

        const postData = {};
        for (const key in scope) {
            const variable = this._variables.find(v => v.VariableName === key);
            if (!variable || variable.Scope == 1) continue;

            postData[key] = scope[key];
        }

        const responses = await this._apiService.postAsync("Module", "CallFieldAction", {
            FieldId: fieldId,
            Event: event,
            ConnectionId: this._connectionId,
            ModuleId: this._moduleId,
            PageUrl: document.URL,
            Data: postData,
            ExtraParams: extraParams,
        });

        for (const response of responses ?? []) {
            if (response.Status === 2 && response.ErrorException) {
                result.status = 2;
                result.isError = true;
                result.error = response.ErrorException;
                console.error(response.ErrorException)
                break;
            }

            if (!response.IsRequiredToUpdateData) continue;

            const moduleData = response.ModuleData;
            for (const key in moduleData ?? {}) {
                const newValue = moduleData[key];
                if (scope[key] !== newValue) {
                    const variable = this._variables.find(v => v.VariableName === key);
                    if (scope[key] && variable && variable.VariableType === 'AppModel') {
                        for (const prop of variable.Properties) {
                            scope[key][prop.PropertyName] = moduleData[key][prop.PropertyName];
                            this.notifyResolved(scope[key], prop.PropertyName);
                        }
                    }
                    else
                        this.set(key, newValue, true);
                }
            }
        }

        if (!result.isError) result.isSuccess = true;
        return result;
    }

    //#endregion

    //#region App Methods

    on(eventName, callback) {
        this._app.on(eventName, callback)
    }

    broadcast(eventName, ...args) {
        this._app.broadcast(eventName, ...args);
    }

    watch(expr, callback) {
        this._app.listenTo(expr, this.scope, callback);
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

    //#endregion
}