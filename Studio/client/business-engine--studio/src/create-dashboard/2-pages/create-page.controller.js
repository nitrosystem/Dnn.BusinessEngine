export class CreateDashboardPageController {
    constructor(
        $scope,
        $rootScope,
        studioService,
        $compile,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService,
        $deferredBroadcast
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.studioService = studioService;
        this.$compile = $compile;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.$deferredBroadcast = $deferredBroadcast;

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");
        const parent = this.globalService.getParameterByName("parent");
        const pageId = this.globalService.getParameterByName("page") || null;
        const parentId = this.globalService.getParameterByName("page-parent") || null;

        this.moduleId = parent || id;

        this.running = "get-page";
        this.awaitAction = {
            title: "Loading Page",
            subtitle: "Just a moment for loading dashboard page...",
        };

        this.apiService.get("Module", "GetDashboardPage", { moduleId: this.moduleId, pageId: pageId, }).then((data) => {
            this.pages = data.Pages;
            this.roles = this.$rootScope.Roles;
            this.page = {
                ...(data.Page || {
                    DashboardId: data.DashboardId,
                    ParentId: parentId,
                    PageType: 0,
                    IsVisible: true,
                    InheritPermissionFromDashboard: true,
                    Module: {}
                }),
            };

            this.populatePages(this.pages);

            this.$timeout(() => {
                this.$scope.$broadcast("onEditPage");
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

        this.setForm();
    }

    setForm() {
        this.form = this.validationService.init({
            PageType: {
                id: "drpPageType",
                required: true,
            },
            PageName: {
                id: "txtPageName",
                rule: (value) => {
                    if (this.page.PageType == 3)
                        return true;
                    else if (/^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Page name is not valid";
                    else
                        return true;
                },
            },
            Title: {
                id: "txtPageTitle",
                required: true,
            },
            "Module.ModuleType": {
                id: "drpModuleType",
                rule: (value) => {
                    if (this.page.PageType === 0 && this.page.IncludeModule && !value)
                        return false;
                    else
                        return true;
                },
                required: true,
            },
            "Module.ModuleName": {
                id: "txtModuleName",
                rule: (value) => {
                    if (this.page.PageType === 0 && this.page.IncludeModule && /^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Page name is not valid";
                    else
                        return true;
                },
                required: true,
            },
            "Module.ModuleTitle": {
                id: "txtModuleTitle",
                rule: (value) => {
                    if (this.page.PageType === 0 && this.page.IncludeModule && !value)
                        return false;
                    else
                        return true;
                },
                required: true,
            },
        },
            true,
            this.$scope,
            "$.page"
        );
    }

    onSetPageValidNameClick() {
        this.page.PageName = this.globalService.normalizeName(this.page.PageName);
    }

    onSetModuleValidNameClick() {
        this.page.Module.ModuleName = this.globalService.normalizeName(this.page.Module.ModuleName);
    }

    onSavePageClick() {
        this.form.validated = true;
        this.form.validator(this.page);
        if (this.form.valid) {
            this.running = "save-page";
            this.awaitAction = {
                title: "Saving Page",
                subtitle: "Just a moment for saving the dashboard page...",
            };

            if (this.page.PageType === 0 && this.page.Module && !this.page.Module.Id) {
                this.page.Module.PageId = this.page.Id;
                this.page.Module.ScenarioId = this.$rootScope.scenario.Id;
                this.page.Module.ParentId = this.moduleId;
            }

            this.apiService.post("Module", "SaveDashboardPage", this.page).then((data) => {
                this.notifyService.success("Dashboard page updated has been successfully");

                this.$scope.$emit('onDashboardPagesReload', { dashboardId: this.page.DashboardId });
                this.onCancelPageClick();

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

    onCancelPageClick() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.page;
        }, 200);
    }

    onDeleteModuleClick() {
        if (confirm("Are you sure delete this module?")) {
            this.running = "delete-page-module";
            this.awaitAction = {
                title: "Deleting Page Module",
                subtitle: "Just a moment for deleting page module...",
            };

            this.apiService.post("Module", "DeleteDashboardPageModule", { Id: this.page.Module.ModuleId, }).then((data) => {
                this.notifyService.success("Dashboard page module deleted has been successfully");

                delete this.page.Module;

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc =
                    this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    onGotoModuleBuilderClick() {

    }

    populatePages(pages) {
        const allPages = [{ Id: null, LevelTitle: "None" }];

        const runner = (page, level) => {
            page.LevelTitle = "...".repeat(level) + page.Title;
            page.Level = level;

            allPages.push(page);

            (page.Pages || []).forEach(c => {
                c.ParentId = page.Id;
                runner(c, level + 1);
            });
        };

        pages.forEach(c => {
            c.ParentId = 0;
            runner(c, 0);
        });

        this.allPages = allPages;
    }

    getPageByPageId(pageId) {
        var result;

        const findNestedPage = (pages) => {
            _.forEach(pages, (p) => {
                if (p.Id == pageId) result = p;
                else return findNestedPage(p.Pages);
            });
        };

        findNestedPage(this.pages);

        return result;
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}