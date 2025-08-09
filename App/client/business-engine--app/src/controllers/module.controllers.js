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

        // const data=this.decodeProtectedData(data.md) ?? {};
        const moduleData = data.data ?? {};
        _.forEach(moduleData, (value, key) => {
            this.data[key] = value;
        });

        _.forEach(this.fields, (field) => {
            field.Actions = [];
            _.filter(this.actions, (a) => { return a.FieldId == field.Id; }).map((action) => { field.Actions.push(action); });

            // this.appendWatches("field." + field.FieldName + ".IsShow", "onFieldShowChange", field.Id);

            if (field.IsValuable) {
                field.ignoreWatchForChangingValue = true;

                if (field.FieldValueProperty) {
                    this.appendWatches("$.field." + field.FieldName + ".Value", "onFieldValueChange", field.Id);
                    this.appendWatches("$.data." + field.FieldValueProperty, "onVariableValueChange", field.Id, field.FieldValueProperty);
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

    onFieldValueChange(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.IsValuable && field.FieldValueProperty && !field.ignoreWatchForChangingValue) {
            _.set(this.data, field.FieldValueProperty, field.Value);

            // if (field.BeValidate) {
            //     const fields = !field.IsGroup ? [field] : this.getFieldChilds(field);
            //     this.validateFields(fields);
            // }

            if (field.Actions && field.Actions.length)
                this.callActionByEvent(fieldId, "OnFieldValueChange");

            this.$timeout(() => {
                this.$scope.$broadcast(`onFieldValueChange_${field.Id}`, { field: field });
            });
        }
    }

    onVariableValueChange(fieldId, property) {
        const field = this.getFieldById(fieldId);
        if (field.IsValuable) {
            const value = _.get(this.data, property);

            field.ignoreWatchForChangingValue = true;
            field.Value = value;

            this.$timeout(() => delete field.ignoreWatchForChangingValue);
        }
    }

    setFieldConditionalValue(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.IsValuable) {
            _.forEach(field.FieldValues, (fv) => {
                debugger
                if (this.expressionService.checkConditions(fv.Conditions, this.data)) {
                    const expressionTree = this.expressionService.parseExpression(fv.ValueExpression, this.data);
                    field.Value = this.expressionService.evaluateExpressionTree(expressionTree, this.data);
                }
            });
        }
    }

    onFieldShowChange(fieldId) {
        const field = this.getFieldById(fieldId);
    }

    showHideField(fieldId) {
        const field = this.getFieldById(fieldId);
        if (field.ShowConditions && field.ShowConditions.length) {
            field.IsShow = this.expressionService.checkConditions(field.ShowConditions, this.data);
        }
    }

    onFieldDataSourceChange(fieldId) {
        const field = this.getFieldById(fieldId);

        field.DataSource = { ...field.DataSource, ...this.data[field.DataSource.VariableName] };

        this.$timeout(() => {
            if (field.DataSource !== field.OldDataSource) this.$scope.$broadcast(`onFieldDataSourceChange_${field.Id}`, { field: field });
            field.OldDataSource = angular.copy(field.DataSource);
        }, 200);
    }

    validateForm() {
        const defer = this.$q.defer();

        const fields = _.filter(this.fields, (f) => {
            return (f.IsValuable || f.IsGroup) && this.isFieldShow(f);
        });

        this.validateFields(fields).then((isValid) => {
            this.form.isValid = isValid;

            defer.resolve(isValid);
        });

        return defer.promise;
    }

    validatePanes(buffer, defer) {
        if (!defer) defer = this.$q.defer();

        if (!buffer.length) {
            defer.resolve();
        } else {
            const paneName = buffer[0];
            this.validatePane(paneName).then(() => {
                buffer.shift();
                this.validatePanes(buffer, defer);
            });
        }

        return defer.promise;
    }

    validatePane(paneName) {
        const defer = this.$q.defer();

        var fields = _.filter(this.fields, (f) => {
            return f.PaneName == paneName && this.isFieldShow(f);
        });

        _.filter(fields, (f) => {
            return f.IsGroup;
        }).map((g) => {
            fields = fields.concat(this.getFieldChilds(g));
        });

        this.validateFields(fields).then((isValid) => {
            this.pane[paneName] = this.pane[paneName] || {};
            this.pane[paneName].IsValid = isValid;

            defer.resolve(isValid);
        });

        return defer.promise;
    }

    validateGroups(buffer, defer) {
        if (!defer) defer = this.$q.defer();

        if (!buffer.length) {
            defer.resolve();
        } else {
            const group = this.getFieldById(buffer[0]);
            this.validateGroup(group).then(() => {
                buffer.shift();
                this.validateGroups(buffer, defer);
            });
        }

        return defer.promise;
    }

    validateGroup(group) {
        const defer = this.$q.defer();

        const fields = !group.IsGroup ? [group] : this.getFieldChilds(group);
        this.validateFields(fields).then((isValid) => {
            group.IsValid = isValid;

            defer.resolve(isValid);
        });

        return defer.promise;
    }

    validateFields(buffer, defer, isNotValid) {
        if (!defer) defer = this.$q.defer();

        if (!buffer.length) {
            defer.resolve(!isNotValid);
        } else {
            const field = buffer[0];
            this.validateField(field).then((isValid) => {
                if (!isValid) console.log(field);
                buffer.shift();
                this.validateFields(buffer, defer, !isValid ? true : isNotValid);
            });
        }

        return defer.promise;
    }

    validateField(field) {
        const defer = this.$q.defer();

        field.IsPatternValidate = true;
        field.BeValidate = true;
        field.Validated = true;

        if (!field.IsValuable && !field.Settings.ValidationMethod) {
            field.IsValid = true;
            defer.resolve(field.IsValid);
        } else if (field.Settings.ValidationMethod) {
            this.$deferredBroadcast(this.$scope, field.Settings.ValidationMethod).then((isValid) => {
                field.IsValid = isValid && this.validateFieldValidationPattern(field);
                field.RequiredError = !field.IsValid;

                defer.resolve(field.IsValid);
            });
        } else if (field.IsRequired) {
            field.IsValid = !(field.Value === null || field.Value === undefined || field.Value === "");
            field.IsValid = field.IsValid && this.validateFieldValidationPattern(field);

            field.RequiredError = !field.IsValid;

            defer.resolve(field.IsValid);
        } else {
            field.IsValid = this.validateFieldValidationPattern(field);

            defer.resolve(field.IsValid);
        }

        return defer.promise;
    }

    validateFieldValidationPattern(field) {
        if (field.Value && field.Settings && field.Settings.ValidationPattern) {
            try {
                field.IsValid = new RegExp(field.Settings.ValidationPattern).test(
                    field.Value
                );
            } catch (e) {
                field.IsValid = false;
            }
        } else field.IsValid = true;

        return field.IsValid;
    }

    isFieldShow(field) {
        return field.IsShow && $(`*[data-field="${field.FieldName}"]`).length;
    }

    /*------------------------------------*/
    /* Action Methods  */
    /*------------------------------------*/
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

    getFieldChilds(field) {
        var fields = [];

        const findNestedFields = (group) => {
            const childs = _.filter(this.fields, (f) => {
                return f.ParentId == group.FieldId && this.isFieldShow(f);
            });

            fields = fields.concat(childs);

            _.filter(childs, (f) => {
                return f.IsGroup;
            }).map((g) => {
                findNestedFields(g);
            });
        };

        findNestedFields(field);

        return fields;
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

    decodeProtectedData(base64String) {
        if (!base64String) return null;

        // Base64 decode
        const binaryString = atob(base64String);
        const binaryData = new Uint8Array(binaryString.length);

        for (let i = 0; i < binaryString.length; i++) {
            binaryData[i] = binaryString.charCodeAt(i);
        }

        // GZip decompress
        const decompressed = pako.inflate(binaryData, { to: 'string' });

        // Parse JSON
        return JSON.parse(decompressed);
    }
}