export class ModuleController {
    constructor(app, globalService, apiService, expressionService, actionService, dslEngine) {
        this.app = app;
        this.globalService = globalService;
        this.apiService = apiService;
        this.actionService = actionService;
        this.expressionService = expressionService;
        this.dslEngine = dslEngine;

        window.addEventListener("beforeunload", () => this.disconnectUser());
    }

    //#region Event Methods

    async onLoad(isDashboard, moduleId, connectionId) {
        this.moduleId = moduleId;
        this.connectionId = connectionId;

        this.scope = {
            form: {},
            pane: {},
            field: {}
        };

        const module = await this.apiService.getAsync("Module", "GetModule", {
            isDashboard: isDashboard,
            moduleId: moduleId,
            connectionId: connectionId,
            pageUrl: encodeURIComponent(document.URL)
        });
        module.data = module.data || {};

        this.dashboard = module.dashboard;
        this.variables = module.variables;

        // const data=this.decodeProtectedData(data.md) ?? {};
        for (const variable of module.variables) {
            const key = variable.VariableName;

            if (variable.VariableType == 'AppModel' && !module.data[key]) {
                this.scope[key] = this.scope[key] ?? {};

                for (const prop of variable.Properties) {
                    if (!this.scope[key].hasOwnProperty(prop.PropertyName))
                        this.scope[key][prop.PropertyName] = null;

                    this.app.listenTo(`${key}.${prop.PropertyName}`, this.scope, () => {
                        const value = this.expressionService.evaluateExpression(`${key}.${prop.PropertyName}`, this.scope);
                        this.broadcast(`${key}.${prop.PropertyName}`, { value });
                    })
                };
            } else {
                this.scope[key] = module.data[key];
                this.app.listenTo(key, this.scope, () => {
                    const value = this.expressionService.evaluateExpression(key, this.scope);
                    this.broadcast(key, { value });
                });
            }
        }

        // this.fields = this.decodeProtectedData(module.mf) ?? [];
        this.fields = module.fields ?? [];
        this.globalService.parseJsonItems(this.fields);

        this.controllerCache = {};

        this.fields.forEach(field => {
            this.scope.field[field.FieldName] = field;

            if (field.CanHaveValue && field.FieldValueProperty) {
                (field.ConditionalValues ?? []).forEach(fv => {
                    this.app.listenTo(fv.ValueExpression, this.scope, () => {
                        this.setFieldConditionalValue(field.Id);
                    });

                    if (fv.Conditions) {
                        this.app.listenTo(fv.Conditions, this.scope, () => {
                            this.setFieldConditionalValue(field.Id);
                        });
                    }
                });
            }

            if (field.DataSource && field.DataSource.Type == 2 && field.DataSource.VariableName) {
                const items = this.expressionService.evaluateExpression(field.DataSource.VariableName, this.scope);
                field.DataSource.Items = items;

                this.app.listenTo(field.DataSource.VariableName, this.scope, () => {
                    this.onFieldDataSourceChange(field.Id);
                });

                this.app.listenTo(`field.${field.FieldName}.DataSource.Items`, this.scope, () => {
                    this.onFieldDataSourceChange(field.Id);
                });
            }

            let controllerInstance = this.controllerCache[field.FieldType];
            if (!controllerInstance) {
                const ControllerClass = ComponentRegistry.resolve(field.FieldType);
                if (typeof ControllerClass === 'function') {
                    controllerInstance = new ControllerClass(this);
                    this.controllerCache[field.FieldType] = controllerInstance;
                }
            }

            if (controllerInstance && typeof controllerInstance.init === 'function') {
                try {
                    controllerInstance.init(field);
                } catch (error) {
                    console.error(error);
                }
            }
        });

        // this.actions = this.decodeProtectedData(module.ma);
        this.actions = module.actions;
        await this.callModuleActionsByEvent('OnPageLoad');

        //handle perloader
        let preloader;
        if (isDashboard)
            preloader = document.querySelector('[dashboard-preloader="true"]');
        else
            preloader = document.querySelector('[module-preloader="true"]');

        if (preloader)
            setTimeout(() => {
                preloader.remove();
            }, 100);

        return this.scope;
    }

    onFieldShowChange(fieldId) {
        const field = this.getFieldById(fieldId);
    }

    onFieldDataSourceChange(fieldId) {
        const field = this.getFieldById(fieldId);
        const items = this.expressionService.evaluateExpression(field.DataSource.VariableName, this.scope);

        field.DataSource.Items = items;

        this.app.updateModel(`field.${field.FieldName}.DataSource.Items`, field.DataSource.Items);
        this.app.broadcast(`onFieldDataSourceChange_${field.Id}`, { field: field });
    }

    //#endregion

    //#region Validate Methods

    async validateForm() {
        const fields = this.fields.filter(f => {
            return f.CanHaveValue || f.IsGroupField
        });

        this.scope.form.isValid = await this.validateFields(fields);

        return this.scope.form.isValid;
    }

    async validatePanes(paneNames) {
        if (!paneNames || !paneNames.length) return true;

        const results = await Promise.all(paneNames.map(pane => this.validatePane(pane)));
        return results.every(Boolean); // true if all are valid
    }

    async validatePane(paneName) {
        const fields = this.fields.filter(f => { return f.PaneName == paneName; });

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

        if (!field.CanHaveValue || !field.IsRequired || !field.FieldValueProperty) return true;

        const value = this.expressionService.evaluateExpression(field.FieldValueProperty, this.scope);
        const isEmpty = value === null || value === undefined || value === "";

        if (isEmpty) {
            field.isValid = false;
            field.requiredError = true;
        }

        if (field.isValid && field.Settings.ValidationMethod) {
            //field.isValid = await his.$deferredBroadcast(this, field.Settings.ValidationMethod, value);
        }

        field.isValid = field.isValid && (isEmpty || this.validateFieldPattern(field, value));

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

    //#region Actions Methods

    async callModuleActionsByEvent(event) {
        const actions = this.actions.filter(a => a.ExecuteInClientSide && !a.FieldId);
        if (actions.length)
            return await this.actions.callActionsByEvent(this, event, actions);
    }

    async callFieldActionsByEvent(fieldId, event, extraParams) {
        const actions = this.actions.filter(a => a.FieldId === fieldId);
        if (actions.length)
            return await this.actionService.callActionsByEvent(this, event, actions, extraParams);
    }

    async callFieldActions(fieldId, actionId, extraParams) {
        const actions = this.actions.filter(a => a.FieldId === fieldId);
        if (actions.length)
            return await this.actionService.callActions(this, actionId, actions, extraParams);
    }

    async callAction(actionId, extraParams) {
        const actions = this.actions.filter(a => a.Id === actionId);
        if (actions.length)
            return await this.actionService.callActions(this, actionId, actions, extraParams);
    }

    async callClientAction(actionId, extraParams) {
        return await this.actionService.callClientAction(this, [actionId], extraParams);
    }

    async callServerAction(actionId, extraParams) {
        return await this.actionService.callServerActions(this, [actionId], extraParams);
    }

    //#endregion

    //#region Field Methods

    getFieldById(fieldId) {
        const field = this.fields.find(f => { return f.Id == fieldId; });
        return field;
    }

    getFieldByName(fieldName) {
        const field = this.fields.find(f => { return f.FieldName == fieldName; });
        return field;
    }

    getGroupFields(groupId) {
        let fields = [];

        const findNestedFields = (groupId) => {
            const childs = this.fields.filter(f => { return f.ParentId == groupId; });
            fields.push(...childs);
            childs.filter(f => { return f.IsGroupField; }).map((group) => { findNestedFields(group.Id); });
        };

        findNestedFields(groupId);

        return fields;
    }

    setFieldConditionalValue(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.CanHaveValue) {
            field.ConditionalValues.forEach(fv => {
                const isTrue = expressionService.evaluateExpression(fv.Conditions, this.scope);
                if (typeof isTrue == 'string') isTrue = JSON.parse(value);

                if (isTrue)
                    field.Value = this.expressionService.evaluateExpression(fv.ValueExpression, this.scope);
            });
        }
    }

    showHideField(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.ShowConditions && field.ShowConditions.length) {
            const isTrue = expressionService.evaluateExpression(field.ShowConditions, this.scope);
            if (typeof isTrue == 'string') isTrue = JSON.parse(value);

            field.IsShow = isTrue;
        }
    }

    isFieldShow(field) {
        return field.IsShow && $(`*[data-field="${field.FieldName}"]`).length;
    }

    //#endregion

    //#region Other Methods

    on(eventName, callback) {
        this.app.on(eventName, callback)
    }

    broadcast(eventName, ...args) {
        this.app.broadcast(eventName, ...args);
    }

    updateModel(expr, value) {
        this.app.updateModel(expr, value);
    }

    //#endregion
}