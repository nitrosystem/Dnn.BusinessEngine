import { GlobalSettings } from "../configs/global.settings";

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

    onInitModule(dnnModuleId, moduleId, moduleName, connectionId) {
        this.module = {
            dnnModuleId: dnnModuleId,
            moduleId: moduleId,
            connectionId: connectionId,
            moduleName: moduleName
        };

        this.onPageLoad();
    }

    onPageLoad() {
        this.$scope._Form = {};
        this.$scope.Field = {};
        this.$scope.Pane = {};

        this.watches = [];

        this.apiService.get("Module", "GetModuleData", {
            moduleId: this.module.moduleId,
            connectionId: this.module.connectionId,
            pageUrl: document.URL
        }).then((data) => {
            this.fields = data.Fields || [];

            this.$scope.variables = data.Variables || [];

            _.forEach(data.Data ?? {}, (value, key) => {
                this.$scope[key] = value;
                this.$scope.variables.push({ VariableName: key });
            });

            this.$scope.variables.push(
                { VariableName: "_Form", IsSystemVariable: true },
                { VariableName: "_Field", IsSystemVariable: true },
                { VariableName: "_Pane", IsSystemVariable: true },
                { VariableName: "_PageParam", IsSystemVariable: true }
            );

            this.actions = data.Actions || [];
            _.map(this.actions, (action) => {
                action.Settings = JSON.parse(action.Settings);
            });

            var dataSourceFields = [];

            _.forEach(this.fields, (field) => {
                this.globalService.parseJsonItems(field.Settings);

                field.Actions = [];
                _.filter(this.actions, (a) => { return a.FieldId == field.Id; }).map((action) => { field.Actions.push(action); });

                this.appendWatches("Field." + field.FieldName + ".IsShow", "onFieldShowChange", field.Id);

                if (field.IsValuable) {
                    this.appendWatches("Field." + field.FieldName + ".Value", "onFieldValueChange", field.Id);

                    this.setFieldValue(field.Id);

                    _.forEach(field.FieldValues, (fv) => {
                        this.appendWatches(fv.ValueExpression, "setFieldValue", field.Id);

                        _.forEach(fv.Conditions, (c) => {
                            this.appendWatches(c.LeftExpression, "setFieldValue", field.Id);
                        });
                    });
                }

                if (field.DataSource) {
                    if (field.DataSource.RunServiceClientSide) dataSourceFields.push(field.Id);

                    this.appendWatches(`Field.${field.FieldName}.DataSource`, "onFieldDataSourceChange", field.Id);
                }

                this.showHideField(field.Id);

                _.forEach(field.ShowConditions, (c) => {
                    this.appendWatches(c.LeftExpression, "showHideField", field.Id);
                    this.appendWatches(c.RightExpression, "showHideField", field.Id);
                });
                this.$scope._Field[field.FieldName] = field;
            });

            this.assignScopeData(data.Data);

            this.raiseWatches();

            this.$timeout(() => {
                var clientActions = _.filter(this.actions, (a) => { return !a.IsServerSide });

                this.actionService.callActions(clientActions, this.module.moduleId, null, "OnPageLoad", this.$scope).then((data) => {
                    this.$timeout(() => {
                        _.forEach(dataSourceFields, (fieldId) => { this.getFieldDataSource(fieldId); })
                    }, 1000);
                });

                this.$scope.loadedModule = true;
                this.completedForm = true;

                $('.b-engine-module').addClass('is-loaded');
            });
        });
    }

    assignScopeData(serverObjects) {
        _.forEach(serverObjects, (data) => {
            this.globalService.parseJsonItems(data);
        })

        _.forOwn(serverObjects, (value, key) => {
            if (key == 'Field') {
                //این قسمت باید بازنویسی شود و با فعال کردن کدهای زیر آبجکت فیلد دچار مشکل می شود
                // _.forOwn(value, (fieldData, fieldName) => {
                //     this.$scope._Field[fieldName] = {...this.$scope._Field[fieldName], ...fieldData };
                //     _.filter(this.fields, (f) => { return f.FieldName == fieldName }).map((f) => {
                //         this.fields[this.fields.indexOf(f)] = this.$scope._Field[fieldName]
                //     });
                // });
            } else {
                if (this.$scope[key])
                    this.$scope[key] = { ...this.$scope[key], ...value };
                else
                    this.$scope[key] = value;
            }
        });
    }

    onFieldValueChange(fieldId) {
        var field = this.getFieldById(fieldId);
        if (field /*&& this.completedForm*/) {
            if (field.Settings.SaveValueIn) {
                var match = /(\w+)([\.\[].[^*+%\-\/\s()]*)?/gm.exec(
                    field.Settings.SaveValueIn
                );
                if (match) {
                    var model = this.$parse(match[0]);
                    model.assign(this.$scope, field.Value);
                }
            } else {
                this.$scope._Field[field.FieldName].Value = field.Value;
                this.$scope._Form[field.FieldName] = field.Value;
            }

            if (field.BeValidate) {
                const fields = !field.IsGroup ? [field] : this.getFieldChilds(field);
                this.validateFields(fields);
            }

            this.$timeout(() => {
                if (field.Value !== field.OldValue) this.$scope.$broadcast(`onFieldValueChange_${field.Id}`, { field: field });
                field.OldValue = angular.copy(field.Value);
            }, 200);

            if (field.Actions && field.Actions.length)
                this.callActionByEvent(this.actions, fieldId, "OnFieldValueChange");
        } else
            console.warn(
                "Field not found. Method: onFieldValueChange, FieldId: " + fieldId
            );
    }

    setFieldValue(fieldId) {
        var field = this.getFieldById(fieldId);
        if (field) {
            this.$scope._Form[field.FieldName] = field.Value;

            _.forEach(field.FieldValues, (fv) => {
                if (!fv.Conditions ||
                    !fv.Conditions.length ||
                    this.expressionService.checkConditions(fv.Conditions, this.$scope)
                ) {
                    const value = this.expressionService.parseExpression(
                        fv.ValueExpression,
                        this.$scope,
                        fv.ExpressionParsingType
                    );

                    if (
                        value &&
                        typeof value == "string" &&
                        field.Settings.AllowMultiple &&
                        !field.IsJsonValue
                    )
                        field.Value = value.split(",");
                    else field.Value = value;

                    this.$scope._Form[field.FieldName] = field.Value;
                }
            });
        } else
            console.warn(
                "Field not found. Method: setFieldValue, FieldId: " + fieldId
            );
    }

    onFieldShowChange(fieldId) {
        var field = this.getFieldById(fieldId);
        if (field) {
            if (!field.Settings.EnableSetNullValueWhenFieldIsHide && !field.IsShow && field.Value) {
                field.ValueBackup = _.clone(field.Value);
                delete field.Value;
            }
            else if (!field.Settings.EnableSetNullValueWhenFieldIsHide && field.IsShow && field.ValueBackup) {
                field.Value = _.clone(field.ValueBackup);
                delete field.ValueBackup;
            }

            // this.$scope._Form[field.FieldName] = field.Value;
        } else
            console.warn("Field not found. Method: onFieldShowChange, FieldId: " + fieldId);
    }

    showHideField(fieldId) {
        var field = this.getFieldById(fieldId);
        if (field) {
            if (field.ShowConditions && field.ShowConditions.length) {
                field.IsShow = this.expressionService.checkConditions(
                    field.ShowConditions,
                    this.$scope
                );
            }
        } else
            console.warn(
                "Field not found. Method: showHideField, FieldId: " + fieldId
            );
    }

    getFieldDataSource(fieldId, pageIndex) {
        var field = this.getFieldById(fieldId);
        if (field && field.DataSource) {
            var datasource = _.clone(field.DataSource);

            if (datasource.Type == "2") {
                this.apiService.post("Module", "GetFieldDataSource", {
                    ModuleId: this.module.moduleId,
                    ConnectionId: this.module.connectionId,
                    FieldId: fieldId,
                    PageIndex: pageIndex || 1,
                    PageSize: this.expressionService.parseExpression(field.Settings.PageSize, this.$scope),
                    Form: this.$scope._Form,
                    PageUrl: document.URL,
                })
                    .then((data) => {
                        this.$scope._Field[data.FieldName].DataSource = data.DataSource;
                    });
            } else if (datasource.Type == "actions") {
                const listName = field.DataSource.ListName;
                const value = this.expressionService.parseExpression(listName, this.$scope);

                field.DataSource = field.DataSource || {};
                field.DataSource.Items = field.DataSource.Items || [];
                field.DataSource.Items = value;
            }
        } else {
            console.warn("Field not found. Method: getFieldDataSource, FieldId: " + fieldId);
        }
    }

    onFieldDataSourceChange(fieldId) {
        var field = this.getFieldById(fieldId);

        this.$timeout(() => {
            if (field.DataSource !== field.OldDataSource) this.$scope.$broadcast(`onFieldDataSourceChange_${field.Id}`, { field: field });
            field.OldDataSource = field.DataSource;
        }, 200);
    }

    validateForm() {
        const defer = this.$q.defer();

        const fields = _.filter(this.fields, (f) => {
            return (f.IsValuable || f.IsGroup) && this.isFieldShow(f);
        });

        this.validateFields(fields).then((isValid) => {
            this.$scope._Form._IsValid = isValid;

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
            return f._PaneName == paneName && this.isFieldShow(f);
        });

        _.filter(fields, (f) => {
            return f.IsGroup;
        }).map((g) => {
            fields = fields.concat(this.getFieldChilds(g));
        });

        this.validateFields(fields).then((isValid) => {
            this.$scope._Pane[paneName] = this.$scope._Pane[paneName] || {};
            this.$scope._Pane[paneName].IsValid = isValid;

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
            this.$deferredBroadcast(
                this.$scope,
                field.Settings.ValidationMethod
            ).then((isValid) => {
                field.IsValid = isValid && this.validateFieldValidationPattern(field);
                field.RequiredError = !field.IsValid;

                defer.resolve(field.IsValid);
            });
        } else if (field.IsRequired) {
            field.IsValid = !(
                field.Value === null ||
                field.Value === undefined ||
                field.Value === ""
            );

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

    callActionsByEvent(target, eventName, includeServerSide, sender) {
        var moduleId;
        var fieldId;
        var actions;

        if (includeServerSide)
            actions = this.actions;
        else
            actions = _.filter(this.actions, (a) => { return !a.IsServerSide });

        if (target == 'module') {
            moduleId = sender ? sender : this.module.moduleId;
        }

        if (target == 'field') {
            fieldId = sender;
        }

        return this.actionService.callActions(
            actions,
            moduleId,
            fieldId,
            eventName,
            this.$scope
        );
    }

    callActionByEvent(actions, fieldId, eventName) {
        const defer = this.$q.defer();

        this.actionService.callActions(
            actions,
            this.module.moduleId,
            fieldId,
            eventName,
            this.$scope
        ).then(() => {
            defer.resolve();
        });

        return defer.promise;
    }

    /*------------------------------------*/
    /* Watch Fields or Variables Changed   */
    /*------------------------------------*/
    appendWatches(expression, callback, ...params) {
        if (!expression || typeof expression != "string" || !callback) return;

        const matches = expression.match(/(\w+)([\.\[].[^*+%\-\/\s()]*)?/gm);
        _.forEach(matches, (match) => {
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
            },
                true
            );
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
}