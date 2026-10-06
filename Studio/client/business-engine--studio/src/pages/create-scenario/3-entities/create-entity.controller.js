import Swal from 'sweetalert2';
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';
import { GlobalUtil } from '../../../shared/utils';

export class CreateEntityController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        validationService,
        notificationService,
        baseOptions
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        this.$scope.$watch('$.entity.EntityName', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.entity.Settings.DatabaseObjectNameModified)
                this.entity.Settings.DatabaseObjectPostfixName = this._getWordPluralize(newVal);
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

        this.running = 'get-entity';
        this.awaitAction = {
            title: 'Loading Entity',
            subtitle: 'Just a moment for loading entity...',
        };

        this.apiService.get('Studio', 'GetEntity', { entityId: id }).then((data) => {
            this.entity = data;
            if (!this.entity) {
                this.entity = {
                    ScenarioId: this.baseOptions.scenarioId,
                    EntityType: 0,
                    Settings: {
                        DatabaseObjectPrefixName: this.$rootScope.scenario.DatabaseObjectPrefix
                    },
                    Columns: [{
                        ColumnName: 'Id',
                        ColumnType: 'int',
                        IsPrimary: true,
                        IsIdentity: true,
                        ViewOrder: 0,
                    }]
                };
            }
            else {
                if (this.entity.IsReadonly) this._getDatabaseObjects();

                this.entity.Settings = this.entity.Settings ?? {};

                this.$scope.$emit('onUpdateCurrentTab', {
                    id: this.entity.Id,
                    title: this.entity.EntityName,
                });
            }

            this._setForm();
            this.onFocusModule();

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
        this.$rootScope.explorerExpandedItems.push(...['entities', 'create-entity']);
        this.$rootScope.explorerCurrentItem = !this.entity || !this.entity.Id
            ? 'create-entity'
            : this.entity.Id;
    }

    onSetValidNameClick() {
        this.entity.EntityName = GlobalHelper.normalizeName(this.entity.EntityName);
    }

    onEntityIsReadOnlyChange() {
        if (!this.dataBaseObjects) {
            this._getDatabaseObjects();
        }
    }

    onDatabaseObjectChange() {
        this.running = 'get-database-object-columns';
        this.awaitAction = {
            title: 'Loading Database Object Columns',
            subtitle: 'Just a moment for loading database object columns...',
        };

        this.apiService.get('Studio', 'GetDatabaseObjectColumns', {
            objectName: this.entity.TableName
        }).then((data) => {
            this.entity.Columns = data;

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

    onAddColumnClick() {
        this.entity.Columns = this.entity.Columns || [];

        //Calculating a new ViewOrder based on the existing latest order.
        const newViewOrder = this._getNextViewOrder();

        const column = {
            IsEdited: true,
            IsNew: true,
            AllowNulls: true,
            ViewOrder: newViewOrder,
        };
        this.entity.Columns.push(column);

        this._focusColumnForm();
    }

    onColumnSwapClick(index, swappedIndex) {
        const columns = this.entity.Columns;
        if (swappedIndex > -1 && swappedIndex < columns.length) {
            [columns[index], columns[swappedIndex]] = [
                columns[swappedIndex],
                columns[index],
            ];

            columns.map((c) => (c.ViewOrder = this.entity.Columns.indexOf(c) + 1));
        }
    }

    onSaveEntityClick() {
        this.form.validated = true;
        this.form.validator(this.entity);

        this.validatedColumns = [];
        let isValid = true;

        this.entity.Columns.forEach((column, index) => {
            this.validatedColumns.push({});
            this.columnForm.validated = true;
            this.columnForm.validator(column);

            if (!this.columnForm.valid) {
                this.validatedColumns[index] = { error: this.columnForm.error };
                isValid = false;
            }
            else if (column.IsPrimary && column.ColumnType !== 'tinyint' && column.ColumnType !== 'smallint' &&
                column.ColumnType !== 'int' && column.ColumnType !== 'bigint')
                column.IsIdentity = false;
        });

        if (this.form.valid && isValid) {
            this.running = 'save-entity';
            this.awaitAction = {
                title: 'Creating Entity',
                subtitle: 'Just a moment for creating entity...',
            };

            this.entity.Columns.sort((a, b) => a.ViewOrder - b.ViewOrder)
                .forEach((column, index) => column.ViewOrder = index);

            this.currentTabKey = this.$rootScope.currentTab?.key;

            this.apiService.post('Studio', 'SaveEntity', this.entity).then((data) => {
                if (!this.entity.Id)
                    this.notifyService.success('Entity has been created successfully.');
                else
                    this.notifyService.info('Entity has been updated successfully.');

                this.entity = data;

                this.$scope.$emit('onUpdateCurrentTab', {
                    id: this.entity.Id,
                    title: this.entity.EntityName,
                    key: this.currentTabKey,
                });

                this.$rootScope.refreshSidebarExplorerItems();

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });
        }
    }

    onDeleteEntityClick() {
        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary entity!</p><b></b>',
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
                this.running = 'delete-entity';
                this.awaitAction = {
                    title: 'Delete Entity',
                    subtitle: 'Just a moment for deleting entity...',
                };

                this.apiService.post('Studio', 'DeleteEntity', { Id: this.entity.Id }).then((data) => {
                    if (data) {
                        this.notifyService.info('Entity has been deleted successfully.');
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
            ScenarioId: {
                required: true,
            },
            EntityName: {
                id: 'txtEntityName' + (this.entity.Id ?? ''),
                rule: (value) => {
                    if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) === false)
                        return 'Entity name is not valid';

                    return true;
                },
                required: true,
            },
            EntityType: {
                required: true,
            },
            TableName: {
                id: 'drpEntityTable' + (this.entity.Id ?? ''),
                rule: (value) => {
                    if (this.entity.IsReadonly &&
                        (this.entity.EntityType === 0 && !this.dataBaseObjects.Tables.some(v => v === value)) ||
                        (this.entity.EntityType === 1 && !this.dataBaseObjects.Views.some(v => v === value))
                    )
                        return false;

                    return true;
                },
                required: true,
            },
            'Settings.DatabaseObjectPrefixName': {
                id: 'txtTablePrefix' + (this.entity.Id ?? ''),
                required: false,
            },
            'Settings.DatabaseObjectPostfixName': {
                id: 'txtTablePostfix' + (this.entity.Id ?? ''),
                required: true,
            },
            Columns: {
                rule: ({ length }) => {
                    if (length >= 1) return true;

                    return 'Entity must have column(s)';
                },
                required: true,
            },
        },
            true,
            this.$scope,
            '$.entity'
        );

        this.columnForm = this.validationService.init({
            ColumnName: {
                required: true,
            },
            ColumnType: {
                rule: (value, column) => {
                    if (!GlobalHelper.checkSqlTypes(value))
                        return 'The column type is not valid';

                    return true;
                },
                required: true,
            },
            AllowNulls: {
                rule: (value, column) => {
                    if (value && column.IsPrimary)
                        return 'The primary column can not be nullable';

                    return true;
                },
            }
        });
    }

    _focusColumnForm() {
        this.$timeout(() => {
            this.$scope.$broadcast('onEditColumn');
        });
    }

    _getWordPluralize(word) {
        return GlobalHelper.pluralize(word);
    }

    _getDatabaseObjects() {
        this.running = 'get-database-objects';
        this.awaitAction = {
            title: 'Loading Database Objects',
            subtitle: 'Just a moment for loading database objects...',
        };

        this.apiService.get('Studio', 'GetDatabaseObjects',).then((data) => {
            this.dataBaseObjects = { Tables: data.Tables, Views: data.Views };

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

    _getNextViewOrder() {
        if (!this.entity.Columns || this.entity.Columns.length === 0) {
            return 0;
        }
        const maxOrder = this.entity.Columns.reduce((max, col) => {
            const order = col.ViewOrder ?? -1;
            return order > max ? order : max;
        }, -1);
        return maxOrder + 1;
    }

    //#endregion
}