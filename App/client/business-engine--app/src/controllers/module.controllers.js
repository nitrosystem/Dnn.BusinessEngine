import { GlobalSettings } from "../configs/global.settings";
import pako from 'pako';

export class ModuleController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        $parse,
        $q,
        $compile,
        Upload,
        $deferredBroadcast,
        globalService,
        apiService,
        expressionService,
        actionService
    ) {
        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$parse = $parse;
        this.$q = $q;
        this.$compile = $compile;
        this.uploadService = Upload;
        this.$deferredBroadcast = $deferredBroadcast;
        this.globalService = globalService;
        this.apiService = apiService;
        this.expressionService = expressionService;
        this.actionService = actionService;

        this.globalSettings = GlobalSettings;

        this.$scope.moduleController = this;
        this.$scope.$rootScope = $rootScope;
    }

    //#region Event Methods

    async onInitModule(dnnModuleId, moduleId, connectionId) {
        this.module = {
            dnnModuleId: dnnModuleId,
            moduleId: moduleId,
            connectionId: connectionId,
        };

        await this.onPageLoad();
    }

    async onPageLoad() {
        this.form = {};
        this.field = {};
        this.pane = {};
        this.data = {};
        this.controllerCache = {};

        this.watches = [];

        const data = await this.apiService.get("Module", "GetModuleData", {
            moduleId: this.module.moduleId,
            connectionId: this.module.connectionId,
            pageUrl: document.URL
        });

        // this.fields = this.decodeProtectedData(data.mf) ?? [];
        this.fields = data.fields ?? [];

        this.globalService.parseJsonItems(this.fields);

        // const data=this.decodeProtectedData(data.md) ?? {};
        const moduleData = data.data ?? {};
        _.forEach(moduleData, (value, key) => {
            this.data[key] = value;
        });

        _.forEach(this.fields, (field) => {
            field.Actions = [];
            _.filter(this.actions, (a) => { return a.FieldId == field.Id; }).map((action) => { field.Actions.push(action); });

            // this.appendWatches("field." + field.FieldName + ".IsShow", "onFieldShowChange", field.Id);

            if (field.CanHaveValue) {
                field.ignoreWatchForChangingValue = true;

                if (field.FieldValueProperty) {
                    this.appendWatches("$.data." + field.FieldValueProperty, "onVariableValueChange", field.Id, field.FieldValueProperty);
                    this.appendWatches("$.field." + field.FieldName + ".Value", "onFieldValueChange", field.Id);
                }
                else if (field.FieldValues && field.FieldValues.length) {
                    _.forEach(field.FieldValues, (fv) => {
                        this.appendWatches(fv.ValueExpression, "setFieldConditionalValue", field.Id);

                        _.forEach(fv.Conditions, (c) => {
                            this.appendWatches(c.LeftExpression, "setFieldConditionalValue", field.Id);
                        });
                    });
                }

                this.$timeout(() => delete field.ignoreWatchForChangingValue);
            }

            if (field.DataSource && field.DataSource.Type == 2 && field.DataSource.VariableName)
                this.appendWatches(field.DataSource.VariableName, "onFieldDataSourceChange", field.Id);

            _.forEach(field.ShowConditions ?? [], (c) => {
                this.appendWatches(c.LeftExpression, "showHideField", field.Id);
            });

            this.field[field.FieldName] = field;

            let controllerInstance = this.controllerCache[field.FieldType];
            if (!controllerInstance) {
                const ControllerClass = ComponentRegistry.resolve(field.FieldType);
                if (typeof ControllerClass === 'function') {
                    controllerInstance = new ControllerClass(this); // فقط فرم رو پاس بده
                    this.controllerCache[field.FieldType] = controllerInstance;
                }
            }

            if (controllerInstance && typeof controllerInstance.init === 'function')
                controllerInstance.init(field);
        });

        this.raiseWatches();

        // this.actions = this.decodeProtectedData(data.ma);
        this.actions = data.actions;

        const moduleActions = _.filter(this.actions, (a) => { return a.ExecuteInClientSide && !a.FieldId });
        if (moduleActions.length) await this.actionService.callActions(moduleActions, "OnPageLoad", this);

        $('.b-engine-module').addClass('is-loaded');
    }

    async onFieldValueChange(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.CanHaveValue && field.FieldValueProperty && !field.ignoreWatchForChangingValue) {
            _.set(this.data, field.FieldValueProperty, field.Value);

            if (field.isValidated) {
                await this.validateField(field);
            }

            if (field.Actions && field.Actions.length)
                await this.callActionByEvent(fieldId, "OnFieldValueChange");

            this.$timeout(() => {
                this.$scope.$broadcast(`onFieldValueChange_${field.Id}`, { field: field });
            });
        }
    }

    onVariableValueChange(fieldId, property) {
        const field = this.getFieldById(fieldId);
        if (field.CanHaveValue) {
            const value = _.get(this.data, property);

            field.ignoreWatchForChangingValue = true;
            field.Value = value;

            this.$timeout(() => delete field.ignoreWatchForChangingValue);
        }
    }

    onFieldShowChange(fieldId) {
        const field = this.getFieldById(fieldId);
    }

    onFieldDataSourceChange(fieldId) {
        const field = this.getFieldById(fieldId);

        field.DataSource = { ...field.DataSource, ...this.data[field.DataSource.VariableName] };

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

        if (field.CanHaveValue && field.IsRequired &&
            (field.Value === null || field.Value === undefined || field.Value === "")
        ) {
            field.isValid = false;
            field.requiredError = true;
        }

        if (field.isValid && field.Settings.ValidationMethod) {
            field.isValid = await his.$deferredBroadcast(this, field.Settings.ValidationMethod);
        }

        field.isValid = field.isValid && this.validateFieldPattern(field);

        return field.isValid;
    }

    validateFieldPattern(field) {
        let patternIsValid = true;

        if (field.CanHaveValue && field.Value && field.Settings.ValidationPattern) {
            try {
                patternIsValid = new RegExp(field.Settings.ValidationPattern).test(field.Value);
            } catch (e) {
                patternIsValid = false;
            }
        }

        field.patternError = !patternIsValid;

        return patternIsValid;
    }

    //#endregion

    //#region Actions Methods

    callAction(actionName, params, ignoreChildActions) {
        const defer = this.$q.defer();

        this.actionService.callActionByName(
            actionName,
            params,
            this.actions,
            this.$scope,
            ignoreChildActions
        ).then((data) => {
            defer.resolve(data);
        }, (error) => {
            defer.reject(error);
        })

        return defer.promise;
    }

    callServerAction(actionName, params, ignoreChildActions) {
        const defer = this.$q.defer();

        this.actionService.callServerActionByName(
            actionName,
            params,
            this.actions,
            this.$scope,
            ignoreChildActions
        ).then((data) => {
            defer.resolve(data);
        }, (error) => {
            defer.reject(error);
        })

        return defer.promise;
    }

    callActionByActionId(actionId, params, ignoreChildActions) {
        _.filter(this.actions, (action) => {
            return action.ActionId == actionId;
        }).map((action) => {
            return this.callAction(action.ActionName, params, ignoreChildActions);
        });
    }

    async callActionsByEvent(fieldId, event) {
        const actions = _.filter(this.actions, (a) => { return a.FieldId == fieldId });
        await this.actionService.callActions(actions, event, this);
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
            _.forEach(field.FieldValues, (fv) => {
                if (this.expressionService.checkConditions(fv.Conditions, this.data)) {
                    const expressionTree = this.expressionService.parseExpression(fv.ValueExpression, this.data);
                    field.Value = this.expressionService.evaluateExpressionTree(expressionTree, this.data);
                }
            });
        }
    }

    showHideField(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.ShowConditions && field.ShowConditions.length) {
            field.IsShow = this.expressionService.checkConditions(field.ShowConditions, this.data);
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

        const matches = expression.match(/(\$\.)?(\w+)([\.\[].[^*+%\-\/\s()]*)?/gm);
        _.forEach(matches, (match) => {
            const expression = /(\$\.)?(\w+)([\.\[].[^*+%\-\/\s()]*)?/gm.exec(match);
            if (!expression[1]) match = '$.data.' + match;

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
        const delay = (ms) => new Promise(res => setTimeout(res, ms));

        try {
            await this.apiService.post("Module", "PingConnection", { ConnectionId: this.module.connectionId });
        } catch (error) {
            console.error(error);
        }

        await delay(40000);
        await this.pingConnection();
    }

    //#endregion
}