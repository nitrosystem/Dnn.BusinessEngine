import { GlobalSettings } from "../../angular-configs/global.settings";

export class ModuleHtmlEditorController {
    constructor(
        $scope,
        $rootScope,
        studioService,
        $timeout,
        $compile,
        $filter,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService,
        eventService,
        hubService,
        moduleDesignerService,
        moduleBuilderService,
        $deferredBroadcast
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.studioService = studioService;
        this.$timeout = $timeout;
        this.$compile = $compile;
        this.$filter = $filter;
        this.$q = $q;
        this.$deferredBroadcast = $deferredBroadcast;

        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.eventService = eventService;
        this.hubService = hubService;

        this.moduleDesignerService = moduleDesignerService;
        this.moduleBuilderService = moduleBuilderService;

        this.currentTabKey = this.$rootScope.currentTab.key;
        this.moduleFormDesigner = moduleFormDesigner;
        this.moduleHtmlEditor = moduleHtmlEditor;
        this.moduleVariablesWidget = moduleVariablesWidget;
        this.moduleSkinWidget = moduleSkinWidget;
        this.moduleLayoutTemplateWidget = moduleLayoutTemplateWidget;
        this.moduleCustomResourcesWidget = moduleCustomResourcesWidgets;
        this.moduleCustomLibrariesWidget = moduleCustomLibrariesWidget;
        this.fieldEditWidget = fieldEditWidget;
        this.fieldTemplateWidget = fieldTemplateWidget;
        this.fieldSettingsWidget = fieldSettingsWidget;
        this.fieldShowConditionsWidget = fieldShowConditionsWidget;
        this.fieldConditionalValuesWidget = fieldConditionalValuesWidget;
        this.fieldDataSourceWidget = fieldDataSourceWidget;

        this.variableScopes = [
            { Text: "Global", Value: 0 },
            { Text: "Client Side", Value: 1 },
            { Text: "Server Side", Value: 2 },
        ];

        this.variableTypes = [{
            Value: "string",
            Text: "String",
            Type: 0,
        },
        {
            Value: "bool",
            Text: "Boolean",
            Type: 0,
        },
        {
            Value: "byte",
            Text: "Byte",
            Type: 0,
        },
        {
            Value: "char",
            Text: "Char",
            Type: 0,
        },
        {
            Value: "datetime",
            Text: "Date Time",
            Type: 0,
        },
        {
            Value: "decimal",
            Text: "Decimal",
            Type: 0,
        },
        {
            Value: "double",
            Text: "Double",
            Type: 0,
        },
        {
            Value: "short",
            Text: "Short",
            Type: 0,
        },
        {
            Value: "int",
            Text: "Int",
            Type: 0,
        },
        {
            Value: "long",
            Text: "Long",
            Type: 0,
        },
        {
            Value: "sbyte",
            Text: "Sbyte",
            Type: 0,
        },
        {
            Value: "float",
            Text: "Float",
            Type: 0,
        },
        {
            Value: "ushort",
            Text: "Ushort",
            Type: 0,
        },
        {
            Value: "uint",
            Text: "Uint",
            Type: 0,
        },
        {
            Value: "ulong",
            Text: "Ulong",
            Type: 0,
        },
        {
            Value: "datetime",
            Text: "Date Time",
            Type: 0,
        },
        {
            Value: "timespan",
            Text: "Time Span",
            Type: 0,
        },
        {
            Value: "guid",
            Text: "Guid",
            Type: 0,
        },
        {
            Value: "imageUrl",
            Text: "Image Url",
            Type: 0,
        },
        {
            Value: "imageFile",
            Text: "Image File",
            Type: 0,
        },
        {
            Value: "file",
            Text: "File",
            Type: 0,
        },
        {
            Value: "customObject",
            Text: "Custom Object",
        },
        {
            Value: "customList",
            Text: "Custom List",
        },
        {
            Value: "viewModel",
            Text: "View Model",
        },
        {
            Value: "listOfViewModel",
            Text: "List Of View Model",
        },
        ];

        $scope.$watch('$.currentField', (oldVal, newVal) => {
            if (!this.ignoreWatchCurrentField && this.currentField && oldVal != newVal) {
                this.currentField.isEdited = true;
            }
        }, true);

        $scope.$on("onShowFieldDataSource", (e, args) => {
            this.onShowFieldDataSourceClick(args.field);
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        /*-----------------------------------------------------------------------
             Standard Window Events Custom Methods
        --------------------------------------------------------------------*/
        /*-----------------------------------------------------------------------
            keydown detect
        -------------------------------------------------------------------*/
        this.eventService.register('keydown', (e) => {
            //ctrl + s -- Save Current Field
            if (e.ctrlKey && (e.key == 's' || e.key == 'S') && this.currentField && this.currentFieldFocused) {
                this.saveCurrentField();

                e.preventDefault();
            }

            //Esc -- Cancel Current Field
            if (e.key == 'Escape' && this.currentField && this.currentFieldFocused) {
                this.onCancelEditFieldClick(e);

                e.preventDefault();
            }

            //ctrl + 5 -- Refresh Field
            if (e.ctrlKey && e.key == '5' && this.currentField && this.currentFieldFocused) {
                this.onRefreshFieldClick(e, this.currentField.FieldId);

                e.preventDefault();
            }

            //Delete -- Delete Field
            if (e.ctrlKey && e.key == 'Delete' && this.currentField && this.currentFieldFocused) {
                this.onDeleteFieldClick(e);

                e.preventDefault();
            }

            //ctrl + q -- Show Field Actions
            if (e.ctrlKey && (e.key == 'q' || e.key == 'Q') && this.currentField && this.currentFieldFocused) {
                this.onShowFieldActionsClick(e);

                e.preventDefault();
            }

            //ctrl + 🠛🠅 -- Select Prev/Next Field
            if (e.ctrlKey && this.currentField && this.currentFieldFocused && (e.key == 'ArrowDown' || e.key == 'ArrowUp')) {
                const $field = this.getFieldElementByFieldId(this.currentField.FieldId);
                const $target = e.key == 'ArrowUp' ? $field.previousElementSibling : $field.nextElementSibling;
                if ($target && $target.attributes['b-field']) {
                    const fieldId = $target.attributes['b-field'].value;
                    this.onFieldItemClick(e, fieldId);
                    if (!$scope.$$phase) $scope.$apply();

                    this.scrollToFieldSection(this.currentField.FieldId)
                }

                e.preventDefault();
            }

            //shift + 🠛🠅 -- Swap Current Field
            if (e.shiftKey && this.currentField && this.currentFieldFocused && (e.key == 'ArrowDown' || e.key == 'ArrowUp')) {
                this.onFieldSwap(e, e.key.replace('ArrowDown', 'down').replace('ArrowUp', 'up'));

                if (!$scope.$$phase) $scope.$apply();

                e.preventDefault();
            }

            //f10 -- Build Module
            if (e.key == 'F10') {
                this.buildModule();
                e.preventDefault();
            }

            //ctrl + f10 -- Build Module
            if (e.ctrlKey && e.key == 'F10') {
                this.buildModule(true);
                e.preventDefault();
            }
        }, [
            { l: 'id', r: this.globalService.getParameterByName("id"), isPageParam: true },
            { l: 'module.ModuleBuilderType', r: 'FormDesigner', _: this },
        ]);

        /*-----------------------------------------------------------------------
            keydown detect
        -------------------------------------------------------------------*/
        this.eventService.register('keydown', (e) => {
            if (e.key == 'F6') {
                var tabNumber = $('button[data-bs-toggle="tab"].active').data('tab');
                tabNumber = tabNumber >= 1 && tabNumber < 3 ? tabNumber + 1 : 1;
                $(`button[data-tab=${tabNumber}]`).tab('show');

                e.preventDefault();
            }
        }, [
            { l: 'id', r: this.globalService.getParameterByName("id"), isPageParam: true },
            { l: 'module.ModuleBuilderType', r: 1, _: this },
        ]);

        /*-----------------------------------------------------------------------
            window scroll detect
         -------------------------------------------------------------------*/
        this.eventService.register('scroll', (e) => {
            const top = $(window).scrollTop();
            if (top > 150)
                $(`#buildWrapper${this.module.Id}`).addClass('b-sticky');
            else
                $(`#buildWrapper${this.module.Id}`).removeClass('b-sticky');
        }, [
            { l: 'id', r: this.globalService.getParameterByName("id"), isPageParam: true },
            { l: 'module.ModuleBuilderType', r: 'FormDesigner', _: this },
        ]);

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");

        this.running = "get-module-builder";
        this.awaitAction = {
            title: "Get Module Builder Data",
            subtitle: "Just a moment for loading module builder data...",
            showProgress: true
        };

        let hubId = this.hubService.subscribe('module', 'module-' + id, 'get-module', this, this.monitoring, () => {
            this.awaitAction.showProgress = false;

            $('.await-action-subtitle').text('');
        });

        this.apiService.getWithMonitoring("Module", "GetModuleBuilder", hubId, { moduleId: (id || null) }).then((data) => {
            this.module = data.Module ?? {};
            this.fields = data.Fields;
            this.actions = data.Actions;
            this.viewModels = data.ViewModels;
            this.variables = data.Variables;
            this.roles = data.Roles;
            this.libraries = data.Libraries;
            this.customResources = data.CustomResources;
            this.customLibraries = data.CustomLibraries;
            this.field = {};

            this.fieldTypes = this.parseFieldTypes(data.FieldTypes);

            this.module.LayoutTemplateBackup = _.clone(this.module.LayoutTemplate);
            this.module.LayoutCssBackup = _.clone(this.module.LayoutCss);

            if (this.module.Wrapper === 1 ) {
                _.filter(this.$rootScope.explorerItems, (item) => {
                    return (
                        item.ItemId == this.module.Id &&
                        item.DashboardPageParentId
                    );
                }).map((item) => {
                    const pageId = item.DashboardPageParentId;

                    this.$rootScope.explorerExpandedItems = [
                        { name: "dashboards" },
                        { name: "dashboard", id: this.module.ParentId },
                        { name: "pages", id: this.module.ParentId },
                        { name: "page", id: pageId },
                        { name: "module", id: this.module.Id },
                    ];
                });
            }

            _.forEach(this.fields, (field) => {
                field.FieldTypeBackup = field.FieldType;
                field.FieldTypeObject = _.find(this.fieldTypes, (ft) => {
                    return ft.FieldType == field.FieldType;
                }) ?? {
                    FieldComponent: 'b-not-field-type'
                };

                this.globalService.parseJsonItems(field.Settings);

                this.field[field.FieldName] = field;
            });

            this.$scope.$emit("onUpdateCurrentTab", {
                id: this.module.Id,
                title: this.module.ModuleName,
            });
            if (this.module.ModuleBuilderType == 'FormDesigner' && this.module.LayoutTemplate) {
                this.$timeout(() => {
                    this.renderDesignForm();
                });
            }
            else if (this.module.ModuleBuilderType ===1 && !this.module.CustomJs)
                this.module.CustomJs = `function ${this.globalService.capitalizeFirstLetter(this.module.ModuleName.replace(/-/g, ''))}Controller(moduleController,$scope) /* Please don't rename function name */ \n{\n\tthis.onPageLoad = function()\n\t{\n\t\tmoduleController.callActionsByEvent('module','OnPageLoad').then((data) => {\t\n\t\t\t//...\n\t\t});\n\t}\n}`;

            this.onFocusModule();

            delete this.currentField;
            delete this.running;

            this.$timeout(() => { delete this.awaitAction; }, 1000);
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });

        this.$scope.$emit("onChangeActivityBar", {
            name: "builder",
            title: "Module Builder",
        });

        this.onSidebarTabClick('toolbox');

        this.setForm();
    }

    onFocusModule() {
        const items = this.studioService.createSidebarExplorerPath(this.module.Id, "Module");
        this.$rootScope.explorerExpandedItems.push(...items);
        this.$rootScope.explorerExpandedItems.push(this.module.ModuleType.toLowerCase() + '-modules');

        this.$rootScope.explorerCurrentItem = !this.module || !this.module.Id ? "module-builder" : this.module.Id;

        this.onSidebarTabClick('toolbox');
        this.$rootScope.currentActivityBar = "builder";
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    setForm() {
        this.form = this.validationService.init({
            FieldName: {
                id: "txtFieldName",
                required: true,
            },
            FieldText: {
                rule: (value) => {
                    if (this.currentField && !this.currentField.Settings.IsHideFieldText && !value) return false;

                    return true;
                },
                id: "txtFieldText",
                required: true,
            },
            FieldType: {
                id: "drpFieldType",
                required: true,
            },
            Template: {
                rule: (value) => {
                    if (!value && this.currentField && this.currentField.InheritTemplate)
                        return true;
                    else if (value)
                        return true;
                },
                required: true,
            },
            Theme: {
                rule: (value) => {
                    if (!value && this.currentField && this.currentField.FieldTypeObject.Themes && this.currentField.FieldTypeObject.Themes.length)
                        return false;
                    else
                        return true;
                },
                required: true,
            },
        },
            true,
            this.$scope,
            "$.currentField"
        );

        this.moduleSkinForm = this.validationService.init({
            Skin: {
                id: "inpModuleSkin",
                required: true,
            }

        },
            true,
            this.$scope,
            "$.module"
        );
    }

    parseFieldTypes(fieldTypes) {
        fieldTypes.forEach(ft => {
            ft.Icon = (ft.Icon || '').replace('[EXTPATH]', GlobalSettings.modulePath + "extensions");

            _.forEach(ft.Templates, (t) => { return t.TemplateImage = (t.TemplateImage || '').replace('[EXTPATH]', GlobalSettings.modulePath + "extensions"); })
            _.forEach(ft.Themes, (t) => { return t.ThemeImage = (t.ThemeImage || '').replace('[EXTPATH]', GlobalSettings.modulePath + "extensions"); })
        });

        return fieldTypes;
    }

    /*------------------------------------*/
    /* Build Module & Render Design Module Methods  */
    /*------------------------------------*/
    buildModule(isFastBuilding) {
        if (!this.module.ModuleBuilderType) {
            alert('the ModuleBuilderType field of the module fields has not set.you must be set the field value.');
            this.onModuleSettingsClick();

            return;;
        }

        let hubId;

        if (!isFastBuilding) {
            hubId = this.hubService.subscribe('module', this.module.ModuleName, 'build', this, this.monitorbuilding, () => {
                this.$scope.building.isEnded = true
            });
        }

        this.running = "building-module";
        this.awaitAction = {
            title: "Building Module...",
            subtitle: "Just a moment for building module without tracing...",
            subtitleColor: '#fff',
        };

        $(`#moduleTemplateProgress${this.module.Id}`).css('width', '0');
        $(`#fieldsScriptsProgress${this.module.Id}`).css('width', '0');
        $(`#actionsScriptsProgress${this.module.Id}`).css('width', '0');
        $(`#moduleStylesProgress${this.module.Id}`).css('width', '0');
        $(`#moduleLibrariesProgress${this.module.Id}`).css('width', '0');

        $(`#buildLogs${this.module.Id}`).html('')

        const renderTemplateFunc = (type, isFastBuilding) => {
            const $defer = this.$q.defer();

            if (type ===1 ) {
                $defer.resolve();
            } else if (type == 'FormDesigner') {
                this.moduleBuilderService.renderModuleTemplate(this.module, this.fields, this.$scope, isFastBuilding).then((moduleTemplate) => {
                    $defer.resolve(moduleTemplate);
                }, (error) => {
                    if (error && error.errorType == 'skin') this.$timeout(() => this.onShowModuleSkinClick(), 1000);

                    $defer.reject(error);
                });
            }

            return $defer.promise;
        };

        renderTemplateFunc(this.module.ModuleBuilderType, isFastBuilding).then((moduleTemplate) => {
            const buildingData = {
                ModuleId: this.module.Id,
                ParentId: this.module.ParentId,
                ModuleBuilderType: this.module.ModuleBuilderType,
                ModuleTemplate: moduleTemplate,
                CustomHtml: this.module.CustomHtml,
                CustomJs: this.module.CustomJs,
                CustomCss: this.module.CustomCss
            };

            const postMethod = !isFastBuilding ?
                this.apiService.postWithMonitoring("Module", "BuildModule", hubId, buildingData)
                : this.apiService.post("Module", "BuildModule", buildingData)

            postMethod.then((data) => {
                this.notifyService.success("Build module has been successfully!. ;)");

                delete this.neetToBuilding;
                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        });
    }

    onContinueEditFormClick() {
        delete this.$scope.building;
    }

    renderDesignForm() {
        const $defer = this.$q.defer();

        this.moduleDesignerService.renderDesignForm(this.module, this.fields, this.$scope, this.actions).then((data) => {
            $("#board" + this.module.Id).html(data.$board);

            this.panes = data.panes;

            this.setFieldsSortingUi();
            this.setFieldsDragingUi();

            $defer.resolve();
        }, (error) => {
            if (error && error.errorType == 'skin') this.$timeout(() => this.onShowModuleSkinClick(), 1000);
        });

        return $defer.promise;
    }

    onClearCache() {
        this.running = "clear-cache";
        this.awaitAction = {
            title: "Clear Cache",
            subtitle: "Just a moment for clear cache and add host version...",
        };

        this.apiService.post("Module", "ClearCacheAndAddCmsVersion").then((data) => {
            this.notifyService.success("Dnn cache and increasing host version has been successfully");

            delete this.awaitAction;
            delete this.running;

            if (confirm("Do you want to refresh the page?")) location.reload();
        }, (error) => {
            $defer.reject(error);

            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    setFieldsSortingUi() {
        $(".sortable-row").sortable({
            tolerance: 'pointer',
            //helper: 'clone',
            appendTo: document.body,
            connectWith: ".sortable-row",
            handle: ".handle",
            start: ($event, ui) => {
                //$(".pane.pane-footer").css("opacity", ".1");
                ui.item.find(".field-body").addClass("dragable");
            },
            stop: ($event, ui) => {
                $(".pane.pane-footer").css("opacity", "1");
                ui.item.find(".field-body").removeClass("dragable");

                this.$scope.$apply(() => {
                    const paneName = $(ui.item[0].parentElement).data("pane");
                    const parentId = $(ui.item[0].parentElement).data("parent-id") || null;

                    this.currentField.PaneName = paneName;
                    this.currentField.ParentId = parentId;

                    this.saveCurrentField().then(() => this.sortPaneFields(paneName));
                });
            },
        });
    }

    setFieldsDragingUi() {
        $("#board" + this.module.Id)
            .find("*[field-drop]")
            .droppable({
                greedy: true,
                accept: '*[data-drag="true"]',
                drop: ($event, ui) => {
                    var args = [$event, ui];
                    this.$scope.$apply(() => {
                        this.onFieldDrop.apply(this, args);
                    });
                },
                over: this.onFieldDragOver,
                out: this.onFieldDragOut,
            });
    }

    onGotoModuleResourcesClick() {
        this.$scope.$emit("onGotoPage", {
            page: "page-resources",
            id: this.module.Id,
            activityBar: 'page-resources',
            subParams: {
                mode: 'module-resources',
                disableActivityBarCallback: true,
            }
        });
    }

    /*------------------------------------*/
    /* Module Variables Methods  */
    /*------------------------------------*/
    onShowVariablesClick() {
        this.workingMode = "module-variables";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onReloadVariablesClick() {
        this.running = "reloading-module-variables";
        this.awaitAction = {
            title: "Reloading Variables",
            subtitle: "Just a moment for reloading the module variables...",
        };

        this.apiService.get("Module", "GetModuleVariables", {
            moduleId: this.module.Id,
        }).then((data) => {
            this.variables = data;

            this.notifyService.success(
                "Module variables loaded has been successfully"
            );

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        }
        );
    }

    onAddVariableClick() {
        if (this.variable) return;

        this.variables = this.variables || [];
        this.variables.push({
            IsNew: true,
            IsEdited: true,
            OrderId: this.variables.length + 1,
        });

        this.variable = _.clone(this.variables[this.variables.length - 1]);

        this.$timeout(() => {
            this.$scope.$broadcast("onEditVariable");
        }, 500);
    }

    onEditVariableClick(variable, $index) {
        if (this.variable) return;

        variable.IsEdited = true;

        this.variable = _.clone(variable);
        this.variable.OrderId = $index + 1;
        this.variable.IsNew = false;
    }

    onDeleteVariableClick($index) {
        if (confirm("Are you sure delete this variable!?"))
            this.variables.splice($index, 1);
    }

    onSaveVariableClick() {
        this.variable.IsEdited = false;

        this.variables[this.variable.OrderId - 1] = _.clone(this.variable);

        delete this.variable;
    }

    onCancelVariableClick() {
        if (this.variable.IsNew)
            this.variables.splice(this.variable.OrderId - 1, 1);
        else this.variables[this.variable.OrderId - 1].IsEdited = false;

        delete this.variable;
    }

    onSaveVariablesClick() {
        this.running = "save-module-variables";
        this.awaitAction = {
            title: "Saving Variables",
            subtitle: "Just a moment for saving the module variables...",
        };

        this.apiService
            .post("Module", "SaveModuleVariables", this.variables, {
                moduleId: this.module.Id,
            }).then((data) => {
                this.variables = data;

                this.notifyService.success("Module variables updated has been successfully");

                this.disposeWorkingMode();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
    }

    onCancelVariablesClick() {
        this.disposeWorkingMode();
    }

    /*------------------------------------*/
    /* Appearance Methods  */
    /*------------------------------------*/
    onShowModuleSkinClick() {
        this.workingMode = "module-theme";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onReloadSkinsClick() {
        this.running = "reloading-module-skins";
        this.awaitAction = {
            title: "Reloading Skins",
            subtitle: "Just a moment for reloading the module skins...",
        };

        this.apiService.get("Module", "GetModuleSkins", { moduleId: this.module.Id }).then((data) => {
            this.skins = data.Skins;
            this.module.SkinObject = data.CurrentSkin;

            this.notifyService.success("Module Skins loaded has been successfully");

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        }
        );
    }

    onSelectSkinClick(skin) {
        if (!this.module.SkinObject || this.module.Skin != skin.SkinName) {
            this.moduleSkinBackup = this.module.Skin;
            this.moduleSkinObjectBackup = this.module.SkinObject;
            this.isSkinOrTemplateChanged = this.module.skinIsEmpty ? false : true;
            this.module.Skin = skin.SkinName;
            this.module.Template = null;

            this.running = "loading-skin-items";
            this.awaitAction = {
                title: "Loading Skin Items",
                subtitle: "Just a moment for loading the module skin items...",
            };

            this.apiService.get("Module", "GetModuleSkin", {
                moduleId: this.module.Id,
                skinName: skin.SkinName,
            }).then((data) => {
                this.module.SkinObject = data;

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    onSelectSkinTemplateClick(template) {
        if (this.module.skinIsEmpty || this.module.Template != template.TemplateName) this.isSkinOrTemplateChanged = true;

        this.moduleTemplateBackup = this.module.Template;
        this.module.Template = template.TemplateName;
    }

    onSelectFieldTemplate(template) {
        this.currentField.Template = template.TemplateName;
        this.currentField.IsSkinTemplate = template.IsSkinTemplate;

        delete this.currentField.Theme;
    }

    onSelectFieldTheme(theme) {
        this.currentField.Theme = theme.ThemeName;
        this.currentField.IsSkinTheme = theme.IsSkinTheme;
        this.currentField.ThemeCssClass = theme.ThemeCssClass
    }
    onSaveSkinClick() {
        this.moduleSkinForm.validated = true;
        this.moduleSkinForm.validator(this.module);
        if (this.moduleSkinForm.valid) {
            if (this.isSkinOrTemplateChanged)
                swal({
                    title: "Important Note!!",
                    text: "after apply change the skin or template, the layout of the module will change, and will losted the Panes then you must sort the fields based on the new layout.",
                    icon: "warning",
                    buttons: true,
                    dangerMode: true,
                }).then((confirm) => {
                    if (confirm) {
                        if (!this.module.Template) this.module.Template = 'Default';
                        this.saveSkin();
                    }
                });
            else {
                this.saveSkin();
            }

            this.skin = _.find(this.skins, (s) => { return s.SkinName == this.module.Skin });
        }
    }

    saveSkin() {
        const $defer = this.$q.defer();

        this.running = "save-skin";
        this.awaitAction = {
            title: "Saving Module Skin",
            subtitle: "Just a moment for saving the module skin...",
        };

        this.apiService.post("Module", "SaveModuleSkin", {
            ModuleId: this.module.Id,
            Skin: this.module.Skin,
            Template: this.module.Template,
            Theme: this.module.Theme,
            EnableFieldsDefaultTemplate: this.module.EnableFieldsDefaultTemplate,
            EnableFieldsDefaultTheme: this.module.EnableFieldsDefaultTheme,
            FieldsDefaultTemplate: this.module.FieldsDefaultTemplate,
            FieldsDefaultTheme: this.module.FieldsDefaultTheme,
            LayoutTemplate: this.module.LayoutTemplate,
            LayoutCss: this.module.LayoutCss
        }).then((data) => {
            this.notifyService.success("Module skin updated has been successfully");

            this.module.LayoutTemplate = data.layoutTemplateHtml;
            this.module.LayoutCss = data.layoutTemplateCss;
            this.fieldTypes = this.parseFieldTypes(data.fieldTypes);

            this.renderDesignForm();
            this.buildModule(true);
            this.disposeWorkingMode();

            $defer.resolve(data);

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            $defer.reject(error);

            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        }
        );

        return $defer.promise;
    }

    onSaveTemplateWidgetClick($event) {
        this.onSaveFieldClick($event);
    }

    onCancelSkinClick() {
        if (this.moduleSkinBackup) {
            this.module.Skin = this.moduleSkinBackup;
            this.module.SkinObject = this.moduleSkinObjectBackup;
        }
        if (this.moduleTemplateBackup) this.module.Template = this.moduleTemplateBackup;

        this.disposeWorkingMode();
    }

    onShowLayoutTemplateClick() {
        this.workingMode = "module-edit-layout-template";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onSaveLayoutTemplateClick() {
        if (this.module.LayoutTemplateBackup != this.module.LayoutTemplate ||
            this.module.LayoutCssBackup != this.module.LayoutCss
        ) this.module.isChangedTemplate = true;
        else
            this.module.isChangedTemplate = false;

        this.running = "save-layout-template";
        this.awaitAction = {
            title: "Saving Module Layout Template",
            subtitle: "Just a moment for saving the module layout template...",
        };

        this.apiService.post("Module", "SaveModuleLayoutTemplate", {
            ModuleId: this.module.Id,
            LayoutTemplate: this.module.LayoutTemplate,
            LayoutCss: this.module.LayoutCss
        }).then((data) => {
            this.disposeWorkingMode();
            this.buildModule(true);
            this.renderDesignForm();

            this.notifyService.success("Module layout template updated has been successfully");

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    onCancelLayoutTemplateClick() {
        this.disposeWorkingMode();
    }

    onModuleSettingsClick() {
        this.$scope.$emit("onGotoPage", {
            page: "create-" + this.module.ModuleType.toLowerCase(),
            id: this.module.Id,
        });
    }

    /*------------------------------------*/
    /*  Field Methods */
    /*------------------------------------*/
    onToggleSearchFieldsClick() {
        this.isShowSearchBox = !this.isShowSearchBox;
        if (this.isShowSearchBox) this.$scope.$broadcast("onFocusSearchInput_" + this.module.Id);
    }

    onRefreshFieldClick($event, fieldId) {
        const $defer = this.$q.defer();

        this.running = "refresh-field";
        this.awaitAction = {
            title: "Refresh Field",
            subtitle: "Just a moment for refresh the field...",
        };

        this.apiService.get("Module", "GetModuleField", { fieldId: fieldId }).then((data) => {
            $defer.resolve();

            this.notifyService.success("Field refreshed has been successfully");

            var field = this.getFieldById(fieldId);
            this.fields[this.fields.indexOf(field)] = data;
            this.field[field.FieldName] = data;
            this.currentField = field;

            delete this.awaitAction;
            delete this.running;

        }, (error) => {
            $defer.resolve();

            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        }
        );

        return $defer.promise;
    }

    onFieldItemClick($event, fieldId) {
        const field = this.getFieldById(fieldId);
        this.currentField = field;
        this.field[this.currentField.FieldName] = this.currentField;
        this.currentFieldBackup = _.cloneDeep(this.currentField);

        this.ignoreWatchCurrentField = true;
        this.$timeout(() => delete this.ignoreWatchCurrentField);

        this.fieldActionsFilter = this.currentField.FieldId;
        if (this.currentSidebarTab != 'actions') this.onSidebarTabClick("field-settings");

        this.$scope.$broadcast("onBindFieldSettings_" + this.currentField.FieldName);

        this.currentFieldFocused = true;

        if ($event) $event.stopPropagation();
    }

    onFieldToolbarClick($event) {
        this.currentFieldFocused = !!this.currentField;

        $event.stopPropagation();
    }

    onFieldItemBlur($event) {
        if ($event.target.contains($event.relatedTarget)) return;

        this.currentFieldFocused = false;

        if ($event) $event.stopPropagation();
    }

    onChangeFieldTypeClick($event, fieldId) {
        swal({
            title: "Are you sure change field type?",
            text: "Some settings may be lost after the field type is changed",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((agree) => {
            if (agree) {
                this.saveCurrentField().then(() => {
                    location.reload();
                    // this.onRefreshFieldClick($event, fieldId).then(() => {
                    //     this.currentField.FieldTypeObject = _.find(this.fieldTypes, (ft) => {
                    //         return ft.FieldType == this.currentField.FieldType;
                    //     });
                    // });
                })
            }
        });
    }

    onCancelEditFieldClick($event) {
        const field = this.getFieldById(this.currentField.FieldId);
        var oldField = _.cloneDeep(this.currentFieldBackup);
        this.fields[this.fields.indexOf(field)] = oldField;
        this.field[this.currentFieldBackup.FieldName] = oldField;

        this.removeCurrentField(true);

        if ($event) $event.stopPropagation();
    }

    addField(paneName, parentId, fieldType, beforeFieldId) {
        const suggestFieldName = fieldType.FieldType + (_.filter(this.fields, (f) => { return f.FieldType == fieldType.FieldType; }).length + 1);
        const defaultSettings = JSON.parse(fieldType.DefaultSettings || "{}");

        this.currentField = {
            moduleId: this.module.Id,
            ParentId: parentId,
            PaneName: paneName,
            FieldType: fieldType.FieldType,
            FieldTypeObject: fieldType ?? {},
            FieldName: suggestFieldName,
            IsRequired: false,
            IsShow: true,
            IsEnabled: true,
            IsGroup: fieldType.IsGroup,
            IsValuable: fieldType.IsValuable,
            IsSelective: fieldType.IsSelective,
            IsJsonValue: fieldType.IsJsonValue,
            InheritTemplate: this.module.EnableFieldsDefaultTemplate,
            InheritTheme: this.module.EnableFieldsDefaultTheme,
            DataSource: {},
            Settings: defaultSettings,
        };

        this.beforeFieldId = beforeFieldId;

        this.checkInheritTemplateAndTheme(this.currentField);

        this.workingMode = "field-edit";
        this.$scope.$emit("onShowRightWidget", { controller: this });

        this.fieldTypeOptions = `<b-${this.currentField.FieldType.toLowerCase()}-options data-field="$.currentField"></b-${this.currentField.FieldType.toLowerCase()}-options>`;
        this.$timeout(() => {
            $('#pnlFieldTypeOptions').append(this.$compile(this.fieldTypeOptions)(this.$scope));
        }, 500);

        this.$deferredBroadcast(this.$scope, `onAdded${this.currentField.FieldType}Field`, { field: this.currentField, controller: this });
    }

    onSaveFieldClick($event, isNewField, changeEditStatus) {
        this.form.validated = true;
        this.form.validator(this.currentField);
        if (this.form.valid) {
            if (isNewField) {
                this.saveCurrentField().then((field) => {
                    field.FieldTypeObject = _.find(this.fieldTypes, (ft) => {
                        return ft.FieldType == field.FieldType;
                    });
                    field.FieldTypeBackup = field.FieldType;

                    this.fields.push(field);
                    this.field[field.FieldName] = field;
                    this.currentField = field;

                    delete this.currentFieldBackup;

                    this.moduleDesignerService.getFieldUI(this.currentField, this.$scope).then(($fieldItem) => {
                        const $field = this.$compile($fieldItem)(this.$scope);

                        if (this.beforeFieldId) {
                            $($field).insertBefore(
                                $("#board" + this.module.Id).find('*[b-field="' + this.beforeFieldId + '"]')
                            );
                        } else {
                            const $boardPane = $("#board" + this.module.Id).find('*[data-pane="' + this.currentField.PaneName + '"]');
                            $boardPane.append($field);
                        }

                        this.sortPaneFields(this.currentField.PaneName);

                        if (field.IsGroup) this.renderDesignForm();

                        this.buildModule(true);

                        this.$timeout(() => {
                            this.disposeWorkingMode();
                            this.onFieldSettingsClick();
                        });
                    });
                });
            } else this.saveCurrentField().then(() => { if (changeEditStatus) delete this.currentField });

            if ($event) $event.stopPropagation();
        }
    }

    saveCurrentField() {
        const $defer = this.$q.defer();

        delete this.currentField.Value;

        this.checkInheritTemplateAndTheme(this.currentField);

        this.running = "save-field";
        this.awaitAction = {
            title: "Saving Field",
            subtitle: "Just a moment for saving the field..."
        };

        this.apiService.post("Module", "SaveModuleField", this.currentField).then((field) => {
            this.notifyService.success(this.currentField.FieldName + " Field updated has been successfully");

            delete this.awaitAction;
            delete this.running;

            this.currentField.isEdited = false;

            $defer.resolve(field);

            this.disposeWorkingMode();
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });

        return $defer.promise;
    }

    checkInheritTemplateAndTheme(field) {
        let fieldType = field.FieldTypeObject;

        if (field.InheritTemplate) {
            if (_.filter(fieldType.Templates, (t) => { return t.TemplateName == this.module.FieldsDefaultTemplate }).length == 0) {
                this.showProblemForInheritTemplate = true;
                delete field.Template;

                this.$timeout(() => {
                    field.InheritTemplate = false
                }, 600)

            } else field.Template = this.module.FieldsDefaultTemplate;
        } else {
            _.filter(field.FieldTypeObject.Templates || [], (t) => { return t.TemplateName == field.Template; }).map((t) => {
                field.IsSkinTemplate = t.IsSkinTemplate;
            });
        }

        if (field.InheritTheme) {
            if (_.filter(fieldType.Themes, (t) => { return (t.TemplateName && t.TemplateName == field.Template && t.ThemeName == this.module.FieldsDefaultTheme) || (!t.TemplateName && t.ThemeName == this.module.FieldsDefaultTheme) }).length == 0) {
                this.showProblemForInheritTheme = true;
                delete field.Theme;
                delete field.ThemeCssClass;

                this.$timeout(() => {
                    field.InheritTheme = false
                }, 600)
            } else field.Theme = this.module.FieldsDefaultTheme;
        } else {
            _.filter(field.FieldTypeObject.Themes || [], (t) => { return (t.TemplateName && t.TemplateName == field.Template && t.ThemeName == field.Theme) || (!t.TemplateName && t.ThemeName == field.Theme); }).map((t) => {
                field.IsSkinTheme = t.IsSkinTemplate;
                field.ThemeCssClass = t.ThemeCssClass;
            });
        }
    }

    onCancelFieldClick() {
        this.disposeWorkingMode();
    }

    onDeleteFieldClick($event) {
        if (confirm("Are you sure delete this field!?")) {
            var fieldId = this.currentField.FieldId;

            this.running = "delete-field";
            this.awaitAction = {
                title: "Deletin Field",
                subtitle: "Just a moment for removing the field...",
            };

            this.apiService.post("Module", "DeleteModuleField", { Id: fieldId }).then((data) => {
                this.notifyService.success("Field deleted has been successfully");

                _.filter(this.fields, (field) => { return field.FieldId == fieldId; }).map((field) => {
                    this.fields.splice(this.fields.indexOf(field), 1);

                    this.buildModule(true);

                    $("#board" + this.module.Id).find('*[b-field="' + field.FieldId + '"]').remove();

                    delete this.field[field.FieldName];
                    delete this.awaitAction;
                    delete this.running;
                });

                this.removeCurrentField(true);
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            }
            );
        }
    }

    getPaneElementByPaneName(paneName) {
        const $pane = $(`*[data-pane="${paneName}"]`);

        return $pane;
    }

    getPaneFieldsByIndex(paneName) {
        const $pane = this.getPaneElementByPaneName(paneName);

        var sortedFields = [];
        $pane.find("div[b-field]").each((index, element) => {
            const fieldId = $(element).attr("b-field");

            var field = this.getFieldById(fieldId);
            field.ViewOrder = index;

            sortedFields.push({ FieldId: field.FieldId, ViewOrder: field.ViewOrder });
        });

        return sortedFields;
    }

    sortPaneFields(paneName) {
        const sortedFields = this.getPaneFieldsByIndex(paneName);

        this.running = "sort-fields";
        this.awaitAction = {
            title: "Sort Fields",
            subtitle: "Just a moment for sorting the fields...",
        };

        this.apiService.post("Module", "SortModuleFields", sortedFields).then((data) => {
            this.notifyService.success("the fields sort has been successfully");

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        }
        );
    }

    removeCurrentField(changeTab) {
        this.currentField.isEdited = false;

        delete this.currentField;
        delete this.currentFieldBackup;

        this.onSidebarTabClick("toolbox");

        return true;
    }

    getFieldById(fieldId) {
        var result = _.find(this.fields, (field) => { return field.FieldId == fieldId; });
        return result;
    }

    getFieldByName(fieldName) {
        var result;

        _.filter(this.fields, (field) => {
            return field.FieldName == fieldName;
        }).map((field) => (result = field));

        return result;
    }

    onFieldSettingsClick($event) {
        this.$scope.$broadcast("onBindFieldSettings_" + this.currentField.FieldName);

        this.onSidebarTabClick("field-settings");
        this.$rootScope.currentActivityBar = "builder";
    }

    getFieldElementByFieldId(fieldId) {
        return $(`div[b-field="${fieldId}"]`)[0];
    }

    onFieldSwap($event, actionType) {
        const $field = this.getFieldElementByFieldId(this.currentField.FieldId);
        const $target = actionType == 'up' ? $field.previousElementSibling : $field.nextElementSibling;
        if ($target && $target.attributes['b-field']) {
            if (actionType == 'up') $field.after($target);
            else if (actionType == 'down') $field.before($target);
            this.sortPaneFields(this.currentField.PaneName);
        }

        if ($event) $event.stopPropagation();
    }

    onFieldChangePaneClick(pane, $event) {
        if (this.currentField.PaneName != pane.paneName) {
            const $pane = $(`div[data-pane="${pane.paneName}"]`);
            const $field = this.getFieldElementByFieldId(this.currentField.FieldId);

            if ($pane.find($field).length == 0) $pane.append($field);

            this.currentField.PaneName = pane.paneName;
            this.currentField.ParentId = pane.parentId;

            this.scrollToFieldSection(this.currentField.FieldId)

            this.saveCurrentField().then(() => this.sortPaneFields(pane.paneName));
        }
    }

    scrollToFieldSection(fieldId) {
        this.$timeout(() => {
            const $field = this.getFieldElementByFieldId(fieldId);
            $([document.documentElement, document.body]).animate({
                scrollTop: $($field).offset().top - 100
            }, 500);

            $('ul.dropdown-menu.show').removeClass('show');
        }, 1000);
    }

    onIsCustomFieldLayoutChange() {
        if (this.currentField.Settings.IsCustomFieldLayout && !this.currentField.Settings.CustomFieldLayoutModified)
            this.currentField.Settings.CustomFieldLayout = this.moduleBuilderService.getDefaultFieldLayoutTemplate(this.currentField)
    }

    onCustomFieldLayoutChange() {
        this.currentField.Settings.CustomFieldLayoutModified = true;
    }

    onResetCustomFieldLayoutClick() {
        this.currentField.Settings.CustomFieldLayout = this.moduleBuilderService.getDefaultFieldLayoutTemplate(this.currentField)

        delete this.currentField.Settings.CustomFieldLayoutModified;
    }

    /*------------------------------------*/
    /* Field Datasource Methods  */
    /*------------------------------------*/
    onShowFieldDataSourceClick($event, fieldId) {
        if (!this.currentField) this.onFieldItemClick($event, fieldId);

        this.workingMode = "field-data-source";
        this.$scope.$emit("onShowRightWidget", { controller: this });

        this.fieldDataSourceBackup = _.clone(this.currentField.DataSource || {});

        this.onFieldDataSourceTypeChange();

        delete this.running;
        delete this.awaitAction;

        if ($event) $event.stopPropagation();
    }

    onFieldDataSourceTypeChange() {
        if (
            this.currentField.DataSource &&
            this.currentField.DataSource.Type == 0 &&
            this.currentField.DataSource.ListId
        ) {
            this.running = "get-field-data-source";
            this.awaitAction = {
                title: "Loading Field Data Source",
                subtitle: "Just a moment for loading the field data source...",
            };

            this.apiService.get("Module", "GetDefinedListByFieldId", {
                fieldId: this.currentField.FieldId,
            }).then((data) => {
                this.definedList = data;

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            }
            );
        } else if (this.currentField.DataSource && this.currentField.DataSource.Type == 1) {
            if (this.definedLists) {
                this.onDefinedListChange();
                return;
            }

            this.running = "get-defined-lists";
            this.awaitAction = {
                title: "Loading Defined Lists",
                subtitle: "Just a moment for loading the defined lists...",
            };

            this.apiService.get("Module", "GetDefinedLists", { fieldId: this.currentField.FieldId, }).then((data) => {
                this.definedLists = data;

                this.onDefinedListChange();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    onFieldDataSourceServiceChange(item) {
        this.currentField.DataSource.ServiceParams = _.clone(
            item.Params
        );
    }

    onCreateFieldDataSourceClick() {
        this.definedList = {
            ScenarioId: this.module.ScenarioId,
            ListType: "FieldOptions",
            ListName: this.currentField.FieldName + "_Options",
            FieldId: this.currentField.FieldId,
        };

        this.running = "create-field-data-source";
        this.awaitAction = {
            title: "Creating Field Data Source",
            subtitle: "Just a moment for creating the field data source...",
        };

        this.apiService.post("Module", "CreateDefinedList", this.definedList).then((data) => {
            this.notifyService.success(
                "Field data source created has been successfully"
            );

            this.definedList.ListId = data;
            this.definedList.Items = [{}];
            this.currentField.DataSource = this.currentField.DataSource || {};
            this.currentField.DataSource.ListId = data;

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    onSaveFieldDataSourceClick($event) {
        if (this.currentField.DataSource && (this.currentField.DataSource.Type == 0 || this.currentField.DataSource.Type == 1) && this.currentField.DataSource.ListId) {
            this.currentField.DataSource.TextField = "Text";
            this.currentField.DataSource.ValueField = "Value";

            this.saveDefinedList($event)
        } else if (this.currentField.DataSource && this.currentField.DataSource.Type == 2) {
            this.onSaveFieldClick($event);
        }
    }

    saveDefinedList($event) {
        this.running = "save-field-data-source";
        this.awaitAction = {
            title: "Saving Field Data Source",
            subtitle: "Just a moment for saving the field data source...",
        };

        this.apiService.post("Module", "SaveDefinedList", this.definedList).then((data) => {
            this.notifyService.success(
                "Field data source saved has been successfully"
            );

            this.currentField.DataSource.Items = _.cloneDeep(this.definedList.Items);

            this.onSaveFieldClick($event);

            this.disposeWorkingMode();

            delete this.definedList;
            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.definedList;
            delete this.running;
        });
    }

    onDefinedListChange() {
        this.definedList = this.definedList || {};
        this.definedList = _.find(this.definedLists, (i) => { return i.ListId == this.currentField.DataSource.ListId });
    }

    onServicePageClick(pageIndex) {
        this.getServices(pageIndex);
    }

    getServices(pageIndex, searchText) {
        const defer = this.$q.defer();

        this.running = "get-services";
        this.awaitAction = {
            title: "Get Services By Page",
            subtitle: "Just a moment for get services...",
        };

        this.apiService.get("Module", "GetServices", { pageIndex: pageIndex, pageSize: 10, searchText: searchText }).then((data) => {
            defer.resolve(data);

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            defer.reject(error);

            delete this.running;
        });

        return defer.promise;
    }

    onCancelFieldDataSourceClick() {
        if (this.fieldDataSourceBackup) {
            this.currentField.DataSource = _.cloneDeep(this.fieldDataSourceBackup);

            delete this.fieldDataSourceBackup;
        }

        this.onCancelFieldClick();
    }

    onEditFieldDataSourceClick($event, fieldId) {
        this.onShowFieldDataSourceClick($event, fieldId);
    }

    /*------------------------------------*/
    /* Actions & Conditions Methods  */
    /*------------------------------------*/
    onShowActionsClick() {
        this.$scope.$emit("onGotoPage", {
            page: "actions",
            parentId: this.module.Id,
            subParams: { type: "module" },
        });
    }

    onShowFieldActionsClick($event) {
        this.fieldActionsFilter = this.currentField.FieldId;

        this.onSidebarTabClick('actions');
        this.$rootScope.currentActivityBar = "builder";

        if ($event) $event.stopPropagation();
    }

    onShowConditionsClick($event, fieldId) {
        if (fieldId) this.onFieldItemClick($event, fieldId);

        this.workingMode = "field-show-conditions";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onShowConditionalValuesClick() {
        this.workingMode = "field-conditional-values";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onAddFieldConditionalValueClick() {
        this.currentField.FieldValues = this.currentField.FieldValues || [];
        this.currentField.FieldValues.push({ Conditions: [] });
    }

    onAddActionClick(type, parentId) {
        this.$scope.$emit("onGotoPage", {
            page: "create-action",
            parentId: parentId,
            subParams: { type: type },
        });
    }

    onEditActionClick(action) {
        var subParams = {};
        if (action.FieldId) subParams.type = "field";

        this.$scope.$emit("onGotoPage", {
            page: "create-action",
            parentId: action.FieldId || action.ModuleId,
            id: action.ActionId,
            title: action.ActionName,
            subParams: subParams,
        });
    }

    onEditFieldActionClick(action) {
        this.$scope.$emit("onGotoPage", {
            page: "create-action",
            parentId: action.FieldId,
            id: action.ActionId,
            title: action.ActionName,
            subParams: { type: 'field' },
        });
    }

    onGotoActionsPageClick() {
        var subParams = {};
        if (this.fieldActionsFilter) subParams.type = "field";
        var parentId = this.fieldActionsFilter ? this.currentField.FieldId : undefined;
        var title = this.fieldActionsFilter ? this.currentField.FieldName + ' Actions' : this.module.ModuleName + ' Actions';

        this.$scope.$emit("onGotoPage", {
            page: "actions",
            parentId: parentId,
            title: title,
            subParams: subParams,
        });
    }

    onRemoveActionsFilterClick() {
        delete this.fieldActionsFilter;
    }

    onShowFieldTemplateClick() {
        this.checkInheritTemplateAndTheme(this.currentField);
        this.currentFieldBackup = _.cloneDeep(this.currentField);

        this.workingMode = "field-template";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onCancelFieldTemplateClick() {
        this.currentField = _.cloneDeep(this.currentFieldBackup);
        this.field[this.currentField.FieldName] = this.currentField;

        this.disposeWorkingMode();
    }

    /*------------------------------------*/
    /* Custom Resources Methods  */
    /*------------------------------------*/
    onShowCustomResourcesClick() {
        this.workingMode = "module-custom-resources";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onAddCustomResourceClick() {
        this.customResources = this.customResources || [];
        this.customResources.push({ ModuleId: this.module.Id });
    }

    onDeleteCustomResourceClick($index) {
        this.customResources.splice($index, 1);
    }

    onSaveCustomResourcesClick() {
        this.running = "save-custom-resources";
        this.awaitAction = {
            title: "Saving Custom Resources",
            subtitle: "Just a moment for saving the custom resources...",
        };

        this.apiService.post("Module", "SaveCustomResources", this.customResources, { moduleId: this.module.Id }).then((data) => {
            this.notifyService.success("Module custom resources updated has been successfully");

            this.disposeWorkingMode();

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            delete this.running;
        });
    }

    onCancelCustomResourcesClick() {
        this.disposeWorkingMode();
    }

    /*------------------------------------*/
    /* Custom Libraries Methods  */
    /*------------------------------------*/
    onShowCustomLibrariesClick() {
        this.workingMode = "module-custom-libraries";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onAddCustomLibraryClick() {
        this.customLibraries = this.customLibraries || [];
        this.customLibraries.push({ ModuleId: this.module.Id });
    }

    onCustomLibraryChange(library) {
        let lb = _.find(this.libraries, (l) => { return l.LibraryId == library.LibraryId });
        library.LibraryName = lb.LibraryName;
        library.Version = lb.Version;
        library.LocalPath = lb.LocalPath;

        this.running = "load-library-resources";
        this.awaitAction = {
            title: "Loading Library Resources",
            subtitle: "Just a moment for loading the library resources...",
        };

        this.apiService.get("Module", "GetLibraryResources", { libraryId: library.LibraryId }).then((data) => {
            this.libraryResources = data;

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    onDeleteCustomLibraryClick($index) {
        this.customLibraries.splice($index, 1);
    }

    onSaveCustomLibrariesClick() {
        this.running = "save-custom-libraries";
        this.awaitAction = {
            title: "Saving Custom Libraries",
            subtitle: "Just a moment for saving the custom libraries...",
        };

        this.apiService.post("Module", "SaveCustomLibraries", this.customLibraries, { moduleId: this.module.Id }).then((data) => {
            this.notifyService.success("Module custom libraries updated has been successfully");

            this.disposeWorkingMode();

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            delete this.running;
        });
    }

    onCancelCustomLibrariesClick() {
        this.disposeWorkingMode();
    }

    /*------------------------------------*/
    /* Common Methods */
    /*------------------------------------*/
    disposeWorkingMode() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
    }

    onExpressionParsingTypeChange(item, type) {
        item.ExpressionParsingType = type;
    }

    onSearchTextChange() {
        this.fieldsBackup = this.fieldsBackup ?? _.cloneDeep(this.fields);

        this.fields = _.filter(this.fieldsBackup, (f) => { return (f.FieldName ?? '').indexOf(this.searchText) >= 0 || (f.FieldText ?? '').indexOf(this.searchText) >= 0 || (f.FieldType ?? '').indexOf(this.searchText) >= 0 });

        this.renderDesignForm();
    }

    //#region Monitoring Methods

    monitorbuilding(hub, mode, content) {
        if (mode == 'log') {
            $(`#buildLogs${this.module.Id}`).append(`<li>${content}</li>`)
            $(`#buildLogs${this.module.Id}`).scrollTop($(`#buildLogs${this.module.Id}`)[0].scrollHeight);
        }
        else if (mode = 'progress') {
            let items = (content ?? '').split(',');
            if (items.length == 2) {
                const state = items[0];
                const progress = items[1];

                if (state == 'module-template')
                    $(`#moduleTemplateProgress${this.module.Id}`).css('width', `${progress}%`);
                else if (state == 'fields-scripts')
                    $(`#fieldsScriptsProgress${this.module.Id}`).css('width', `${progress}%`);
                else if (state == 'actions-scripts')
                    $(`#actionsScriptsProgress${this.module.Id}`).css('width', `${progress}%`);
                else if (state == 'module-styles')
                    $(`#moduleStylesProgress${this.module.Id}`).css('width', `${progress}%`);
                else if (state == 'resource-libraries')
                    $(`#moduleLibrariesProgress${this.module.Id}`).css('width', `${progress}%`);
            }
        }
    }

    monitoring(hub, mode, content) {
        if (mode == 'log') {
            $('.await-action-subtitle').text(content);
        } else if (mode = 'progress') {
            $(`#progress${hub.handler.currentTabKey}`).css('width', content + '%');
            $(`#progress${hub.handler.currentTabKey}`).text(content + '%');
        }
    }

    //#endregion

    /*------------------------------------*/
    /*  Drag & Drop Methods */
    /*------------------------------------*/
    onFieldDragOver($event, ui) {
        $($event.target).addClass("drag");
    }

    onFieldDragOut($event, ui) {
        $($event.target).removeClass("drag");
    }

    onFieldDrop($event, ui, paneName, parentId, beforeFieldId) {
        const $element = $($event.target);
        $element.removeClass("drag");

        paneName = paneName ? paneName : $element.attr("field-drop");

        parentId = parentId ? parentId : $element.attr("data-parent-id");

        const fieldTypeName = $(ui.draggable[0]).data("field-type");

        _.filter(this.fieldTypes, (fieldType) => {
            return fieldType.FieldType == fieldTypeName;
        }).map((fieldType) => {
            this.addField(paneName, parentId, fieldType, beforeFieldId);
        });
    }

    onStopDrag($event, ui, $index) {
        $($event.target).css("top", 0);
        $($event.target).css("left", 0);
    }
}