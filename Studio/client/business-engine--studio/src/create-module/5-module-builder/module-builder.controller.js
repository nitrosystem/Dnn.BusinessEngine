import { GlobalSettings } from "../../angular-configs/global.settings";
import Swal from 'sweetalert2'
import moment from "moment";
import 'animate.css';

import editLayoutTemplateWidget from "./edit-layout-template.html";
import fieldTemplateWidget from "./field-options/field-template.html";
import fieldEditWidget from "./field-options/field-edit.html";
import fieldSettingsWidget from "./field-options/field-settings.html";
import fieldShowConditionsWidget from "./field-options/field-show-conditions.html";
import fieldConditionalValuesWidget from "./field-options/field-conditional-values.html";
import fieldDataSourceWidget from "./field-options/field-data-source.html";

export class CreateModuleModuleBuilderController {
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
        $deferredBroadcast,
        moduleDesignerService
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

        this.currentTabKey = this.$rootScope.currentTab.key;

        this.editLayoutTemplateWidget = editLayoutTemplateWidget;
        this.fieldTemplateWidget = fieldTemplateWidget;
        this.fieldEditWidget = fieldEditWidget;
        this.fieldSettingsWidget = fieldSettingsWidget;
        this.fieldShowConditionsWidget = fieldShowConditionsWidget;
        this.fieldConditionalValuesWidget = fieldConditionalValuesWidget;
        this.fieldDataSourceWidget = fieldDataSourceWidget;

        this.field = {};
        this.backupFields = {};

        this.roles = this.$rootScope.roles;

        this.$rootScope.createModuleValidatedStep.push(5);

        $scope.$on("onCreateModuleValidateStep5", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        $scope.$on("onCreateModuleValidateStep5", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        $scope.$on("onShowFieldDataSource", (e, args) => {
            this.onShowFieldDataSourceClick(args.field);
        });

        $scope.$on("onReloadModuleBuilder", (e, args) => {
            if (args.type === 0 && args.id) this.onPageLoad();
        });

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
                this.onRefreshFieldClick(e, this.currentField.Id);

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
                const $field = this.getFieldElementByFieldId(this.currentField.Id);
                const $target = e.key == 'ArrowUp' ? $field.previousElementSibling : $field.nextElementSibling;
                if ($target && $target.attributes['b-field']) {
                    const fieldId = $target.attributes['b-field'].value;
                    this.onFieldItemClick(e, fieldId);
                    if (!$scope.$$phase) $scope.$apply();

                    this.scrollToFieldSection(this.currentField.Id)
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
            { l: 'id', r: this.globalService.getParameterByName("id"), isPageParam: true }
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
            { l: 'id', r: this.globalService.getParameterByName("id"), isPageParam: true }
        ]);

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");

        this.running = "get-module-builder";
        this.awaitAction = {
            title: "Get Module Builder Data",
            subtitle: "Just a moment for loading module builder data..."
        };

        this.apiService.get("Module", "GetModuleBuilder", { moduleId: id || null }).then((data) => {
            this.module = data.Module;
            this.fields = data.Fields;
            this.variablesAsList = data.VariablesAsList;

            this.fieldTypes = this.parseFieldTypes(data.FieldTypes);

            this.module.PreloadingTemplateBackup = this.module.PreloadingTemplate;
            this.module.LayoutTemplateBackup = this.module.LayoutTemplate;
            this.module.LayoutCssBackup = this.module.LayoutCss;

            _.forEach(this.fields, (field) => {
                field.FieldTypeBackup = field.FieldType;
                field.FieldTypeObject = _.find(this.fieldTypes, (ft) => {
                    return ft.FieldType == field.FieldType;
                }) ?? {
                    FieldComponent: 'b-not-field-type'
                };

                this.field[field.FieldName] = field;
                this.backupFields[field.Id] = angular.copy(field);
            });

            let newKey = this.globalService.getParameterByName("key") || null;
            if (!newKey)
                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.module.Id,
                    title: this.module.ModuleName,
                });

            if (this.module.LayoutTemplate) {
                this.$timeout(() => {
                    this.renderDesignForm();
                });
            }

            this.$scope.$emit("onChangeActivityBar", {
                name: "builder",
                title: "Module Builder",
            });

            this.onSidebarTabClick('toolbox');

            delete this.currentField;
            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });

        this.setForm();
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

    validateStep(task, args) {
        task.wait(() => {
            const $defer = this.$q.defer();

            $defer.resolve(true);

            return $defer.promise;
        });
    }

    /*------------------------------------*/
    /* Build Module & Render Design Module Methods  */
    /*------------------------------------*/
    buildModule(scope) {
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

        this.apiService.post("Module", "BuildModule", null, { moduleId: this.module.Id }).then((data) => {
            this.notifyService.success("Build module has been successfully!. ;)");

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
            helper: 'clone',
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

                    this.sortPaneFields(paneName);
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
    /* Appearance Methods  */
    /*------------------------------------*/
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

    onShowLayoutTemplateClick() {
        this.workingMode = "module-edit-layout-template";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onSaveTemplateWidgetClick($event) {
        this.onSaveFieldClick($event);
    }

    onCancelLayoutTemplate() {

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

    onToggleSidebarMenuClick() {
        const moduleId = this.module.ParentId ? this.module.ParentId : this.module.Id;
        $(`#sidebarMenu${moduleId}`).toggleClass('show-menu');
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

    onFieldItemClick($event, Id) {
        const field = this.getFieldById(Id);

        if (this.currentField && this.currentField.Id == field.Id) return;

        this.currentField = field;
        this.field[this.currentField.FieldName] = this.currentField;
        this.currentFieldBackup = angular.copy(this.currentField);

        this.fieldActionsFilter = this.currentField.Id;
        if (this.currentSidebarTab != 'actions') this.onSidebarTabClick("field-settings");

        this.onFieldSettingsClick($event)

        this.currentFieldFocused = true;

        if ($event) $event.stopPropagation();
    }

    onFieldToolbarClick($event) {
        this.currentFieldFocused = !!this.currentField;

        $event.stopPropagation();
    }

    onFieldItemBlur($event, field) {
        if ($event.target.contains($event.relatedTarget)) return;

        this.currentFieldFocused = false;

        if ($event) $event.stopPropagation();
    }

    onChangeFieldTypeClick($event, fieldId) {
        Swal.fire({
            title: '<h4>Are you sure change the field type?</h4>',
            html: '<p>Some the field settings may be lost after the field type is changed</p>',
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, change it!",
            backdrop: false,
        }).then((result) => {
            if (result.isConfirmed) {
                this.currentField.FieldType = angular.copy(this.currentField.FieldTypeBackup);
                this.saveCurrentField().then(() => {
                    location.reload();
                });
            }
        });
    }

    onCancelPaneSortFields(id) {
        var field = angular.copy(this.backupFields[id]);
        this.field[field.FieldName].PaneName = field.PaneName;

        const index = _.findIndex(this.fields, (f) => { return f.Id == id });
        this.fields[index].PaneName = field.PaneName;

        const $pane = $(`div[data-pane="${field.PaneName}"]`);
        const $field = this.getFieldElementByFieldId(field.Id);

        if ($pane.find($field).length == 0) $pane.append($field);
    }

    onCancelEditFieldClick($event, id) {
        var field = angular.copy(this.backupFields[id]);
        if (field) {
            const index = this.getfieldByIndex(id)
            if (index >= 0) this.fields[index] = field;

            this.field[field.FieldName] = field;
        }

        if (this.currentField && this.currentField.Id == id) this.removeCurrentField(true);

        if ($event) $event.stopPropagation();
    }

    addField(paneName, parentId, fieldType, beforeField) {
        const suggestFieldName = fieldType.FieldType + (_.filter(this.fields, (f) => { return f.FieldType == fieldType.FieldType; }).length + 1);
        const defaultSettings = fieldType.DefaultSettings || {};

        this.currentField = {
            ModuleId: this.module.Id,
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
            DataSource: {},
            Settings: defaultSettings,
        };

        this.beforeField = beforeField;

        this.workingMode = "field-edit";
        this.$scope.$emit("onShowRightWidget", { controller: this });
        this.focusColumnForm();

        this.fieldOptions = window[`${this.currentField.FieldType}Options`];

        this.$deferredBroadcast(this.$scope, `onAdded${this.currentField.FieldType}Field`, { field: this.currentField, controller: this });
    }

    onSaveFieldClick($event, isNewField, changeEditStatus) {
        var field = this.currentField;
        this.form.validated = true;
        this.form.validator(field);
        if (this.form.valid) {
            if (isNewField) {
                this.saveCurrentField(field, isNewField).then((field) => {
                    field.FieldTypeObject = _.find(this.fieldTypes, (ft) => {
                        return ft.FieldType == field.FieldType;
                    });

                    field.FieldTypeBackup = field.FieldType;

                    this.fields.push(field);
                    this.field[field.FieldName] = field;
                    this.currentField = field;

                    this.currentFieldBackup = angular.copy(field);

                    delete this.currentFieldBackup;

                    this.moduleDesignerService.getFieldUI(field, this.$scope).then(($fieldItem) => {
                        const $field = this.$compile($fieldItem)(this.$scope);

                        if (this.beforeField) {
                            $($field).insertBefore(
                                $("#board" + this.module.Id).find('*[b-field="' + this.beforeField + '"]')
                            );
                        } else {
                            const $boardPane = $("#board" + this.module.Id).find('*[data-pane="' + field.PaneName + '"]');
                            $boardPane.append($field);
                        }

                        if (field.IsGroup) this.renderDesignForm();

                        this.$timeout(() => {
                            this.disposeWorkingMode();
                            this.onFieldSettingsClick();
                        });
                    });
                });
            }
            else this.saveCurrentField().then(() => {
                if (changeEditStatus) delete this.currentField
            });

            if ($event) $event.stopPropagation();
        }
    }

    saveCurrentField(targetField, isNewField) {
        const $defer = this.$q.defer();

        targetField = targetField ?? this.currentField;
        var field = angular.copy(targetField);
        delete field.Value;

        let guids = [...document.querySelectorAll(`.pane-body[data-pane="${field.PaneName}"] [b-field]`)].map(el => el.getAttribute('b-field'));

        if (isNewField && !this.beforeField) field.ViewOrder = guids.length;

        var postData = {
            Field: field
        }

        if (!!this.beforeField) {
            postData = {
                ...postData,
                ...{
                    ReorderFields: true,
                    PaneFieldIds: guids,
                    FieldViewOrder: guids.indexOf(this.beforeField)
                }
            }
        }

        this.running = "save-field";
        this.awaitAction = {
            title: "Saving Field",
            subtitle: "Just a moment for saving the field..."
        };

        this.apiService.post("Module", "SaveModuleField", postData).then((id) => {
            this.notifyService.success(field.FieldName + " Field updated has been successfully");

            field.Id = id;

            this.backupFields[field.Id] = angular.copy(field);

            this.disposeWorkingMode();

            delete this.awaitAction;
            delete this.running;

            $defer.resolve(field);

        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;

            $defer.reject(error);
        });

        return $defer.promise;
    }

    focusColumnForm() {
        this.$timeout(() => {
            this.$scope.$broadcast("onEditField");
        }, 500);
    }

    onCloseWindowClick() {
        this.disposeWorkingMode();
    }

    onCancelFieldClick() {
        this.disposeWorkingMode();
    }

    onDeleteFieldClick($event) {
        let timerInterval;
        Swal.fire({
            title: "<h4>Are you sure delete this field!?</h4><b></b>",
            icon: "warning",
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, delete it!",
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector("b");
                var timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                this.currentField.isDeleted = true;
                var field = angular.copy(this.currentField);

                this.running = "delete-field";
                this.awaitAction = {
                    title: "Deletin Field",
                    subtitle: "Just a moment for removing the field...",
                };

                this.apiService.post("Module", "DeleteModuleField", { Id: field.Id }).then((data) => {
                    _.filter(this.fields, (f) => { return f.Id == field.Id; }).map((field) => {
                        $("#board" + this.module.Id).find('*[b-field="' + field.Id + '"]').remove();

                        this.fields.splice(this.fields.indexOf(field), 1);

                        delete this.field[field.FieldName];
                        delete this.awaitAction;
                        delete this.running;
                    });
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                    this.notifyService.error(error.data.Message);

                    delete this.running;
                });
            }
        });
    }

    getPaneElementByPaneName(paneName) {
        const $pane = $(`*[data-pane="${paneName}"]`);
        return $pane;
    }

    sortPaneFields(paneName) {
        let serviceName = 'SortModuleFields';
        if (this.currentField.PaneName != this.currentFieldBackup.PaneName)
            serviceName = 'UpdateModuleFieldPaneAndReorderFields';

        const guids = [...document.querySelectorAll(`.pane-body[data-pane="${paneName}"] [b-field]`)].map(el => el.getAttribute('b-field'));

        var postData = {
            ModuleId: this.module.Id,
            FieldId: this.currentField.Id,
            PaneName: paneName,
            PaneFieldIds: guids
        }

        this.apiService.post("Module", serviceName, postData)
            .then(() => {
                this.notifyService.success("the fields sort has been successfully");

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

    removeCurrentField(changeTab) {
        delete this.currentField;
        delete this.currentFieldBackup;

        this.onSidebarTabClick("toolbox");

        return true;
    }

    getFieldById(fieldId) {
        return _.find(this.fields, (field) => { return field.Id == fieldId; });
    }

    getFieldByName(fieldname) {
        var result;

        _.filter(this.fields, (field) => {
            return field.FieldName == fieldname;
        }).map((field) => (result = field));

        return result;
    }

    getfieldByIndex(id) {
        return _.findIndex(this.fields, (f) => { return f.Id == id });
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
        const $field = this.getFieldElementByFieldId(this.currentField.Id);
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
            const $field = this.getFieldElementByFieldId(this.currentField.Id);

            if ($pane.find($field).length == 0) $pane.append($field);

            this.currentField.PaneName = pane.paneName;
            this.currentField.ParentId = pane.parentId;

            this.scrollToFieldSection(this.currentField.Id)
            this.sortPaneFields(pane.paneName);
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
        if (!this.currentField.DataSource || !this.currentField.DataSource.Type) {
            this.running = "get-field-data-source";
            this.awaitAction = {
                title: "Loading Field Data Source",
                subtitle: "Just a moment for loading the field data source...",
            };

            this.apiService.get("Studio", "GetDefinedListByFieldId", {
                fieldId: this.currentField.Id,
            }).then((data) => {
                this.definedList = data ?? { Items: [] };

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
        else if (this.currentField.DataSource && this.currentField.DataSource.Type == 1) {
            if (this.definedLists) {
                this.onDefinedListChange();
                return;
            }

            this.running = "get-defined-lists";
            this.awaitAction = {
                title: "Loading Defined Lists",
                subtitle: "Just a moment for loading the defined lists...",
            };

            this.apiService.get("Module", "GetDefinedLists", { fieldId: this.currentField.Id, }).then((data) => {
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
        else if (this.currentField.DataSource && this.currentField.DataSource.Type == 2) {
            this.onDataSourceVariableChange();
        }
    }

    onSaveFieldDataSourceClick($event) {
        if (this.currentField.DataSource.Type == 0) {
            if (!this.definedList.ListName) this.definedList.ListName = this.currentField.FieldName + '_Options';
            this.definedList.FieldId = this.currentField.Id;

            this.saveDefinedList($event).then(() => {
                this.saveDataSource($event);
            });
        }
        else
            this.saveDataSource($event);
    }

    saveDefinedList() {
        const $defer = this.$q.defer();

        this.running = "save-field-data-source";
        this.awaitAction = {
            title: "Saving Field Data Source",
            subtitle: "Just a moment for saving the field data source...",
        };

        this.apiService.post("Studio", "SaveDefinedList", this.definedList).then((data) => {
            this.notifyService.success(
                "Field data source saved has been successfully"
            );

            this.currentField.DataSource.ListId = data;
            this.currentField.DataSource.Items = angular.copy(this.definedList.Items);

            $defer.resolve();

            delete this.definedList;
            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            $defer.reject();

            delete this.definedList;
            delete this.running;
        });

        return $defer.promise;
    }

    saveDataSource($event) {
        if (
            (this.currentField.DataSource.Type == 0 || this.currentField.DataSource.Type == 1) &&
            (!this.currentField.DataSource.TextField || !this.currentField.DataSource.ValueField)) {
            this.currentField.DataSource.TextField = "Text";
            this.currentField.DataSource.ValueField = "Value";
        }

        this.$timeout(() => this.onSaveFieldClick($event));

        this.disposeWorkingMode();
    }

    onDefinedListChange() {
        this.definedList = this.definedList || {};
        this.definedList = _.find(this.definedLists, (i) => { return i.ListId == this.currentField.DataSource.ListId });
    }

    onCancelFieldDataSourceClick() {
        if (this.fieldDataSourceBackup) {
            this.currentField.DataSource = angular.copy(this.fieldDataSourceBackup);

            delete this.fieldDataSourceBackup;
        }

        this.onCancelFieldClick();
    }

    onEditFieldDataSourceClick($event, fieldId) {
        this.onShowFieldDataSourceClick($event, fieldId);
    }

    onDataSourceVariableChange() {
        const variableName = this.currentField.DataSource.VariableName;
        this.currentFieldVariableProperties =
            _.find(this.variablesAsList, (v) => { return v.VariableName == variableName }).Properties;
    }

    /*------------------------------------*/
    /* Actions & Conditions Methods  */
    /*------------------------------------*/
    onShowActionsClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 5 })
    }

    onShowFieldActionsClick($event) {
        this.fieldActionsFilter = this.currentField.Id;

        this.onSidebarTabClick('actions');
        this.$rootScope.currentActivityBar = "builder";

        if ($event) $event.stopPropagation();
    }

    onShowConditionsClick($event, fieldId) {
        if (fieldId) this.onFieldItemClick($event, fieldId);

        this.workingMode = "field-show-conditions";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onShowConditionalValuesClick($event, fieldId) {
        if (fieldId) this.onFieldItemClick($event, fieldId);

        this.workingMode = "field-conditional-values";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onAddFieldConditionalValueClick() {
        this.currentField.FieldValues = this.currentField.FieldValues || [];
        this.currentField.FieldValues.push({ Conditions: [] });
    }

    onAddActionClick(fieldType, fieldId) {
        const page = {
            page: "create-action",
            subParams: {
                module: this.module.Id,
                ...(fieldType && { type: fieldType }),
                ...(fieldId && { field: fieldId }),
            }
        };

        this.$scope.$emit("onGotoPage", page);
    }

    onEditActionClick(actionId, fieldType, fieldId) {
        const page = {
            page: "create-action",
            id: actionId,
            subParams: {
                module: this.module.Id,
                ...(fieldType && { type: fieldType }),
                ...(fieldId && { field: fieldId }),
            }
        };

        this.$scope.$emit("onGotoPage", page);
    }

    onGotoActionsPageClick() {
        var subParams = {};
        if (this.fieldActionsFilter) subParams.type = "field";
        var parentId = this.fieldActionsFilter ? this.currentField.Id : undefined;
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
        this.workingMode = "field-template";
        this.$scope.$emit("onShowRightWidget", { controller: this });
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

    onSearchTextChange() {
        this.fieldsBackup = this.fieldsBackup ?? angular.copy(this.fields);

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

    onFieldDrop($event, ui, paneName, parentId, beforeField) {
        const $element = $($event.target);
        $element.removeClass("drag");

        paneName = paneName ? paneName : $element.attr("field-drop");
        parentId = parentId ? parentId : $element.attr("data-parent-id");

        const fieldTypeName = $(ui.draggable[0]).data("field-type");

        _.filter(this.fieldTypes, (fieldType) => {
            return fieldType.FieldType == fieldTypeName;
        }).map((fieldType) => {
            this.addField(paneName, parentId, fieldType, beforeField);
        });
    }

    onStopDrag($event, ui, $index) {
        $($event.target).css("top", 0);
        $($event.target).css("left", 0);
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#region Dashboard Modules Methods

    onShowModuleTemplateClick() {
        this.workingMode = "module-template";
        this.$scope.$emit("onShowRightWidget", { controller: this });

        if (!this.templates) this.onReloadSkinsClick();
    }

    onReloadSkinsClick() {
        this.running = "reloading-module-skins";
        this.awaitAction = {
            title: "Reloading Skins",
            subtitle: "Just a moment for reloading the module skins...",
        };

        const dashboardModuleId = this.module.ParentId ? this.module.ParentId : this.module.Id;

        this.apiService.get("Module", "GetModuleTemplates", {
            dashboardModuleId: dashboardModuleId,
            moduleType: this.module.ModuleType
        }).then((data) => {
            this.templates = data;

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

    onGetTemplateContent(template) {
        this.running = "get-template-content";
        this.awaitAction = {
            title: "Loading Template Content",
            subtitle: "Just a moment for loading the module template content...",
        };

        this.module.Template = template.TemplateName;

        this.apiService.get("Module", "GetModuleTemplateContent", { templatePath: template.TemplatePath, }).then((data) => {
            this.template = { templatePreview: data }
            $('#templatePreview').modal('show');

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

    onApplyTemplateClick() {
        Swal.fire({
            title: 'Important Note!!',
            html: '<p>after apply change the template, the layout of the module will change, and will losted the Panes then you must sort the fields based on the new layout.</p>',
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, change it!",
            backdrop: false,
        }).then((result) => {
            if (result.isConfirmed) {
                this.module.LayoutTemplate = this.template.templatePreview;
                this.$timeout(() => {
                    this.disposeWorkingMode();
                    $('#templatePreview').modal('hide');
                    this.onSaveLayoutTemplateClick();
                }, 1500);
            }
        });
    }

    onSaveLayoutTemplateClick() {
        if (
            this.module.PreloadingTemplateBackup != this.module.PreloadingTemplate ||
            this.module.LayoutTemplateBackup != this.module.LayoutTemplate ||
            this.module.LayoutCssBackup != this.module.LayoutCss
        ) {
            this.apiService.post("Module", "SaveModuleTemplate", {
                ModuleId: this.module.Id,
                Template: this.module.Template,
                LayoutTemplate: this.module.LayoutTemplate,
                LayoutCss: this.module.LayoutCss
            }).then((data) => {
                if (data)
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

            this.disposeWorkingMode();
            this.renderDesignForm();

        }
        else
            this.disposeWorkingMode();
    }

    onSelectSkinTemplateClick(template) {
        if (this.module.skinIsEmpty || this.module.Template != template.TemplateName) this.isSkinOrTemplateChanged = true;

        this.moduleTemplateBackup = this.module.Template;
        this.module.Template = template.TemplateName;
    }

    onModuleChangeDetected() {
        this.lastBuildActivity = moment();
        this.scheduleBuild();
    }

    //#endregion

    onPreviousStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 3 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 5 });
    }

    validateStep(task, args) {
        task.wait(() => {
            const $defer = this.$q.defer();

            $defer.resolve(true);

            return $defer.promise;
        });
    }
}