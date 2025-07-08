import { GlobalSettings } from "../../angular-configs/global.settings";

export class moduleBuilderService {
    constructor($filter, $q, $compile, $timeout, $deferredBroadcast, globalService, apiService, notificationService) {
        this.$filter = $filter;
        this.$q = $q;
        this.$compile = $compile;
        this.$timeout = $timeout;
        this.$deferredBroadcast = $deferredBroadcast;
        this.globalService = globalService;
        this.apiService = apiService;
        this.notifyService = notificationService;

        this.fieldLayoutTemplate;
        this.fieldsTemplate = {};
        this.fieldsScripts = {};
    }

    //#region Build Service Methods

    async renderModuleTemplate(module, fields, $scope, isFastBuilding) {
        const $defer = this.$q.defer();

        if (!module.Template) {
            this.notifyService.notify(`
                You must selected template for the module before building and rendering module...
            `);

            $defer.reject({ errorType: 'template', msg: 'no any Template set for the Module.' });

            return;
        }

        this.module = module;
        this.fields = fields;
        this.$scope = $scope;
        this.isFastBuilding = isFastBuilding;

        if (!this.isFastBuilding) {
            this.$scope.building = {
                index: 1,
                total: this.fields.length,
                step: 80 / this.fields.length,
                value: 0,
            };

            $(`#buildLogs${this.module.Id}`).html(``)
            $(`#moduleTemplateProgress${this.module.Id}`).css('width', '0');
            $(`#moduleTemplateFieldTotal${this.module.Id}`).text(this.$scope.building.total);

            await this.globalService.sleep(500);
        }

        this.$form = $(`<div>${module.LayoutTemplate}</div>`);

        _.map(this.fields, (f) => (f.AddedToPane = false));

        var panes = [];
        $(`<div>${module.LayoutTemplate}</div>`)
            .find("*[data-pane]").each((index, element) => {
                const paneName = $(element).data("pane");
                panes.push(paneName);
            });

        for (var pane in _.groupBy(this.fields, "PaneName")) {
            if (panes.indexOf(pane) == -1) panes.push(pane);
        }

        this.buffer = [];
        _.forEach(panes, (p) => {
            this.buffer.push({ PaneName: p });
        });

        this.processBuffer($defer).then((data) => {
            $defer.resolve(data);
        });

        return $defer.promise;
    }

    processBuffer($defer) {
        if (!this.buffer.length) {
            this.completed().then((data) => {
                $defer.resolve(data);
            });
        } else {
            const currentPane = this.buffer[0];
            const $pane = this.$form.find(
                '*[data-pane="' + currentPane.PaneName + '"]'
            );

            if ($pane.length) {
                const paneFields = _.orderBy(
                    _.filter(this.fields, (f) => {
                        return f.PaneName == currentPane.PaneName && !f.AddedToPane;
                    }), ["ViewOrder"], ["desc"]
                );

                this.parsePaneFields($pane, paneFields).then(() => {
                    this.buffer.shift();
                    this.processBuffer($defer);
                });
            } else {
                currentPane.tryForFindPane = currentPane.tryForFindPane || 0;
                if (currentPane.tryForFindPane < 10) {
                    currentPane.tryForFindPane++;

                    this.buffer.push(_.clone(currentPane));
                }

                this.buffer.shift();
                this.processBuffer($defer);
            }
        }

        return $defer.promise;
    }

    parsePaneFields($pane, fields) {
        const $defer = this.$q.defer();

        this.parseField($defer, $pane, fields, fields.length).then(() =>
            $defer.resolve()
        );

        return $defer.promise;
    }

    parseField($defer, $pane, fields, index) {
        if (index <= 0) {
            $defer.resolve();
        } else {
            this.$timeout(() => {
                var field = fields[index - 1];

                if (!field.IsShow) this.parseField($defer, $pane, fields, index - 1);

                this.getFieldUI(field).then(($fieldItem) => {
                    if (field.FieldTypeObject.IsContent) {
                        if (field.ShowConditions && field.ShowConditions.length) {
                            $fieldItem.find("*[data-field-content]").append(field.Settings.Content);
                        }
                        else {
                            $fieldItem = $(field.Settings.Content);
                        }
                    }

                    $pane.append($fieldItem);

                    field.AddedToPane = true;

                    if (!this.isFastBuilding) {
                        this.$scope.building.value = Math.round(this.$scope.building.step * this.$scope.building.index);
                        $(`#imgBuilding${this.module.Id}`).attr('src', field.FieldTypeObject.Icon);
                        $(`#moduleTemplateProgress${this.module.Id}`).css('width', `${this.$scope.building.value}%`);
                        $(`#moduleTemplateFieldIndex${this.module.Id}`).text(this.$scope.building.index);

                        this.$scope.building.index++;
                    }

                    this.parseField($defer, $pane, fields, index - 1);
                });
            })
        }

        return $defer.promise;
    }

    completed() {
        const $defer = this.$q.defer();

        $defer.resolve(this.$form.html());

        return $defer.promise;
    }

    async getFieldUI(field) {
        const $defer = this.$q.defer();

        await this.globalService.sleep(100);

        this.getBoardFieldItem(field).then((fieldLayoutTemplate) => {
            $defer.resolve($(fieldLayoutTemplate));
        });

        return $defer.promise;
    }

    getBoardFieldItem(field) {
        const $defer = this.$q.defer();

        if (!field.FieldTypeObject) {
            $defer.resolve("<div></div>");
            return;
        }

        const fieldTemplate = _.find(field.FieldTypeObject.Templates || [], (t) => { return t.TemplateName == field.Template; });
        const fieldTemplatePath = fieldTemplate ? (fieldTemplate.TemplatePath || "").replace("[EXTPATH]", GlobalSettings.modulePath + "extensions") : "";

        $(`#buildLogs${this.module.Id}`).append(`Loading ${field.FieldName} Field Template From ${fieldTemplatePath}...<br/>`)

        if (fieldTemplatePath) {
            var layout = field.Settings.IsCustomFieldLayout && field.Settings.CustomFieldLayoutModified && field.Settings.CustomFieldLayout
                ? field.Settings.CustomFieldLayout : this.getDefaultFieldLayoutTemplate(field);

            fetch(fieldTemplatePath + "?ver=" + GlobalSettings.version).then((stream) => {
                if (stream.status >= 400) throw new Error("Bad response from server");
                return stream.text();
            }).then((fieldHtml) => {
                fieldHtml = fieldHtml || "";

                layout = layout.replace(/\[FIELD-COMPONENT\]/g, fieldHtml);

                // ------------------ Parse IF Expression(s) Conditions --------------------
                while (true) {
                    const matches = layout.match(/{{IF:(.[^:]+):(.[^}}]+)}}/gm);
                    if (matches == null || !matches.length) break;

                    const m = matches[0];
                    const match = /{{IF:(.[^:]+):(.[^}}]+)}}/gm.exec(m);
                    const value = eval(match[1]) ?? '';
                    layout = layout.replace(m, (value ? match[2] : ''));
                }

                // ------------------ Parse Conditional Templates --------------------
                while (true) {
                    const matches = layout.match(/\{\{TMPL\d:(.[^:]+):([\s\S]*?)\}\}/gm);
                    if (matches == null || !matches.length) break;

                    const m = matches[0];
                    const match = /\{\{TMPL\d:(.[^:]+):([\s\S]*?)\}\}/gm.exec(m);
                    const value = eval(match[1]) ?? '';
                    layout = layout.replace(m, (value ? match[2] : ''));
                }

                // ------- Parse [[EXPRESSION]] > Expression must be field properties path ---------
                _.forEach(layout.match(/\[\[(.[^\[\]\{\}\?]+)(\?\?)?(.[^\[\]\{\}]*)?\]\]/gm), (m) => {
                    const match = /\[\[(.[^\[\]\{\}\?]+)(\?\?)?(.[^\[\]\{\}]*)?\]\]/gm.exec(m);
                    var value = _.get(field, match[1]) ?? '';

                    if (!value && match[3]) value = this.globalService.bEval(match[3]);

                    layout = layout.replace(m, value);
                });

                // --------------------- PARSE Field Show Conditions --------------------------
                if (field.ShowConditions && field.ShowConditions.length) {
                    layout = layout.replace(/\[FIELD-DISPLAY-EXPRESSION\]/g, `ng-if="Field.${field.FieldName}.IsShow"`);
                } else {
                    layout = layout.replace(/\[FIELD-DISPLAY-EXPRESSION\]/g, "");
                }

                this.parseFieldPanes(field, layout).then((data) => {
                    if (data && data.type == 0) {
                        layout = data.html;
                    } else if (data && data.type == 1) {
                        layout = layout.replace(/\[FIELDPANES(,attrs=(.[^\]]+))?\]/gm, data.html);
                    }

                    layout = layout.replace(/\[FIELD\]/g, "Field." + field.FieldName);
                    layout = layout.replace(/\[FIELDId\]/g, field.FieldId);
                    layout = layout.replace(/\[FIELDNAME\]/g, field.FieldName);

                    $defer.resolve(layout);
                });
            }).catch((err) => {
                $defer.resolve("");
            });
        } else {
            $defer.resolve("");
        }

        return $defer.promise;
    }

    parseFieldPanes(field, layout) {
        const $defer = this.$q.defer();

        var matches = layout.match(/\[FIELDPANES(,attrs=(.[^\]]+))?\]/gm);
        if (matches && matches.length) {
            _.forEach(matches, (m) => {
                const match = /^\[FIELDPANES(,attrs=(.[^\]]+))?\]$/gm.exec(m);

                this.$deferredBroadcast(this.$scope, `onGet${field.FieldType}FieldPanes`, { field: field, attrs: match[2], layout }).then((data) => {
                    $defer.resolve({ html: data.html, type: data.type != undefined ? data.type : 1 });
                });
            });
        }
        else {
            $defer.resolve({ type: 2 });
        }

        return $defer.promise;
    }

    getDefaultFieldLayoutTemplate(field) {
        const result =
            `<div data-field="[[FieldName]]" [FIELD-DISPLAY-EXPRESSION] class="[[Settings.CssClass??'b-field']]"
                {{IF:field.IsValuable:ng-class="{'b-invalid':[FIELD].Validated && ([FIELD].RequiredError || ![FIELD].IsValid)}"}}>
    {{IF:field.FieldText:<label class="[[Settings.FieldTextCssClass??'b-form label']]">[[FieldText]]</label>}}
    [FIELD-COMPONENT]
    {{IF:field.IsValuable && field.IsRequired:<p ng-show="[FIELD].Validated && ([FIELD].Value==null || [FIELD].Value==undefined || [FIELD].Value=='') && [FIELD].RequiredError" 
        class="[[Settings.RequiredMessageCssClass??'b-invlid-message']]">[[Settings.RequiredMessage]]</p>}}
    {{IF:field.Settings.EnableValidationPattern&&Settings.ValidationPattern:<p ng-show="[FIELD].IsPatternValidate && ![FIELD].IsValid && [FIELD].Value!==undefined && [FIELD].Value!==null && [FIELD].Value!==''" 
        class="[[Settings.ValidationMessageCssClass??b-pattern-message]]">[[Settings.ValidationMessage]]</p>}}
    {{IF:field.Settings.Subtext:<span class="[[Settings.SubtextCssClass??'b-subtext']]">[[Settings.Subtext]]</span>}}
</div>`;
        return result;
    }

    //#endregion

    //#region Other Methods

    rebuildScenarioModules(scenarioId, $scope) {
        this.apiService.get("Module", "GetScenarioModulesAndFields", { scenarioId: scenarioId })
            .then((data) => {
                var fieldTypes = data.FieldTypes;

                const populateModules = (index) => {
                    if (index < data.Modules.length) {
                        var m = data.Modules[index];

                        if (m.Skin) {
                            angular.forEach(m.Fields, (f) => {
                                f.FieldTypeObject = {};
                                _.filter(fieldTypes, (ft) => { return ft.FieldType == f.FieldType }).map((fieldType) => {
                                    f.FieldTypeObject = fieldType;
                                });

                                f.Settings = f.Settings || {};
                                this.globalService.parseJsonItems(f.Settings);
                            });

                            this.renderModule(m, m.Fields, $scope).then((moduleTemplate) => {
                                this.awaitAction = {
                                    title: "Save Rendered Module",
                                    subtitle: "Just a moment for save rendered module...",
                                };

                                this.apiService.post("Module", "SaveRenderedModule", {
                                    ModuleId: m.ModuleId,
                                    ParentId: m.ParentId,
                                    ModuleTemplate: moduleTemplate,
                                    IsRenderScriptsAndStyles: true,
                                }).then((data) => {
                                    populateModules(index + 1);

                                    this.notifyService.success(
                                        "Rendered module save has been successfully"
                                    );

                                    delete this.awaitAction;
                                }, (error) => {
                                    populateModules(index + 1);

                                    this.awaitAction.isError = true;
                                    this.awaitAction.subtitle = error.statusText;
                                    this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                                    this.notifyService.error(error.data.Message);
                                });
                            });
                        } else
                            populateModules(index + 1);
                    }
                }

                populateModules(0);
            });
    }

    //#endregion
}