import { GlobalHelper, UrlHelper } from "../../../shared/helpers";
import { GlobalUtil } from "../../../shared/utils";

export class CreateDashboardPageController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        validationService,
        notificationService,
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');
        const parent = UrlHelper.getParameterByName('parent');
        const pageId = UrlHelper.getParameterByName('page') || null;
        const parentId = UrlHelper.getParameterByName('page-parent') || null;

        this.moduleId = parent || id;

        this.running = 'get-page';
        this.awaitAction = {
            title: 'Loading Page',
            subtitle: 'Just a moment for loading dashboard page...',
        };

        this.apiService.get('Module', 'GetDashboardPage', { moduleId: this.moduleId, pageId: pageId, }).then((data) => {
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

            this._populatePages(this.pages);
            this._setForm();
            this.$timeout(() => {
                this.$scope.$broadcast('onEditPage');
            });

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

    onSetPageValidNameClick() {
        this.page.PageName = GlobalHelper.normalizeName(this.page.PageName);
    }

    onSetModuleValidNameClick() {
        this.page.Module.ModuleName = GlobalHelper.normalizeName(this.page.Module.ModuleName);
    }

    onSavePageClick() {
        this.form.validated = true;
        this.form.validator(this.page);
        if (this.form.valid) {
            this.running = 'save-page';
            this.awaitAction = {
                title: 'Saving Page',
                subtitle: 'Just a moment for saving the dashboard page...',
            };

            if (this.page.PageType === 0 && this.page.Module && !this.page.Module.Id) {
                this.page.Module.PageId = this.page.Id;
                this.page.Module.ScenarioId = this.$rootScope.scenario.Id;
                this.page.Module.ParentId = this.moduleId;
            }
            else if (this.page.PageType === 3) {
                this.page.IncludeModule = null;
                this.page.Module = null;
                this.page.parentId = null;
                this.page.PageName = null;
            }

            this.apiService.post('Module', 'SaveDashboardPage', this.page).then((data) => {
                this.notifyService.success('Dashboard page updated has been successfully');

                this.$scope.$emit('onDashboardPagesReload', { dashboardId: this.page.DashboardId });
                this.onCloseWindow();

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

    onCloseWindow() {
        this.$scope.$emit('onHideEditPage');
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            PageType: {
                id: 'drpPageType',
                required: true,
            },
            PageName: {
                id: 'txtPageName',
                rule: (value) => {
                    if (this.page.PageType == 3)
                        return true;
                    else if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) == false)
                        return 'Page name is not valid';
                    else
                        return true;
                },
            },
            Title: {
                id: 'txtPageTitle',
                required: true,
            },
            'Module.ModuleType': {
                id: 'drpModuleType',
                rule: (value) => {
                    if (this.page.PageType === 0 && this.page.IncludeModule && !value)
                        return false;
                    else
                        return true;
                },
                required: true,
            },
            'Module.ModuleName': {
                id: 'txtModuleName',
                rule: (value) => {
                    if (this.page.PageType === 0 && this.page.IncludeModule && /^[A-Za-z][A-Za-z0-9_]*$/.test(value) == false)
                        return 'Page name is not valid';
                    else
                        return true;
                },
                required: true,
            },
            'Module.ModuleTitle': {
                id: 'txtModuleTitle',
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
            '$.page'
        );
    }

    _populatePages(pages) {
        const allPages = [{ Id: null, LevelTitle: 'None' }];

        const processPage = (page, level) => {
            if(page.PageType===3) return;
            
            page.Level = level;
            page.LevelTitle = level === 0
                ? page.Title
                : '...'.repeat(level) + page.Title;

            allPages.push(page);

            const children = pages.filter(p => p.ParentId === page.Id);
            for (const child of children) {
                processPage(child, level + 1);
            }
        }

        for (const page of pages.filter(p => !p.ParentId)) {
            processPage(page, 0);
        }

        this.allPages = allPages;
        return allPages;
    }

    //#endregion
}