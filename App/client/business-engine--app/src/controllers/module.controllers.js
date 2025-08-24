export class ModuleController {
    constructor(app, apiService, expressionService, actionService, globalService) {
        this.app = app;
        this.apiService = apiService;
        this.expressionService = expressionService;
        this.actionService = actionService;
        this.globalService = globalService;
    }

    //#region Event Methods

    async onLoad(moduleId, connectionId) {
        this.moduleId = moduleId;
        this.connectionId = connectionId;

        this.form = {};
        this.field = {};
        this.pane = {};
        this.controllerCache = {};

        this.Data = {};

        const data = await this.apiService.getAsync("Module", "GetModuleData", {
            moduleId: moduleId,
            connectionId: connectionId,
            pageUrl: document.URL
        });

        // this.fields = this.decodeProtectedData(data.mf) ?? [];
        this.fields = data.fields ?? [];
        this.globalService.parseJsonItems(this.fields);

        // const data=this.decodeProtectedData(data.md) ?? {};
        const moduleData = data.data ?? {};
        _.forEach(moduleData, (value, key) => {
            this.Data[key] = value;
        });

        this.fields.forEach(field => {
            field.actions = _.filter(this.actions, (a) => { return a.FieldId == field.Id; });

            if (field.CanHaveValue && field.FieldValueProperty) {
                _.forEach(field.ConditionalValues ?? [], (fv) => {
                    this.app.listenTo(this, fv.ValueExpression, setFieldConditionalValue, { fieldId: field.Id });

                    (fv.Conditions ?? []).forEach(condition => {
                        this.app.listenTo(this, condition.LeftExpression, setFieldConditionalValue, { fieldId: field.Id });
                        this.app.listenTo(this, condition.RightExpression, setFieldConditionalValue, { fieldId: field.Id });
                    });
                });
            }

            if (field.DataSource && field.DataSource.Type == 2 && field.DataSource.VariableName)
                this.app.listenTo(this, field.DataSource.VariableName, onFieldDataSourceChange, { fieldId: field.Id });

            (field.ShowConditions ?? []).forEach(condition => {
                this.app.listenTo(this, condition.LeftExpression, showHideField, { fieldId: field.Id });
                this.app.listenTo(this, condition.RightExpression, showHideField, { fieldId: field.Id });
            });

            this.field[field.FieldName] = field;

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

        // this.actions = this.decodeProtectedData(data.ma);
        this.actions = data.actions;

        const moduleActions = _.filter(this.actions, (a) => { return a.ExecuteInClientSide && !a.FieldId });
        if (moduleActions.length) await this.actionService.callActions(this, moduleActions, "OnPageLoad");

        $('.b-engine-module').addClass('is-loaded');

        this.pingConnection();
    }

    onFieldShowChange(fieldId) {
        const field = this.getFieldById(fieldId);
    }

    onFieldDataSourceChange(fieldId) {
        const field = this.getFieldById(fieldId);

        field.DataSource = { ...field.DataSource, ...this.Data[field.DataSource.VariableName] };

        this.$timeout(() => {
            if (field.DataSource !== field.OldDataSource) this.$scope.$broadcast(`onFieldDataSourceChange_${field.Id}`, { field: field });
            field.OldDataSource = angular.copy(field.DataSource);
        }, 200);
    }

    //#endregion

    //#region Validate Methods

    async validateForm() {
        const fields = _.filter(this.fields, (f) => {
            return f.CanHaveValue || f.IsGroupField
        });

        this.form.isValid = await this.validateFields(fields);

        return this.form.isValid;
    }

    async validatePanes(paneNames) {
        if (!paneNames || !paneNames.length) return true;

        const results = await Promise.all(paneNames.map(pane => this.validatePane(pane)));
        return results.every(Boolean); // true if all are valid
    }

    async validatePane(paneName) {
        let fields = _.filter(this.fields, (f) => {
            return f.PaneName == paneName;
        });

        _.filter(fields, (f) => { return f.IsGroupField; }).map((group) => {
            fields.push(...this.getGroupFields(group.Id));
        });

        this.pane[paneName] = this.pane[paneName] || {};
        this.pane[paneName].isValid = await this.validateFields(fields);

        return this.pane[paneName].isValid;
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

        const value = this.expressionService.evaluateExpression(field.FieldValueProperty, this);
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
        const actions = _.filter(this.actions, (a) => { return a.FieldId == fieldId });
        await this.actionService.callActions(this, actions, event);
    }

    //#endregion

    //#region Field Methods

    getFieldById(fieldId) {
        const field = _.find(this.fields, (f) => {
            return f.Id == fieldId;
        });

        return field;
    }

    getFieldByName(fieldName) {
        const field = _.find(this.fields, (f) => {
            return f.FieldName == fieldName;
        });

        return field;
    }

    getGroupFields(groupId) {
        let fields = [];

        const findNestedFields = (groupId) => {
            const childs = _.filter(this.fields, (f) => { return f.ParentId == groupId; });
            fields.push(...childs);

            _.filter(childs, (f) => { return f.IsGroupField; }).map((group) => {
                findNestedFields(group.Id);
            });
        };

        findNestedFields(groupId);

        return fields;
    }

    setFieldConditionalValue(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.CanHaveValue) {
            field.ConditionalValues.forEach(fv => {
                if (this.expressionService.checkConditions(fv.Conditions, this.Data)) {
                    const expressionTree = this.expressionService.parseExpression(fv.ValueExpression, this.Data);
                    field.Value = this.expressionService.evaluateExpressionTree(expressionTree, this.Data);
                }
            });
        }
    }

    showHideField(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.ShowConditions && field.ShowConditions.length) {
            field.IsShow = this.expressionService.checkConditions(field.ShowConditions, this.Data);
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
            const watch = _.find(this.watches, (w) => {
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

    raiseWatches() {
        _.forEach(this.watches, (w) => {
            this.$scope.$watch(w.property, () => {
                _.forEach(w.callbacks, (item) => {
                    if (typeof this[item.callback] == "function")
                        this[item.callback].apply(this, item.params);
                });
            }, true);
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