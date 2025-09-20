export class ModuleController {
    constructor(app, globalService, apiService, expressionService, actionService) {
        this.app = app;
        this.globalService = globalService;
        this.apiService = apiService;
        this.actionService = actionService;
        this.expressionService = expressionService;
    }

    //#region Event Methods

    async onLoad(module, moduleId, connectionId) {
        this.moduleId = moduleId;
        this.connectionId = connectionId;

        this.scope = {
            moduleId: moduleId,
            connectionId: connectionId,
            form: {},
            pane: {},
            field: {}
        };

        this.controllerCache = {};

        if (!module) {
            module = await this.apiService.getAsync("Module", "GetModule", {
                moduleId: moduleId,
                connectionId: connectionId,
                pageUrl: document.URL
            });
        }

        this.scope.variables = module.variables;

        // const data=this.decodeProtectedData(data.md) ?? {};
        for (const key in module.data) {
            const variable = module.variables.find(v => v.VariableName === key);

            if (!variable) continue;

            if (variable.VariableType == 'AppModel' && !module.data[key]) {
                this.scope[key] = {};
                variable.Properties.forEach(prop => {
                    this.scope[key][prop.PropertyName] = this.globalService.getDefaultValueByType(undefined, prop.PropertyType);
                });
            }
            else if (this.globalService.isSystemType(variable.VariableType))
                this.scope[key] = this.globalService.convertToRealType(module.data[key], variable.VariableType);
            else
                this.scope[key] = module.data[key];
        }

        // this.fields = this.decodeProtectedData(module.mf) ?? [];
        this.fields = module.fields ?? [];
        this.globalService.parseJsonItems(this.fields);

        this.fields.forEach(field => {
            this.scope.field[field.FieldName] = field;

            if (field.CanHaveValue && field.FieldValueProperty) {
                (field.ConditionalValues ?? []).forEach(fv => {
                    this.app.listenTo(fv.ValueExpression, this.scope, setFieldConditionalValue, { fieldId: field.Id });

                    if (fv.Conditions)
                        this.app.listenTo(fv.Conditions, this.scope, setFieldConditionalValue, { fieldId: field.Id });
                });
            }

            if (field.DataSource) {
                if (field.DataSource.Type == 2 && field.DataSource.VariableName)
                    this.app.listenTo(field.DataSource.VariableName, this.scope, onFieldDataSourceChange, { fieldId: field.Id });
            }

            let controllerInstance = this.controllerCache[field.FieldType];
            if (!controllerInstance) {
                const ControllerClass = ComponentRegistry.resolve(field.FieldType);
                if (typeof ControllerClass === 'function') {
                    controllerInstance = new ControllerClass(this);
                    this.controllerCache[field.FieldType] = controllerInstance;
                }
            }

            if (controllerInstance && typeof controllerInstance.init === 'function')
                controllerInstance.init(field);
        });

        // this.actions = this.decodeProtectedData(module.ma);
        this.actions = module.actions;

        const moduleActions = this.actions.filter(a => { return a.ExecuteInClientSide && !a.FieldId });
        if (moduleActions.length) await this.actionService.callActions('OnPageLoad', moduleActions, this.scope);

        $('.b-engine-module').addClass('is-loaded');

        this.pingConnection();

        return this.scope;
    }

    onFieldShowChange(fieldId) {
        const field = this.getFieldById(fieldId);
    }

    onFieldDataSourceChange(fieldId) {
        const field = this.getFieldById(fieldId);

        field.DataSource = { ...field.DataSource, ...this.scope[field.DataSource.VariableName] };

        this.$timeout(() => {
            if (field.DataSource !== field.OldDataSource) this.$this.scope.$broadcast(`onFieldDataSourceChange_${field.Id}`, { field: field });
            field.OldDataSource = angular.copy(field.DataSource);
        }, 200);
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
            field.isValid = await his.$deferredBroadcast(this, field.Settings.ValidationMethod, value);
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

    async callActionsByEvent(fieldId, event) {
        const actions = this.actions.filter(a => { return a.FieldId == fieldId });
        await this.actionService.callActions(event, actions, this.scope);
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

    /*------------------------------------*/
    /* Watch Fields or Variables Changed   */
    /*------------------------------------*/
    appendWatches(expression, callback, ...params) {
        if (!expression || typeof expression != "string" || !callback) return;

        const matches = expression.match(/(\w+)([\.\[].[^*+%\-\/\s()]*)?/gm);
        (matches ?? []).forEach(match => {
            const propertyPath = match;
            const watch = this.watches.find(w => {
                return w.property == propertyPath;
            });
            if (!watch) {
                this.watches.push({
                    property: propertyPath,
                    callbacks: [{
                        callback: callback,
                        params: params,
                    },],
                });
            } else {
                watch.callbacks.push({
                    callback: callback,
                    params: params,
                });
            }
        });
    }

    async pingConnection() {
        await new Promise(res => setTimeout(res, 40000));

        try {
            await this.apiService.postAsync("Module", "PingConnection", {
                ConnectionId: this.connectionId
            });
        } catch (error) {
            console.error(error);
        }

        this.pingConnection();
    }

    //#endregion
}