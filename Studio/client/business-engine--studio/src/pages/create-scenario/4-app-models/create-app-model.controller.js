import Swal from 'sweetalert2';
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';
import { GlobalUtil } from '../../../shared/utils';

export class CreateAppModelController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        validationService,
        notificationService,
        sqlConstants,
        baseOptions
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.sqlConstants = sqlConstants;
        this.baseOptions = baseOptions;
        this._modelTypes = ['ViewModel', 'ListItem', 'Dto', 'Info']

        const currentTabKey = this.$rootScope.currentTab.key;
        $scope.$on(`onTab--${currentTabKey}--Selected`, (e, args) => {
            this.onFocusModule();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');
        const scenarioId = this.baseOptions.scenarioId;

        this.running = 'get-appModel';
        this.awaitAction = {
            title: 'Loading AppModel',
            subtitle: 'Just a moment for loading view model...',
        };

        this.apiService.get('Studio', 'GetAppModel', { scenarioId, appModelId: id }).then((data) => {
            this.entities = data.Entities;
            this.appModel = data.AppModel;
            this.propertyTypes = data.PropertyTypes;

            if (!this.appModel) {
                this.appModel = {
                    ScenarioId: this.baseOptions.scenarioId,
                    Properties: [],
                    Settings: {
                        SelectedEntities: [],
                    }
                };
            } else {
                if (!this.appModel.Settings.BaseName) {
                    const words = this._splitNameByKeywords(this.appModel.ModelName, this._modelTypes);
                    this.appModel.Settings.BaseName = words[0];
                    if (words.length > 1) this.appModel.Settings.Postfix = words[1];
                }

                this.$scope.$emit('onUpdateCurrentTab', {
                    id: this.appModel.Id,
                    title: this.appModel.ModelName,
                });
            }

            this.selectedEntities = [];
            this.appModel.Settings.SelectedEntities = this.appModel.Settings.SelectedEntities ?? [];

            const selectedEntityIds = new Set(this.appModel.Settings.SelectedEntities);
            const propertyMap = new Map(
                this.appModel.Properties.map(p => [p.Settings.ColumnId, p])
            );

            for (const entity of this.entities) {
                entity.isSelected = selectedEntityIds.has(entity.Id);

                if (entity.isSelected) {
                    const clonedEntity = structuredClone(entity);
                    this.selectedEntities.push(clonedEntity);

                    for (const column of clonedEntity.Columns) {
                        column.isSelected = propertyMap.has(column.Id);
                    }
                }
            }

            this._setForm();
            this.onFocusModule();

            this.$timeout(() => this._setSelectedEntitiesWidgetHeight());

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

        this.$rootScope.explorerExpandedItems.push(...['app-models', 'create-app-model']);
        this.$rootScope.explorerCurrentItem = !this.appModel || !this.appModel.Id ?
            'create-app-model' :
            this.appModel.Id;
    }

    onSetValidNameClick() {
        this.appModel.Settings.BaseName = GlobalHelper.normalizeName(this.appModel.Settings.BaseName);
        if (this.appModel.Settings.Postfix)
            this.appModel.Settings.Postfix = GlobalHelper.normalizeName(this.appModel.Settings.Postfix);

        this.appModel.ModelName = this.appModel.Settings.BaseName + this.appModel.Settings.Postfix;
    }

    onToggleSelectedEntityClick(entity) {
        entity.isSelected = !entity.isSelected;

        if (!entity.isSelected) {
            const selectedEntity = this.selectedEntities.find(e => e.Id === entity.Id);
            for (const column of selectedEntity.Columns) {
                if (column.isSelected) {
                    column.isSelected = false;
                    this.onToggleEntityColumnChange(column);
                }
            }
        }

        const isExists = this.appModel.Settings.SelectedEntities.includes(entity.Id);
        this.appModel.Settings.SelectedEntities = isExists
            ? this.appModel.Settings.SelectedEntities = this.appModel.Settings.SelectedEntities.filter(id => id !== entity.Id)
            : this.appModel.Settings.SelectedEntities.concat(entity.Id);

        this._buildSelectedEntities();
    }

    onToggleEntityColumnChange(column) {
        if (!column.isSelected)
            this.appModel.Properties =
                this.appModel.Properties.filter(p => p.Settings?.ColumnId !== column.Id);
        else {
            const sqlType = column.ColumnType;

            const normalized = sqlType.trim().toLowerCase().replace(/\(.*\)/, '');
            const match = this.sqlConstants.sqlTypeMap.find(t => t.sql.test(normalized));
            const maxOrder = this.appModel.Properties.length
                ? Math.max(...this.appModel.Properties.map(p => p.ViewOrder || 0)) + 1
                : 0;
            const prop = {
                PropertyName: column.ColumnName,
                PropertyType: match ? match.cs : 'object',
                ViewOrder: maxOrder,
                Settings: {
                    ColumnId: column.Id
                }
            }

            this.appModel.Properties.push(prop);
        }

        this._setSelectedEntitiesWidgetHeight();
    }

    onModelBaseNameChange() {
        this.appModel.ModelName = this.appModel.Settings.BaseName + this.appModel.Settings.Postfix;
    }

    onChangeModelTypeClick(modelType) {
        this.appModel.ModelType = modelType;

        if (!this.appModel.Settings.PostfixModified) {
            this.appModel.ModelName = this.appModel.Settings.BaseName + this.appModel.Settings.Postfix;
            this.appModel.Settings.Postfix = this._getModuleTypeText(modelType);
        }
    }

    onAddPropertyClick() {
        const maxOrder = this.appModel.Properties.length
            ? Math.max(...this.appModel.Properties.map(p => p.ViewOrder || 0))
            : 1;
        const property = { ViewOrder: maxOrder + 1 };

        this.appModel.Properties.push(property);
        this._reorderProperties();

        this.$timeout(() => {
            this.$scope.$broadcast('onEditProperty' + this.appModel.Properties.length);
            this._setSelectedEntitiesWidgetHeight()
        });
    }

    onRemovePropertyClick(prop, index) {
        if (prop.Settings?.ColumnId) {
            this.selectedEntities.forEach(entity => {
                const column = entity.Columns.find(c => c.Id === prop.Settings.ColumnId);
                if (column) column.isSelected = false;
            });
        }

        this.appModel.Properties.splice(index, 1);
        this._reorderProperties();
    }

    onPropertySwapClick(index, swappedIndex) {
        const props = this.appModel.Properties;

        if (swappedIndex < 0 || swappedIndex >= props.length) return;

        // swap
        const temp = props[index];
        props[index] = props[swappedIndex];
        props[swappedIndex] = temp;

        // Only update the two items that changed
        props[index].ViewOrder = index;
        props[swappedIndex].ViewOrder = swappedIndex;

        this._reorderProperties();
    }

    onSaveAppModelClick() {
        this.form.validated = true;
        this.form.validator(this.appModel);

        if (this.form.valid) {
            this.appModel.ModelName = this.appModel.Settings.BaseName + this.appModel.Settings.Postfix;

            this.running = 'save-appModel';
            this.awaitAction = {
                title: 'Creating AppModel',
                subtitle: 'Just a moment for creating appModel...',
            };

            this.currentTabKey = this.$rootScope.currentTab?.key;

            this.apiService.post('Studio', 'SaveAppModel', this.appModel).then((data) => {
                if (!this.appModel.Id)
                    this.notifyService.success('AppModel has been created successfully.');
                else
                    this.notifyService.info('AppModel has been updated successfully.');

                this.appModel = data;

                this.$scope.$emit('onUpdateCurrentTab', {
                    id: this.appModel.Id,
                    title: this.appModel.ModelName,
                    key: this.currentTabKey,
                });

                this.$rootScope.refreshSidebarExplorerItems();

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
    }

    onDeleteAppModelClick() {
        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary model!</p><b></b>',
            icon: 'warning',
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
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
                this.running = 'delete-appModel';
                this.awaitAction = {
                    title: 'Delete AppModel',
                    subtitle: 'Just a moment for deleting App Model...',
                };

                this.apiService.post('Studio', 'DeleteAppModel', { Id: this.appModel.Id }).then((data) => {
                    if (data) {
                        this.notifyService.info('AppModel has been deleted successfully.');
                        this.$rootScope.refreshSidebarExplorerItems();
                        this.onCloseWindow();
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
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            ModelName: {
                id: `appModelBaseName_${this.appModel.Id ?? ''}`,
                rule: (value) => {
                    if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) == false)
                        return 'Model name is not valid';

                    if (value)
                        return this._checkConsecutiveKeywordsWithMessage(value, this._modelTypes);

                    return true;
                },
                required: true,
            },
            Properties: {
                rule: (value) => {
                    if (!value?.length)
                        return 'Model must have properties';

                    for (const prop of value) {
                        if (!prop.PropertyName || !prop.PropertyType)
                            return 'Model properties are not valid'
                    }

                    return true;
                },
                required: true,
            },
            'Settings.BaseName': {
                id: `appModelBaseName${this.appModel.Id}`,
                rule: (value) => {
                    if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) == false)
                        return 'Model base name is not valid';
                    else
                        return true;
                },
                required: true,
            }
        },
            true,
            this.$scope,
            '$.appModel'
        );

        this.propertyForm = this.validationService.init({
            PropertyName: {
                id: 'txtPropertyName',
                required: true,
            },
            PropertyType: {
                id: 'drpPropertyType',
                required: true,
            },
            PropertyTypeId: {
                rule: (value) => {
                    if (
                        (this.property.PropertyType == 'appModel' ||
                            this.property.PropertyType == 'listOfAppModel') &&
                        !value
                    ) {
                        return 'Select a view model for property type';
                    } else return true;
                },
                id: 'drpPropertyTypeId',
            },
        });
    }

    _buildSelectedEntities() {
        const selectedIds = this.appModel.Settings.SelectedEntities ?? [];
        this.selectedEntities = selectedIds.reduce((result, id) => {
            const entity = this.entities.find(e => e.Id === id);
            if (entity) {
                entity.isSelected = true;

                const clone = GlobalHelper.clone(entity);
                for (const column of clone.Columns ?? []) {
                    const prop = this.appModel.Properties?.find(p => p.Settings?.ColumnId === column.Id);
                    if (!!prop)
                        column.isSelected = true;
                    else
                        column.isSelected = false
                }

                result.push(clone);
            }
            return result;
        }, []);

        this._setSelectedEntitiesWidgetHeight();
    }

    _getModuleTypeText(modelType) {
        switch (modelType) {
            case 0:
                return '';
            case 1:
                return 'ViewModel';
            case 2:
                return 'ListItem';
            case 3:
                return 'Info';
        }
    }

    _checkConsecutiveKeywordsWithMessage(str, keywords) {
        if (!str || !keywords || keywords.length === 0) return { warning: false };

        const normalizedStr = str.trim();
        const pattern = keywords.join('|');
        const regex = new RegExp(`(${pattern}){2,}$`, 'i');

        if (!regex.test(normalizedStr)) {
            return !false;
        }

        let tempStr = normalizedStr;
        const matchedWords = [];

        while (true) {
            let matched = false;
            for (const kw of keywords) {
                const kwRegex = new RegExp(kw + '$', 'i');
                if (kwRegex.test(tempStr)) {
                    matchedWords.unshift(kw);
                    tempStr = tempStr.slice(0, -kw.length);
                    matched = true;
                    break;
                }
            }
            if (!matched) break;
        }

        if (matchedWords.length <= 1) {
            return !false;
        }

        const extraWords = matchedWords.slice(1);

        return `The word${extraWords.length > 1 ? 's' : ''} ${extraWords.join(', ')} ${extraWords.length > 1 ? 'are' : 'is'} added, please remove ${extraWords.length > 1 ? 'them' : 'it'}.`
    }

    _splitNameByKeywords(str, keywords) {
        if (!str || !keywords || keywords.length === 0)
            return [str];

        const normalizedStr = str.trim();
        const lowerStr = normalizedStr.toLowerCase();

        for (const kw of keywords) {
            const kwLower = kw.toLowerCase();

            if (lowerStr.endsWith(kwLower)) {
                const base = normalizedStr.slice(0, normalizedStr.length - kw.length);
                return [base, kw];
            }
        }

        return [normalizedStr];
    }

    _setSelectedEntitiesWidgetHeight() {
        const $left = $(`#selectedEntitiesLeftWidget_${this.appModel.Id ?? ''}`);
        const $right = $(`#selectedEntitiesRightWidget_${this.appModel.Id ?? ''}>.app-model-card`);

        setTimeout(() => {
            const height = $right.outerHeight();
            $left.height(height);
        }, 3000);
    }

    _reorderProperties() {
        this.appModel.Properties.forEach((p, index) => {
            p.ViewOrder = index;
        })
    }

    //#endregion
}