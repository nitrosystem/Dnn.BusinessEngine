import Swal from 'sweetalert2'
import { GlobalHelper, UrlHelper, JsonHelper } from '../../../shared/helpers';
import { GlobalUtil } from '../../../shared/utils';
import { Sortable, Dragable } from '../../../shared/libs';
import editLayoutTemplateWidget from './edit-layout-template.html';
import fieldTemplateWidget from './field-options/field-template.html';
import fieldEditWidget from './field-options/field-edit.html';
import fieldSettingsWidget from './field-options/field-settings.html';
import fieldDataSourceWidget from './field-options/field-data-source.html';

export class CreateModuleModuleBuilderController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        $compile,
        $q,
        apiService,
        actionCenterService,
        validationService,
        notificationService,
        baseOptions,
        moduleDesignerService
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$compile = $compile;
        this.$q = $q;
        this.apiService = apiService;
        this.validationService = validationService;
        this.actionCenterService = actionCenterService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;
        this.moduleDesignerService = moduleDesignerService;

        this.editLayoutTemplateWidget = editLayoutTemplateWidget;
        this.fieldTemplateWidget = fieldTemplateWidget;
        this.fieldEditWidget = fieldEditWidget;
        this.fieldSettingsWidget = fieldSettingsWidget;
        this.fieldDataSourceWidget = fieldDataSourceWidget;

        this.currentTabKey = this.$rootScope.currentTab?.key;
        this.uiState = {};
        this.version = this.baseOptions.version;

        $rootScope.$on('onUpdateVariables', (e, args) => {
            if (args.moduleId === this.module.Id)
                this._getVariables();
        });

        $rootScope.$on('onUpdateActions', (e, args) => {
            if (args.moduleId === this.module.Id && args.fieldId)
                this._getFieldActions(args.fieldId);
        });

        $scope.$parent.createModuleValidatedStep.push(5);
        $scope.$on('onCreateModuleValidateStep5', (e, defer) => {
            defer.resolve(true);
        });

        $scope.$on('onCreateModuleGotoStep5', (e, task, args) => {
            this.onFocusModule();
        });

        $scope.$on('onShowFieldDataSource', (e, args) => {
            this.onShowFieldDataSourceClick(args.field);
        });

        const currentTabKey = this.$rootScope.currentTab.key;
        $scope.$on(`onTab--${currentTabKey}--Selected`, (e, args) => {
            this.onFocusModule();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');

        this.running = 'get-module-builder';
        this.awaitAction = {
            title: 'Get Module Builder Data',
            subtitle: 'Just a moment for loading module builder data...'
        };

        this.apiService.get('Module', 'GetModuleBuilder', { moduleId: id }).then((data) => {
            this.module = data.Module;
            this.fieldTypes = data.FieldTypes;
            this.fields = data.Fields;
            JsonHelper.parseJsonItems(this.fields);
            this.field = {};

            for (const field of this.fields) {
                field.FieldTypeBackup = field.FieldType;
                field.FieldTypeObject = this.fieldTypes.find(ft => ft.FieldType === field.FieldType) ?? {}

                this.field[field.FieldName] = field;
            }

            if (this.module.LayoutTemplate)
                this.$timeout(() => this.onRenderDesignFormClick());
            else
                this.onShowLayoutTemplateClick();

            this.onSearchComponentChange();
            this.onFocusModule();
            this.onSidebarTabClick('toolbox');

            this._setForm();
            this._processVariables(data.Variables);
            this._registerWindowEvents();

            this.$scope.$emit('onUpdateCurrentTab', {
                id: this.module.Id,
                title: this.module.ModuleName,
            });

            this.currentField = null;
            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onFocusModule() {
        this.$scope.$emit('onChangeActivityBar', {
            name: 'builder',
            title: 'Module Builder',
        });
    }

    onSetValidFieldNameClick() {
        this.currentField.FieldName = GlobalHelper.normalizeName(this.currentField.FieldName);
    }

    onBuildModuleClick() {
        this.running = 'building-module';
        this.awaitAction = {
            title: 'Building Module...',
            subtitle: 'Just a moment for building module without tracing...',
            subtitleColor: '#fff',
        };

        this.apiService.post('Module', 'BuildModule', null, { moduleId: this.module.Id }).then(() => {
            this.notifyService.build('Build module has been successfully!.');

            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onRenderDesignFormClick(showNotify) {
        const defer = this.$q.defer();

        this.moduleDesignerService.renderDesignForm(this.module, this.fields, this.$scope)
            .then((data) => {
                $('#board' + this.module.Id).html(data.$board);
                this.panes = data.panes;

                this._setFieldsSortingUi();
                this._setFieldsDragingUi();

                if (showNotify) this.notifyService.build('Render form has been successfully!.');

                defer.resolve();
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.message);
            });

        return defer.promise;
    }

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
    }

    onSetFieldPropertyValueClick(fieldId) {
        this.onSidebarTabClick('field-settings');
        this.$timeout(() => {
            $('.tab-contents').animate({
                scrollTop: $(`[editor-id="monacoFieldValueProperty${fieldId}"]`).position().top
            }, 50);

            this.$scope.$broadcast('onFocusFieldValueProperty');
        }, 500);
    }

    onSearchComponentChange() {
        const input = this.searchComponent?.toLowerCase() || '';
        this.components = this.fieldTypes
            .filter(ft => ft.FieldType?.toLowerCase().includes(input) || ft.Title?.toLowerCase().includes(input))
            .map(ft => {
                return {
                    FieldDomain: ft.IsPopular
                        ? '_Popular'
                        : ft.FieldDomain,
                    FieldType: ft.FieldType,
                    Title: ft.Title,
                    Icon: ft.Icon
                }
            })
            .sort((a, b) => a.FieldDomain.localeCompare(b.FieldDomain));

        this.$timeout(() => this._setFieldsDragingUi());
    }

    onPreviousStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 3 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 5 });
    }

    onCloseFieldWindow() {
        this._disposeWorkingMode();
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#region Field Toolbar Methods

    onFieldToolbarClick($event) {
        this.currentFieldFocused = !!this.currentField;

        $event.stopPropagation();
    }

    onFieldReloadClick($event, fieldId) {
        this.running = 'refresh-field';
        this.awaitAction = {
            title: 'Refresh Field',
            subtitle: 'Just a moment for refresh the field...',
        };

        this.apiService.get('Module', 'GetModuleField', { fieldId }).then((data) => {
            const index = this._getFieldIndex(fieldId);
            if (index >= 0) {
                this.fields[index] = data;
                this.field[data.FieldName] = this.currentField = data;
            }

            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onFieldItemClick($event, Id) {
        const field = this._getFieldById(Id);
        if (field.Id === this.currentField?.Id) {
            $event.stopPropagation();
            return false;
        }

        this.currentField = field;
        this.field[this.currentField.FieldName] = this.currentField;
        this.currentFieldBackup = GlobalHelper.clone(this.currentField);

        this.fieldActionsFilter = this.currentField.Id;
        if (this.currentSidebarTab !== 'actions') this.onSidebarTabClick('field-settings');

        this.onFieldSettingsClick($event)

        this.currentFieldFocused = true;

        if ($event) $event.stopPropagation();
    }

    onFieldItemBlur($event) {
        this.currentFieldFocused = false;

        if ($event.target.contains($event.relatedTarget)) return;
        if ($event) $event.stopPropagation();
    }

    onChangeFieldTypeClick($event) {
        Swal.fire({
            title: '<h4>Are you sure change the field type?</h4>',
            html: '<p>Some the field settings may be lost after the field type is changed</p>',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, change it!',
            backdrop: false,
        }).then((result) => {
            if (result.isConfirmed) {
                this.currentField.FieldType = GlobalHelper.clone(this.currentField.FieldTypeBackup);
                this._saveCurrentField().then(() => {
                    location.reload();
                });
            }
        });

        if ($event) $event.stopPropagation();
    }

    onCancelFieldClick($event, fieldId) {
        const field = GlobalHelper.clone(this.currentFieldBackup);
        if (field) {
            const index = this._getFieldIndex(fieldId)
            if (index >= 0) this.fields[index] = field;
            this.field[field.FieldName] = field;
        }

        if (this.currentField?.Id === fieldId)
            this._removeCurrentField(true);

        this._disposeWorkingMode();

        if ($event) $event.stopPropagation();
    }

    onSaveFieldClick($event, isNewField, changeEditStatus) {
        if (!isNewField) this.onSidebarTabClick('field-settings');

        if (this.currentField.Settings.IsDisabledLayout)
            this.currentField.Settings.IsHiddenFieldText = true;

        this.fieldForm.validated = true;
        this.fieldForm.validator(this.currentField);
        if (this.fieldForm.valid) {
            if (isNewField) {
                this._saveCurrentField(this.currentField, isNewField).then((field) => {
                    field.FieldTypeObject = this.fieldTypes.find(ft => ft.FieldType === field.FieldType);
                    field.FieldTypeBackup = field.FieldType;

                    this.fields.push(field);
                    this.field[field.FieldName] = field;
                    this.currentField = field;
                    this.currentFieldBackup = GlobalHelper.clone(field);

                    this.moduleDesignerService.getFieldUI(field, this.$scope).then(($fieldItem) => {
                        const $field = this.$compile($fieldItem)(this.$scope);

                        if (this.currentField._beforeFieldId) {
                            $($field).insertBefore(
                                $('#board' + this.module.Id).find(`*[b-field="${this.currentField._beforeFieldId}"]`)
                            );
                        } else {
                            const $boardPane = $('#board' + this.module.Id).find(`*[data-pane="${field.PaneName}"]`);
                            $boardPane.append($field);
                        }

                        if (field.IsGroupField) this.onRenderDesignFormClick();

                        this._disposeWorkingMode();
                        this.$timeout(() => this.onFieldSettingsClick());
                    });
                });
            }
            else {
                this._saveCurrentField().then(() => {
                    if (changeEditStatus) {
                        this.currentField = null;
                        this.onSidebarTabClick('toolbox');
                    }
                });
            }

            if ($event) $event.stopPropagation();
        }
    }

    onDeleteFieldClick($event) {
        let timerInterval;
        Swal.fire({
            title: '<h4>Are you sure delete this field!?</h4><b></b>',
            icon: 'warning',
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!',
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector('b');
                timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const field = this.currentField;

                this.running = 'delete-field';
                this.awaitAction = {
                    title: 'Delete Field',
                    subtitle: 'Just a moment for deleting the field...',
                };

                this.apiService.post('Module', 'DeleteModuleField', { Id: field.Id }).then((data) => {
                    if (data) {
                        $('#board' + this.module.Id).find(`*[b-field="${field.Id}"]`).remove();

                        this.field[field.FieldName] = null;
                        const index = this._getFieldIndex(field.Id);
                        if (index >= 0) this.fields.splice(index, 1);
                    }

                    this.awaitAction = null;
                    this.running = null;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                    this.notifyService.error(error.data);

                    this.running = null;
                });
            }
        });

        if ($event) $event.stopPropagation();
    }

    onFieldSettingsClick($event) {
        this.$rootScope.currentActivityBar = 'builder';
        this.$scope.$broadcast('onBindFieldSettings_' + this.currentField.FieldType);
        this.onSidebarTabClick('field-settings');
    }

    onFieldSwap($event, actionType) {
        const $field = this._getFieldElementByFieldId(this.currentField.Id);
        const $target = actionType === 'up'
            ? $field.previousElementSibling
            : $field.nextElementSibling;

        if ($target && $target.attributes['b-field']) {
            if (actionType === 'up')
                $field.after($target);
            else if (actionType === 'down')
                $field.before($target);

            this._sortPaneFields(this.currentField.PaneName, this.currentField.ParentId);
        }

        if ($event) $event.stopPropagation();
    }

    onFieldChangePaneClick(pane, $event) {
        if (this.currentField.PaneName !== pane.paneName) {
            const $pane = $(`div[data-pane='${pane.paneName}']`);
            const $field = this._getFieldElementByFieldId(this.currentField.Id);
            if ($pane.find($field).length === 0) $pane.append($field);

            this.currentField.PaneName = pane.paneName;
            this.currentField.ParentId = $pane.attr('data-parent-id');

            this._scrollToFieldSection(this.currentField.Id)
            this._sortPaneFields(this.currentField.PaneName, this.currentField.ParentId);
        }

        if ($event) $event.stopPropagation();
    }

    //#endregion

    //#region Action Methods

    onAddActionClick(fieldType, fieldId, $event) {
        const page = {
            page: 'create-action',
            subParams: {
                module: this.module.Id,
                ...(fieldType && { type: fieldType }),
                ...(fieldId && { field: fieldId })
            }
        };
        this.$scope.$emit('onGotoPage', page);

        if ($event) $event.stopPropagation();
    }

    onEditActionClick(actionId, fieldType, fieldId, $event) {
        const page = {
            page: 'create-action',
            id: actionId,
            subParams: {
                module: this.module.Id,
                ...(fieldType && { type: fieldType }),
                ...(fieldId && { field: fieldId })
            }
        };
        this.$scope.$emit('onGotoPage', page);

        if ($event) $event.stopPropagation();
    }

    onGotoActionsPageClick() {
        const subParams = {};
        if (this.fieldActionsFilter)
            subParams.type = 'field';
        const parentId = this.fieldActionsFilter && this.currentField
            ? this.currentField.Id
            : undefined;
        const title = this.fieldActionsFilter && this.currentField
            ? this.currentField.FieldName + ' Actions'
            : this.module.ModuleName + ' Actions';

        this.$scope.$emit('onGotoPage', {
            page: 'actions',
            parentId: parentId,
            title: title,
            subParams: subParams,
        });
    }

    //#endregion

    //#region Field Datasource Methods

    onShowFieldDataSourceClick($event, fieldId) {
        if (this.currentField?.Id !== fieldId) this.onFieldItemClick($event, fieldId);
        if (!this.currentField.HasDataSource) return;

        this.workingMode = 'field-data-source';
        this.$scope.$emit('onShowRightWidget', { controller: this });

        this.fieldDataSourceBackup = GlobalHelper.clone(this.currentField.DataSource ?? {});
        this.currentField.DataSource = this.currentField.DataSource ?? {};

        this.onFieldDataSourceTypeChange();

        this.running = null;
        this.awaitAction = null;

        if ($event) $event.stopPropagation();
    }

    onEditFieldDataSourceClick($event, fieldId) {
        this.onShowFieldDataSourceClick($event, fieldId);
    }

    onFieldDataSourceTypeChange() {
        if (this.currentField.DataSource.Type === 0) {
            if (!this.currentField.DataSource.ListId)
                this.onShowCreateDefinedList();
            else
                this.onShowDefinedList();
        }
        else if (this.currentField.DataSource.Type === 1) {
            this.onDataSourceVariableChange();
        }
    }

    onSaveFieldDataSourceClick($event) {
        if (this.currentField.DataSource.Type === 0) {
            this._saveDefinedList($event).then((data) => {
                this.currentField.DataSource.ListId = data;
                this._saveFieldDataSource();
            });
        }
        else
            this._saveFieldDataSource();
    }

    onCancelFieldDataSourceClick() {
        if (this.fieldDataSourceBackup) {
            this.currentField.DataSource = GlobalHelper.clone(this.fieldDataSourceBackup);
            this.fieldDataSourceBackup = null;
        }

        this._disposeWorkingMode();
    }

    onDataSourceVariableChange() {
        const variableName = this.currentField.DataSource.VariableName;
        this.currentFieldVariableProperties =
            this.variablesAsDataSource.find(v => v.VariableName === variableName)?.Properties;
    }

    //#endregion

    //#region Defined List Methods

    onShowCreateDefinedList() {
        this.definedList = { ListName: this.currentField.FieldName + '_Options', Items: [] };
        this.currentField.DataSource.ListId = null;
        this.uiState.isDataSourceShowDefinedList = 0;
    }

    onShowDefinedList() {
        this._getDefinedLists();
        this.definedList = null;
        this.uiState.isDataSourceShowDefinedList = 1;
    }

    onDefinedListChange() {
        if (this.currentField.DataSource.ListId) {
            this.definedList = this.definedLists.find(i => i.Id === this.currentField.DataSource.ListId);
        }
    }

    //#endregion

    //#region Appearance Methods

    onShowFieldTemplateClick() {
        this.currentField.TemplateBackup = this.currentField.Template;
        this.currentField.ThemeBackup = this.currentField.Theme;

        this.workingMode = 'field-template';
        this.$scope.$emit('onShowRightWidget', { controller: this });
    }

    onSelectFieldTemplate(template) {
        this.currentField.Template = template.TemplateName;
        this.currentField.Theme = null;
    }

    onSelectFieldTheme(theme) {
        this.currentField.Theme = theme.ThemeName;
        this.currentField.ThemeCssClass = theme.ThemeCssClass
    }

    onSaveFieldTemplateClick() {
        this.currentField.TemplateBackup = null;
        this.currentField.ThemeBackup = null;

        this.onSaveFieldClick();
        this._disposeWorkingMode();
    }

    onCancelFieldTemplateClick() {
        this.currentField.Template = this.currentField.TemplateBackup;
        this.currentField.Theme = this.currentField.ThemeBackup;

        this.currentField.TemplateBackup = null;
        this.currentField.ThemeBackup = null;

        this._disposeWorkingMode();
    }

    onShowLayoutTemplateClick() {
        this.module.PreloadingTemplateBackup = this.module.PreloadingTemplate;
        this.module.LayoutTemplateBackup = this.module.LayoutTemplate;
        this.module.LayoutCssBackup = this.module.LayoutCss;

        this.workingMode = 'module-edit-layout-template';
        this.$scope.$emit('onShowRightWidget');
    }

    onCancelLayoutTemplateClick() {
        this.module.PreloadingTemplate = this.module.PreloadingTemplateBackup;
        this.module.LayoutTemplate = this.module.LayoutTemplateBackup;
        this.module.LayoutCss = this.module.LayoutCssBackup;

        this.module.PreloadingTemplateBackup = null;
        this.module.LayoutTemplateBackup = null;
        this.module.LayoutCssBackup = null;

        this._disposeWorkingMode();
    }

    onSaveLayoutTemplateClick() {
        if (
            this.module.PreloadingTemplateBackup !== this.module.PreloadingTemplate ||
            this.module.LayoutTemplateBackup !== this.module.LayoutTemplate ||
            this.module.LayoutCssBackup !== this.module.LayoutCss
        ) {
            this.running = 'saving-template';
            this.awaitAction = {
                title: 'Saving Template...',
                subtitle: 'Just a moment for saving module template...',
                subtitleColor: '#fff',
            };

            this.apiService.post('Module', 'SaveModuleTemplate', this.module).then((data) => {
                this.notifyService.info('Module layout template has been updated successfully.');

                this.module.PreloadingTemplateBackup = null;
                this.module.LayoutTemplateBackup = null;
                this.module.LayoutCssBackup = null;

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });

            this._disposeWorkingMode();
            this.onRenderDesignFormClick();
        }
        else
            this._disposeWorkingMode();
    }

    //#endregion

    //#endregion

    //#region Private Methods

    _setForm() {
        this.fieldForm = this.validationService.init({
            FieldName: {
                id: 'txtFieldName',
                rule: (value) => {
                    if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) === false)
                        return 'Field name is not valid';

                    return true;
                },
                required: true,
            },
            FieldText: {
                rule: (value) => {
                    if (!value && (!this.currentField?.Settings.IsDisabledLayout && !this.currentField?.Settings.IsHiddenFieldText))
                        return false;

                    return true;
                },
                id: 'txtFieldText',
                required: true,
            },
            FieldType: {
                id: 'drpFieldType',
                required: true,
            },
            Template: {
                id: 'txtFieldTemplate',
                required: true,
            },
            Theme: {
                id: 'txtFieldTheme',
                rule: (value) => {
                    if (!value && this.currentField?.FieldTypeObject.Themes?.length)
                        return false;

                    return true;
                },
                required: true,
            }
        },
            true,
            this.$scope,
            '$.currentField'
        );
    }

    _getFieldById(fieldId) {
        return this.fields.find(f => f.Id === fieldId);
    }

    _getFieldByName(fieldName) {
        return this.fields.find(f => f.FieldName === fieldName);
    }

    _getFieldIndex(fieldId) {
        return this.fields.findIndex(f => f.Id === fieldId);
    }

    _getFieldElementByFieldId(fieldId) {
        const $field = $(`div[b-field='${fieldId}']`);
        return $field?.length
            ? $field[0]
            : undefined;
    }

    _getFieldActions(fieldId) {
        this.apiService.get('Module', 'GetFieldActionsForModuleBuilder', { moduleId: this.module.Id, fieldId })
            .then((data) => {
                const field = this._getFieldById(fieldId);
                if (field) field.Actions = data;

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });
    }

    _getSuggestedFieldName(fieldType) {
        if (!fieldType) return '';

        const prefix = fieldType.FieldType;
        const usedNumbers = new Set(
            this.fields
                .map(f => {
                    const match = f.FieldName?.match(new RegExp(`^${prefix}(\\d+)$`));
                    return match ? Number(match[1]) : null;
                })
                .filter(n => n !== null)
        );

        let suggestedNumber = 1;
        while (usedNumbers.has(suggestedNumber)) {
            suggestedNumber++;
        }

        return `${prefix}${suggestedNumber}`;
    }

    _addField(paneName, parentId, fieldType, beforeFieldId) {
        const suggestFieldName = this._getSuggestedFieldName(fieldType);
        const source = fieldType.DefaultSettings || {};
        const defaults = {
            CssClass: 'b-field',
            FieldTextCssClass: 'b-field-label',
            InvalidCssClass: 'b-field-invalid',
            RequiredMessageCssClass: 'b-field-invalid-message',
            ValidationMessageCssClass: 'b-field-pattern-message',
            SubtextCssClass: 'b-field-subtext',
            IsDisabledLayout: false,
            IsHiddenFieldText: false
        };
        const defaultSettings = Object.fromEntries(
            Object.entries(defaults).map(([key, value]) => [key, source[key] ?? value])
        );
        Object.assign(defaultSettings, { ...source, ...defaultSettings });

        this.currentField = {
            ModuleId: this.module.Id,
            ParentId: parentId,
            PaneName: paneName,
            FieldType: fieldType.FieldType,
            FieldTypeObject: fieldType ?? {},
            FieldName: suggestFieldName,
            CanHaveValue: fieldType.CanHaveValue,
            CanHaveAction: fieldType.CanHaveAction,
            IsRequired: false,
            IsGroupField: fieldType.IsGroupField,
            HasDataSource: fieldType.HasDataSource,
            DataSource: {},
            Settings: defaultSettings,
            _beforeFieldId: beforeFieldId
        };

        this.workingMode = 'field-edit';
        this.$scope.$emit('onShowRightWidget', { controller: this });
        this.$timeout(() => this.$scope.$broadcast('onEditField'), 500);
        this.fieldOptions = window[`${this.currentField.FieldType}Options`];
    }

    _saveCurrentField(targetField, isNewField) {
        const defer = this.$q.defer();

        targetField = targetField ?? this.currentField;
        const field = GlobalHelper.clone(targetField);
        const fieldIds = [...document.querySelectorAll(`.pane-body[data-pane='${field.PaneName}'] [b-field]`)].map(el => el.getAttribute('b-field'));

        if (isNewField) {
            field.ViewOrder = !targetField._beforeFieldId
                ? fieldIds.length
                : fieldIds.indexOf(targetField._beforeFieldId);
        }

        const postData = !targetField._beforeFieldId
            ? { Field: field }
            : {
                Field: field,
                ReorderFields: true,
                PaneFieldIds: fieldIds,
                FieldViewOrder: field.ViewOrder
            };

        this.running = 'save-field';
        this.awaitAction = {
            title: 'Saving Field',
            subtitle: 'Just a moment for saving the field...'
        };

        this.apiService.post('Module', 'SaveModuleField', postData).then((id) => {
            if (isNewField)
                this.notifyService.success(`${field.FieldName} Field has been created successfully.`);
            else
                this.notifyService.info(`${field.FieldName} Field has been updated successfully`);

            field.Id = id;
            defer.resolve(field);

            this._disposeWorkingMode();

            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            defer.reject(error);

            this.running = null;
        });

        return defer.promise;
    }

    _saveFieldDataSource() {
        if (
            this.currentField.DataSource.Type === 0 &&
            (!this.currentField.DataSource.TextField || !this.currentField.DataSource.ValueField)) {
            this.currentField.DataSource.TextField = 'Text';
            this.currentField.DataSource.ValueField = 'Value';
        }

        this.fieldDataSourceBackup = null;

        this.$timeout(() => this._saveCurrentField());
        this._disposeWorkingMode();
    }

    _setFieldsDragingUi() {
        const elements = document.querySelectorAll('[data-drag="true"]');
        new Dragable(Array.from(elements).filter(el => !el.__dragable), {
            drop: '[data-drop]',
            animationDuration: 200,
            onOver: (evt) => {
                evt.to.classList.add('drag');
            },
            onOut: (evt) => {
                evt.to.classList.remove('drag');
            },
            onDrop: (evt) => {
                const paneName = evt.to.dataset.pane || evt.to.dataset.drop;
                const parentId = evt.to.dataset.parentId;
                const beforeField = evt.to.dataset.field
                    ? this._getFieldByName(evt.to.dataset.field)?.Id
                    : null;
                const fieldType = this.fieldTypes.find(ft => ft.FieldType === evt.from.dataset.fieldType);
                if (fieldType)
                    this.$scope.$apply(() => this._addField(paneName, parentId, fieldType, beforeField));

                evt.to.classList.remove('drag');
            },
        });
    }

    _setFieldsSortingUi() {
        const elements = document.querySelectorAll('.sortable-row:not([__sortable])');
        for (const element of elements) {
            new Sortable(element, {
                group: 'fields',
                targetAttr: '[b-field]',
                handle: '[data-handle="true"]',
                scrollSpeed: 500,         // Pixels per frame when auto-scrolling
                scrollZone: 60,          // Distance from viewport edge that triggers scroll (px)
                animationDuration: 200,  // FLIP animation duration (ms)
                apexY: 45,  //For the problems that Css(Transform) creates
                onEnd: (evt) => {
                    this.$scope.$apply(() => {
                        const paneName = evt.to.dataset.pane;
                        const parentId = evt.to.dataset.parentId || null;

                        this.currentField.PaneName = paneName;
                        this.currentField.ParentId = parentId;

                        this._sortPaneFields(paneName, parentId);
                    });
                }
            });

            element.__sortable = true;
        }
    }

    _sortPaneFields(paneName, parentId) {
        let serviceName = 'SortModuleFields';
        if (this.currentField.PaneName !== this.currentFieldBackup.PaneName)
            serviceName = 'UpdateModuleFieldPaneAndReorderFields';

        const fieldIds = [...document.querySelectorAll(`.pane-body[data-pane='${paneName}'] [b-field]`)]
            .map(el => el.getAttribute('b-field'));

        const postData = {
            ModuleId: this.module.Id,
            FieldId: this.currentField.Id,
            ParentId: parentId,
            PaneName: paneName,
            PaneFieldIds: fieldIds
        }

        this.running = 'sort-fields';
        this.awaitAction = {
            title: 'Saving Fields',
            subtitle: 'Just a moment for sorting the pane fields...',
        };

        this.apiService.post('Module', serviceName, postData).then(() => {
            this.notifyService.info('Fields has been sorted successfully.');

            fieldIds.forEach((fieldId, index) => {
                const field = this._getFieldById(fieldId);
                field.ViewOrder = index;
            });

            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    _scrollToFieldSection(fieldId) {
        this.$timeout(() => {
            const $field = this._getFieldElementByFieldId(fieldId);
            $([document.documentElement, document.body]).animate({
                scrollTop: $($field).offset().top - 100
            }, 500);

            $('ul.dropdown-menu.show').removeClass('show');
        }, 1000);
    }

    _removeCurrentField(changeTab) {
        this.currentField = null;
        this.currentFieldBackup = null;
        this.onSidebarTabClick('toolbox');
        return true;
    }

    _getDefinedLists() {
        const scenarioId = this.baseOptions.scenarioId;
        this.running = 'get-defined-lists';
        this.awaitAction = {
            title: 'Loading Defined Lists',
            subtitle: 'Just a moment for loading the defined lists...',
        };

        this.apiService.get('Studio', 'GetDefinedLists', { scenarioId }).then((data) => {
            this.definedLists = data;
            this.onDefinedListChange();

            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    _saveDefinedList() {
        const defer = this.$q.defer();

        this.definedList.ScenarioId = this.baseOptions.scenarioId;
        this.running = 'save-field-data-source';
        this.awaitAction = {
            title: 'Saving Field Data Source',
            subtitle: 'Just a moment for saving the field data source...',
        };

        this.apiService.post('Studio', 'SaveDefinedList', this.definedList).then((data) => {
            if (!this.definedList.Id)
                this.notifyService.success('Defined list has been created successfully.');
            else
                this.notifyService.info('Defined list has been updated successfully.');

            this.currentField.DataSource.Items = GlobalHelper.clone(this.definedList.Items);
            defer.resolve(data);

            this.definedList = null;
            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            defer.reject();

            this.running = null;
        });

        return defer.promise;
    }

    _getVariables() {
        this.apiService.get('Module', 'GetModuleVariablesForModuleBuilder', { moduleId: this.module.Id })
            .then((data) => {
                this._processVariables(data);
            });
    }

    _processVariables(variables) {
        let suggestions = {
            field: {},
            form: {},
            pane: []
        };
        let variablesAsFieldValueProperty = {};

        for (const variable of variables) {
            const baseObject = variable.Properties?.reduce((acc, prop) => {
                acc[prop.PropertyName] = {};
                return acc;
            }, {});

            if (variable.VariableType === 'AppModelList') {
                suggestions[variable.VariableName] = [baseObject];
            } else {
                suggestions[variable.VariableName] = baseObject;
                variablesAsFieldValueProperty[variable.VariableName] = baseObject;
            }
        }

        this.suggestions = suggestions;
        this.variablesAsFieldValueProperty = variablesAsFieldValueProperty;
        this.variablesAsDataSource = variables.filter(v =>
            v.Scope !== 'ServerSide' && (v.VariableType === 'AppModelList' || v.VariableType === 'object')
        );

        for (const field of this.fields) {
            this.suggestions.field[field.FieldName] = { Id: '', FieldName: '' }
        }
    }

    _registerWindowEvents() {
        /*-----------------------------------------------------------------------
            keydown detect
        -------------------------------------------------------------------*/
        document.addEventListener('keydown', (e) => {
            if (this.module.Id === UrlHelper.getParameterByName('id')) {
                //ctrl + s -- Save Current Field
                if (e.ctrlKey && (e.key === 's' || e.key === 'S') && this.currentField && this.currentFieldFocused) {
                    this._saveCurrentField();

                    e.preventDefault();
                }

                //Esc -- Cancel Current Field
                if (e.key === 'Escape' && this.currentField && this.currentFieldFocused) {
                    this.onCancelFieldClick(e);

                    e.preventDefault();
                }

                //ctrl + 5 -- Refresh Field
                if (e.ctrlKey && e.key === '5' && this.currentField && this.currentFieldFocused) {
                    this.onFieldReloadClick(e, this.currentField.Id);

                    e.preventDefault();
                }

                //Delete -- Delete Field
                if (e.ctrlKey && e.key === 'Delete' && this.currentField && this.currentFieldFocused) {
                    this.onDeleteFieldClick(e);

                    e.preventDefault();
                }

                //ctrl + q -- Show Field Actions
                if (e.ctrlKey && (e.key === 'q' || e.key === 'Q') && this.currentField && this.currentFieldFocused) {
                    this.onShowFieldActionsClick(e);

                    e.preventDefault();
                }

                //ctrl + 🠛🠅 -- Select Prev/Next Field
                if (e.ctrlKey && this.currentField && this.currentFieldFocused && (e.key === 'ArrowDown' || e.key === 'ArrowUp')) {
                    const $field = this._getFieldElementByFieldId(this.currentField.Id);
                    const $target = e.key === 'ArrowUp' ? $field.previousElementSibling : $field.nextElementSibling;
                    if ($target && $target.attributes['b-field']) {
                        const fieldId = $target.attributes['b-field'].value;
                        this.onFieldItemClick(e, fieldId);
                        if (!$scope.$$phase) $scope.$apply();

                        this._scrollToFieldSection(this.currentField.Id)
                    }

                    e.preventDefault();
                }

                //shift + 🠛🠅 -- Swap Current Field
                if (e.shiftKey && this.currentField && this.currentFieldFocused && (e.key === 'ArrowDown' || e.key === 'ArrowUp')) {
                    this.onFieldSwap(e, e.key.replace('ArrowDown', 'down').replace('ArrowUp', 'up'));
                    if (!$scope.$$phase) $scope.$apply();

                    e.preventDefault();
                }

                //f10 -- Build Module
                if (e.key === 'F10') {
                    this.onBuildModuleClick();

                    e.preventDefault();
                }

                //ctrl + f10 -- Build Module
                if (e.ctrlKey && e.key === 'F10') {
                    this.onBuildModuleClick(true);

                    e.preventDefault();
                }
            }
        });

        /*-----------------------------------------------------------------------
            window scroll detect
         -------------------------------------------------------------------*/
        document.addEventListener('scroll', (e) => {
            if (this.module.Id === UrlHelper.getParameterByName('id')) {

                const top = $(window).scrollTop();
                if (top > 150)
                    $(`#buildWrapper${this.module.Id}`).addClass('b-sticky');
                else
                    $(`#buildWrapper${this.module.Id}`).removeClass('b-sticky');
            }
        });
    }

    _disposeWorkingMode() {
        this.$scope.$emit('onHideRightWidget');
        this.$timeout(() => this.workingMode = null, 200);
    }

    //#endregion
}