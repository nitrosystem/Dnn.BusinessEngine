import { GlobalSettings } from "../../angular/angular-configs/global.settings";

export class CreateAppModelController {
    constructor(
        $scope,
        $rootScope,
        studioService,
        $timeout,
        globalService,
        apiService,
        validationService,
        notificationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        // نقشه‌ی تبدیل SQL Server → C#
        this.typeMap = [
            { sql: /^bigint$/i, cs: "long?" },
            { sql: /^binary|varbinary/i, cs: "byte[]" },
            { sql: /^bit$/i, cs: "bool?" },
            { sql: /^char|nchar|varchar|nvarchar|text|ntext/i, cs: "string" },
            { sql: /^date$/i, cs: "DateTime?" },
            { sql: /^datetime$/i, cs: "DateTime?" },
            { sql: /^datetime2$/i, cs: "DateTime?" },
            { sql: /^datetimeoffset$/i, cs: "DateTimeOffset" },
            { sql: /^decimal|numeric/i, cs: "decimal?" },
            { sql: /^float$/i, cs: "double?" },
            { sql: /^int$/i, cs: "int?" },
            { sql: /^money|smallmoney$/i, cs: "decimal?" },
            { sql: /^real$/i, cs: "float?" },
            { sql: /^smallint$/i, cs: "short?" },
            { sql: /^small datetime$/i, cs: "DateTime?" },
            { sql: /^time$/i, cs: "TimeSpan" },
            { sql: /^tinyint$/i, cs: "byte?" },
            { sql: /^uniqueidentifier$/i, cs: "Guid?" },
            { sql: /^xml$/i, cs: "string" },
            { sql: /^image$/i, cs: "byte[]" },
            { sql: /^sql_variant$/i, cs: "object" }
        ];
        this.modelTypes = ['ViewModel', 'ListItem', 'Dto', 'Info']

        this.groups = _.filter(this.$rootScope.groups, (g) => { return g.GroupType == 'AppModel' });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");

        this.running = "get-appModel";
        this.awaitAction = {
            title: "Loading AppModel",
            subtitle: "Just a moment for loading view model...",
        };

        this.apiService.get("Studio", "GetAppModel", { appModelId: id || null }).then((data) => {
            this.propertyTypes = data.PropertyTypes;
            this.entities = data.Entities;
            this.appModels = data.AppModels;
            this.appModel = data.AppModel;
            if (!this.appModel) {
                this.appModel = {
                    ScenarioId: GlobalSettings.scenarioId,
                    Properties: [],
                    Settings: {
                        SelectedEntities: [],
                    }
                };
            } else {
                if (!this.appModel.Settings.BaseName) {
                    const words = this.splitNameByKeywords(this.appModel.ModelName, this.modelTypes);
                    this.appModel.Settings.BaseName = words[0];
                    if (words.length > 1) this.appModel.Settings.Postfix = words[1];
                }

                this.$scope.$emit("onUpdateCurrentTab", {
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

            this.setForm();
            this.onFocusModule();

            this.$timeout(() => this.setSelectedEntitiesWidgetHeight());

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

    onFocusModule() {
        this.$scope.$emit("onChangeActivityBar", {
            name: "builder",
            title: "Module Builder",
        });

        this.$rootScope.explorerExpandedItems.push(...["app-models", "create-app-model"]);
        this.$rootScope.explorerCurrentItem = !this.appModel || !this.appModel.Id ?
            "create-app-model" :
            this.appModel.Id;
    }

    setForm() {
        this.form = this.validationService.init({
            ModelName: {
                id: `appModelBaseName_${this.appModel.Id ?? ''}`,
                rule: (value) => {
                    if (/^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Model name is not valid";

                    if (value)
                        return this.checkConsecutiveKeywordsWithMessage(value, this.modelTypes);

                    return true;
                },
                required: true,
            },
            Properties: {
                rule: (value) => {
                    if (!value || !value.length)
                        return "Model must have properties";

                    for (const prop of value) {
                        if (!prop.PropertyName || !prop.PropertyType)
                            return "Model properties are not valid"
                    }

                    return true;
                },
                required: true,
            },
            "Settings.BaseName": {
                id: `appModelBaseName${this.appModel.Id}`,
                rule: (value) => {
                    if (/^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Model base name is not valid";
                    else
                        return true;
                },
                required: true,
            }
        },
            true,
            this.$scope,
            "$.appModel"
        );

        this.propertyForm = this.validationService.init({
            PropertyName: {
                id: "txtPropertyName",
                required: true,
            },
            PropertyType: {
                id: "drpPropertyType",
                required: true,
            },
            PropertyTypeId: {
                rule: (value) => {
                    if (
                        (this.property.PropertyType == "appModel" ||
                            this.property.PropertyType == "listOfAppModel") &&
                        !value
                    ) {
                        return "Select a view model for property type";
                    } else return true;
                },
                id: "drpPropertyTypeId",
            },
        });
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

        this.buildSelectedEntities();
    }

    buildSelectedEntities() {
        const selectedIds = this.appModel.Settings.SelectedEntities ?? [];

        this.selectedEntities = selectedIds.reduce((result, id) => {
            const entity = this.entities.find(e => e.Id === id);
            if (entity) {
                entity.isSelected = true;
                const clone = angular.copy(entity);
                clone.Columns?.forEach(c => c.isSelected = false);
                result.push(clone);
            }
            return result;
        }, []);

        this.setSelectedEntitiesWidgetHeight();
    }

    onToggleEntityColumnChange(column) {
        if (!column.isSelected)
            this.appModel.Properties =
                this.appModel.Properties.filter(p => (p.Settings ?? {}).ColumnId !== column.Id);
        else {
            const sqlType = column.ColumnType;

            // بخش اصلی نوع را جدا می‌کنیم: nvarchar(50) → nvarchar
            const normalized = sqlType.trim().toLowerCase().replace(/\(.*\)/, "");

            // پیدا کردن نوع مناسب
            const match = this.typeMap.find(t => t.sql.test(normalized));
            const maxOrder = this.appModel.Properties.length
                ? Math.max(...this.appModel.Properties.map(p => p.ViewOrder || 0))
                : 1;
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

        this.setSelectedEntitiesWidgetHeight();
    }

    onModelBaseNameChange() {
        this.appModel.ModelName = this.appModel.Settings.BaseName + this.appModel.Settings.Postfix;
    }

    onModelNamePostfixChange() {
        this.appModel.ModelName = this.appModel.Settings.BaseName + this.appModel.Settings.Postfix;
        this.appModel.Settings.PostfixModified = true;
    }

    onSetValidNameClick() {
        this.appModel.Settings.BaseName = this.globalService.normalizeName(this.appModel.Settings.BaseName);
        if (this.appModel.Settings.Postfix)
            this.appModel.Settings.Postfix = this.globalService.normalizeName(this.appModel.Settings.Postfix);

        this.appModel.ModelName = this.appModel.Settings.BaseName + this.appModel.Settings.Postfix;
    }

    onChangeModelTypeClick(modelType) {
        this.appModel.ModelType = modelType;

        if (!this.appModel.Settings.PostfixModified) {
            this.appModel.Settings.Postfix = this.getModuleTypeText(modelType);
            this.onModelNamePostfixChange();
        }
    }

    getModuleTypeText(modelType) {
        switch (modelType) {
            case 0:
                return '';
            case 1:
                return 'ViewModel';
            case 2:
                return 'ListItem';
            case 3:
                return 'Dto';
            case -1:
                return 'Info';
        }
    }

    onAddPropertyClick() {
        const maxOrder = this.appModel.Properties.length
            ? Math.max(...this.appModel.Properties.map(p => p.ViewOrder || 0))
            : 1;
        const property = { ViewOrder: maxOrder + 1 };

        this.appModel.Properties.push(property);

        this.$timeout(() => {
            this.$scope.$broadcast("onEditProperty" + this.appModel.Properties.length);
            this.setSelectedEntitiesWidgetHeight()
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
    }

    onPropertySwapClick(index, swappedIndex) {
        const props = this.appModel.Properties;

        if (swappedIndex < 0 || swappedIndex >= props.length) return;

        // swap
        const temp = props[index];
        props[index] = props[swappedIndex];
        props[swappedIndex] = temp;

        // Only update the two items that changed
        props[index].ViewOrder = index + 1;
        props[swappedIndex].ViewOrder = swappedIndex + 1;
    }

    onSaveAppModelClick() {
        this.form.validated = true;
        this.form.validator(this.appModel);

        if (this.form.valid) {
            this.running = "save-appModel";
            this.awaitAction = {
                title: "Creating AppModel",
                subtitle: "Just a moment for creating appModel...",
            };

            this.currentTabKey = this.$rootScope.currentTab.key;

            this.apiService.post("Studio", "SaveAppModel", this.appModel).then((data) => {
                this.appModel = data;

                this.notifyService.success("AppModel updated has been successfully");

                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.appModel.Id,
                    title: this.appModel.ModelName,
                    key: this.currentTabKey,
                });

                this.$rootScope.refreshSidebarExplorerItems();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                if (error.data.HResult == "-2146232060")
                    this.notifyService.error(
                        `AppModel name must be unique.${this.appModel.ModelName} is already in the scenario appModels`
                    );
                else if (error.data.HResult == "-2146233088")
                    this.notifyService.error(
                        `Table name must be unique.${this.appModel.TableName} is already in the database`
                    );
                else this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    checkConsecutiveKeywordsRegex(str, keywords) {
        if (!str || !keywords || keywords.length === 0) return false;

        // ایجاد pattern با join کردن کلمات با |
        const pattern = keywords.join("|");

        // regex که بررسی می‌کند آخر string بیش از یک کلمه از لیست متوالی آمده باشد
        const regex = new RegExp(`(${pattern}){2,}$`, "i"); // i → case-insensitive, {2,} → حداقل دو بار

        return regex.test(str);
    }

    checkConsecutiveKeywordsWithMessage(str, keywords) {
        if (!str || !keywords || keywords.length === 0) return { warning: false };

        const normalizedStr = str.trim();
        const pattern = keywords.join("|");

        // regex برای گرفتن کلمات متوالی در انتهای رشته (حداقل دو تا)
        const regex = new RegExp(`(${pattern}){2,}$`, "i");

        if (!regex.test(normalizedStr)) {
            return !false;
        }

        // پیدا کردن کلمات انتهایی
        let tempStr = normalizedStr;
        const matchedWords = [];

        while (true) {
            let matched = false;
            for (const kw of keywords) {
                const kwRegex = new RegExp(kw + "$", "i"); // فقط انتهای رشته
                if (kwRegex.test(tempStr)) {
                    matchedWords.unshift(kw); // به ابتدای array اضافه می‌کنیم تا ترتیب درست باشد
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

        // تمام کلمات بعد از اولین کلمه اضافی → هشدار
        const extraWords = matchedWords.slice(1);

        return `The word${extraWords.length > 1 ? 's' : ''} ${extraWords.join(", ")} ${extraWords.length > 1 ? "are" : "is"} added, please remove ${extraWords.length > 1 ? "them" : "it"}.`
    }

    splitNameByKeywords(str, keywords) {
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

    setSelectedEntitiesWidgetHeight() {
        var $left = $(`#selectedEntitiesLeftWidget_${this.appModel.Id ?? ''}`);
        var $right = $(`#selectedEntitiesRightWidget_${this.appModel.Id ?? ''}>.app-model-card`);

        $left.css('height', 'auto');     // ریست ارتفاع
        var h = $right.outerHeight();    // ارتفاع واقعی right
        $left.height(h);
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}