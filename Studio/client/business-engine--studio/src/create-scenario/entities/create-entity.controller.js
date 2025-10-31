import pluralize from "pluralize";
import { GlobalSettings } from "../../angular/angular-configs/global.settings";

export class CreateEntityController {
    constructor(
        $scope,
        $rootScope,
        studioService,
        $timeout,
        globalService,
        apiService,
        validationService,
        notificationService,
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.studioService = studioService;

        this.groups = _.filter(this.$rootScope.groups, (g) => { return g.ObjectType == 'Entity' });

        this.$scope.$watch("$.entity.EntityName", (newVal, oldVal) => {
            if (newVal != oldVal && !this.entity.Settings.DatabaseObjectNameModified)
                this.entity.Settings.DatabaseObjectPostfixName =
                    this.getWordPluralize(newVal);
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");

        this.running = "get-entity";
        this.awaitAction = {
            title: "Loading Entity",
            subtitle: "Just a moment for loading entity...",
        };

        this.apiService.get("Studio", "GetEntity", { entityId: id || null }).then((data) => {
            this.entity = data;
            if (!this.entity) {
                this.entity = {
                    ScenarioId: GlobalSettings.scenarioId,
                    EntityType: 0,
                    Settings: {
                        DatabaseObjectPrefixName: this.$rootScope.scenario.DatabaseObjectPrefix
                    },
                    Columns: [{
                        ColumnName: "Id",
                        ColumnType: "int",
                        IsPrimary: true,
                        IsIdentity: true,
                        ViewOrder: 0,
                    }]
                };
            } else {
                if (this.entity.IsReadonly) this.getDatabaseObjects();

                this.entity.Settings = this.entity.Settings ?? {};

                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.entity.Id,
                    title: this.entity.EntityName,
                });
            }

            this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        }
        );

        this.setForm();
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(
            ...["entities", "create-entity"]
        );
        this.$rootScope.explorerCurrentItem = !this.entity || !this.entity.Id ?
            "create-entity" :
            this.entity.Id;
    }

    setForm() {
        this.form = this.validationService.init({
            ScenarioId: {
                rule: "guid",
                id: "drpScenarioId",
                required: true,
            },
            EntityName: {
                id: "txtEntityName",
                rule: (value) => {
                    if (/^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Entity name is not valid";
                    else
                        return true;
                },
                required: true,
            },
            EntityType: {
                id: "rdEntityType",
                required: true,
            },
            "Settings.DatabaseObjectPrefixName": {
                id: "txtTablePrefix",
                required: false,
            },
            "Settings.DatabaseObjectPostfixName": {
                id: "txtTablePostfix",
                required: true,
            },
            Columns: {
                rule: ({ length }) => {
                    if (length >= 1) return true;

                    return "Entity must have column(s)";
                },
                required: true,
            },
        },
            true,
            this.$scope,
            "$.entity"
        );

        this.columnForm = this.validationService.init({
            ColumnName: {
                id: "txtColumnName",
                required: true,
            },
            ColumnType: {
                rule: (value) => {
                    if (!this.globalService.checkSqlTypes(value))
                        return "The column type is not valid";

                    return true;
                },
                id: "txtColumnType",
                required: true,
            },
            AllowNulls: {
                rule: (value, column) => {
                    if (value && column.IsPrimary)
                        return "The primary key column value can not be null";

                    return true;
                },
            },
            IsIdentity: {
                rule: (value, column) => {
                    if (
                        (value && column.ColumnType != "int") ||
                        _.filter(this.entity.Columns, (c) => c.IsIdentity).length > 1
                    )
                        return "The identity column is not valid";

                    return true;
                },
            },
            IsComputedColumn: {
                rule: (value, column) => {
                    if (value && !column.Formula)
                        return "The column formula is not valid";

                    return true;
                },
            },
        });
    }

    onSetValidNameClick() {
        this.entity.EntityName = this.globalService.normalizeName(this.entity.EntityName);
    }

    onEntityIsReadOnlyChange() {
        if (!this.dataBaseObjects) {
            this.getDatabaseObjects();
        }
    }

    getDatabaseObjects() {
        this.running = "get-database-objects";
        this.awaitAction = {
            title: "Loading Database Objects",
            subtitle: "Just a moment for loading database objects...",
        };

        this.apiService
            .get("Studio", "GetDatabaseObjects",).then((data) => {
                this.dataBaseObjects = { Tables: data.Tables, Views: data.Views };

                delete this.running;
                delete this.awaitAction;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
    }

    onDatabaseObjectChange() {
        this.running = "get-database-object-columns";
        this.awaitAction = {
            title: "Loading Database Object Columns",
            subtitle: "Just a moment for loading database object columns...",
        };

        this.apiService.get("Studio", "GetDatabaseObjectColumns", {
            objectName: this.entity.TableName
        }).then((data) => {
            this.entity.Columns = data;

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    onSyncColumns() {
        this.running = "get-database-object-columns";
        this.awaitAction = {
            title: "Loading Database Object Columns",
            subtitle: "Just a moment for loading database object columns...",
        };

        this.apiService.get("Studio", "GetDatabaseObjectColumns", {
            objectName: this.entity.TableName,
        }).then((columns) => {
            var beAdded = _.differenceBy(
                columns,
                this.entity.Columns,
                "ColumnName"
            );
            var beRemoved = _.differenceBy(
                this.entity.Columns,
                columns,
                "ColumnName"
            );

            this.entity.Columns.push(...beAdded);

            _.remove(this.entity.Columns, (c) => {
                return _.filter(beRemoved, (i) => {
                    return i.ColumnName == c.ColumnName;
                }).length;
            });

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    onRefreshColumns() {
        this.onDatabaseObjectChange();
    }

    /*------------------------------------
           Entity Column
          ------------------------------------*/
    onAddColumnClick() {
        if (this.column) return;

        this.entity.Columns = this.entity.Columns || [];

        const columnIndex = this.entity.Columns.length;
        const column = {
            IsEdited: true,
            IsNew: true,
            AllowNulls: true,
            ViewOrder: columnIndex,
        };

        this.entity.Columns.push(column);

        this.column = _.clone(column);

        this.columnIndex = columnIndex;

        this.focusColumnForm();
    }

    onRowItemClick(column, index) {
        if (this.column || this.entity.IsReadonly) return;

        column.IsEdited = true;

        this.column = _.clone(column);
        this.column.IsNew = false;

        this.columnIndex = index;

        this.focusColumnForm();
    }

    focusColumnForm() {
        this.$timeout(() => {
            this.$scope.$broadcast("onEditColumn");
        });
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

    onSaveColumnClick() {
        this.columnForm.validated = true;
        this.columnForm.validator(this.column);

        if (this.columnForm.valid) {
            this.column.IsEdited = false;
            this.entity.Columns[this.columnIndex] = _.clone(this.column);

            delete this.column;
            delete this.columnIndex;
        }
    }

    onCancelColumnClick() {
        if (this.column.IsNew) this.entity.Columns.splice(this.columnIndex, 1);
        else this.entity.Columns[this.columnIndex].IsEdited = false;

        delete this.column;
        delete this.columnIndex;
    }

    onColumnPropertiesClick(column, isEditMode, index) {
        this.columnForm.validated = true;
        this.columnForm.validator(column);

        if (this.columnForm.valid) {
            if (isEditMode) this.backupColumn = _.clone(column);
            else {
                this.column = _.clone(column);
                this.columnIndex = index;
            }

            window["wnColumnProperties"].show();
        }
    }

    onColumnPrimaryKeyChange() {
        var isPrimary = this.column.IsPrimary;

        _.filter(this.entity.Columns, (c) => {
            return c.IsPrimary;
        }).map((c) => {
            c.IsPrimary = false;
        });

        this.column.IsPrimary = isPrimary;
    }

    onSaveColumnPropertiesClick() {
        this.columnForm.validated = true;
        this.columnForm.validator(this.column);

        if (this.columnForm.valid) {
            if (!this.backupColumn) {
                this.entity.Columns[this.columnIndex] = _.clone(this.column);

                delete this.column;
                delete this.columnIndex;
            }

            delete this.backupColumn;

            window["wnColumnProperties"].hide();
        }
    }

    onCancelColumnPropertiesClick() {
        if (this.backupColumn) this.column = _.clone(this.backupColumn);
        else {
            delete this.column;
            delete this.columnIndex;
        }

        delete this.backupColumn;

        window["wnColumnProperties"].hide();
    }

    onSaveEntityClick() {
        this.form.validated = true;
        this.form.validator(this.entity);

        if (this.form.valid) {
            this.running = "save-entity";
            this.awaitAction = {
                title: "Creating Entity",
                subtitle: "Just a moment for creating entity...",
            };

            this.currentTabKey = this.$rootScope.currentTab.key;

            this.apiService.post("Studio", "SaveEntity", this.entity).then((data) => {
                this.entity = data;

                this.notifyService.success("Entity updated has been successfully");

                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.entity.Id,
                    title: this.entity.EntityName,
                    key: this.currentTabKey,
                });

                this.$rootScope.refreshSidebarExplorerItems();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                if (error.data.HResult == "-2146232060")
                    this.notifyService.error(
                        `Entity name must be unique.${this.entity.EntityName} is already in the scenario entities`
                    );
                else if (error.data.HResult == "-2146233088")
                    this.notifyService.error(
                        `Table name must be unique.${this.entity.TableName} is already in the database`
                    );
                else this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    getWordPluralize(word) {
        return pluralize(word);
    }

    onDeleteEntityClick() {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary entity!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "get-entities";
                this.awaitAction = {
                    title: "Remove Entity",
                    subtitle: "Just a moment for removing entity...",
                };

                this.apiService.post("Studio", "DeleteEntity", { Id: this.entity.Id }).then(
                    (data) => {
                        this.notifyService.success("Entity deleted has been successfully");

                        this.onCloseWindow();

                        this.$rootScope.refreshSidebarExplorerItems();

                        delete this.awaitAction;
                        delete this.running;
                    },
                    (error) => {
                        this.awaitAction.isError = true;
                        this.awaitAction.subtitle = error.statusText;
                        this.awaitAction.desc =
                            this.globalService.getErrorHtmlFormat(error);

                        this.notifyService.error(error.data.Message);

                        delete this.running;
                    }
                );
            }
        });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}