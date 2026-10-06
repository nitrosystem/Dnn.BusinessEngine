import Swal from 'sweetalert2';
import editPageWidget from './create-page.html';
import { GlobalUtil } from '../../../shared/utils';
import { UrlHelper } from '../../../shared/helpers';

export class CreateDashboardPagesController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        validationService,
        notificationService
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        this.editPageWidget = editPageWidget;

        $scope.createDashboardValidatedStep.push(2);

        $scope.$on('onHideEditPage', (e, args) => {
            this.onCloseEditPageWindow();
        });

        $scope.$on('onDashboardPagesReload', (e, args) => {
            if (this.dashboard.Id === args.dashboardId)
                this.onPageLoad();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');
        const parent = UrlHelper.getParameterByName('parent');
        const moduleId = parent || id;

        this.running = 'get-dashboard-pages';
        this.awaitAction = {
            title: 'Loading Dashboard Pages',
            subtitle: 'Just a moment for loading dashboard pages...',
        };

        this.apiService.get('Module', 'GetDashboardPages', { moduleId: moduleId, }).then((data) => {
            this.dashboard = data.Dashboard;
            this.pages = data.Pages;

            this._populatePages(this.pages);

            this.running = null;
            this.awaitAction = null;
        });
    }

    oGotoModuleBuilderClick(moduleId, template) {
        moduleId = moduleId ?? UrlHelper.getParameterByName('id');
        const page = {
            page: 'create-module',
            id: moduleId,
            subParams: {
                st: !template ? 2 : 5
            }
        };

        this.$scope.$emit('onGotoPage', page);
    }

    onEditPageClick(page, parent) {
        let url = UrlHelper.replaceUrlParam('page', page ?? '');
        url = UrlHelper.replaceUrlParam('page-parent', parent, url);
        UrlHelper.pushState(url);

        this.page = page ?? {};
        this.workingMode = 'edit-page';
        this.$scope.$emit('onShowRightWidget');
    }

    onSwapPageClick(page, state) {
        const list = this._findListContainingPage(page.Id);
        const index = list.findIndex(p => p.Id === page.Id);
        const newIndex = state === 1 ? index - 1 : index + 1;
        [list[index], list[newIndex]] = [list[newIndex], list[index]];

        this._normalizeViewOrders(list);

        const sortedIds = this._collectSortedIds(list);

        this.running = 'sort-pages';
        this.awaitAction = {
            title: 'Sorting Pages',
            subtitle: 'Just a moment for sorting pages...',
        };
        this.apiService.post('Module', 'SortDashboardPages', {
            Id: this.dashboard.Id,
            SortedIds: sortedIds,
        }).then(() => {
            this.notifyService.success('Sorting pages has been successfully');
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

    onLeftPageClick(page) {
        const currentParent = this._findParentOfPage(page.Id);
        const grandParent = this._findParentOfPage(currentParent.Id);

        const newParentPages = grandParent ? grandParent.Pages : this.pages;
        const oldIndex = currentParent.Pages.findIndex(p => p.Id === page.Id);
        currentParent.Pages.splice(oldIndex, 1);

        const parentIndexInNew = newParentPages.findIndex(p => p.Id === currentParent.Id);
        newParentPages.splice(parentIndexInNew + 1, 0, page);

        page.IsChild = !!grandParent;

        this._normalizeViewOrders(currentParent.Pages);
        this._normalizeViewOrders(newParentPages);
        this._updatePageParent({
            Id: page.Id,
            ParentId: grandParent ? grandParent.Id : null,
        });
    }

    onRightPageClick(page) {
        const currentList = this._findListContainingPage(page.Id);
        const index = currentList.findIndex(p => p.Id === page.Id);
        if (index <= 0) return;

        const newParent = currentList[index - 1];
        if (!newParent.Pages) newParent.Pages = [];
        currentList.splice(index, 1);

        page.IsChild = true;

        newParent.Pages.push(page);
        newParent.isCollapsed = false;

        this._normalizeViewOrders(currentList);
        this._normalizeViewOrders(newParent.Pages);

        this._updatePageParent({
            Id: page.Id,
            ParentId: newParent.Id,
        });
    }

    onDeletePageClick(pageId) {
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
                this.running = 'delete-dashboard-page';
                this.awaitAction = {
                    title: 'Deleting Dashboard page',
                    subtitle: 'Just a moment for deleting dashboard page...',
                };

                this.apiService.post('Module', 'DeleteDashboardPage', { Id: pageId }).then((data) => {
                    if (data) {
                        this.notifyService.success('Dashboard page deleted has been successfully');

                        const parent = this._findParentOfPage(pageId)?.Pages ?? this.pages;
                        const index = parent.findIndex(p => p.Id === pageId);
                        if (index >= 0) parent.splice(index, 1);
                    }

                    //this.$rootScope.refreshSidebarExplorerItems();

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

    onPreviousStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 1 });
    }

    onCloseEditPageWindow() {
        this.$scope.$emit('onHideRightWidget');
        this.$timeout(() => this.workingMode = null, 200);
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _updatePageParent(page) {
        this.apiService.post('Module', 'UpdatePageParent', page).then((data) => {
            this.notifyService.success('Updated page parent has been successfully');

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

    _populatePages(pages) {
        const allPages = [{ Id: null, LevelTitle: 'None' }];

        const runner = (page, level, parentId) => {
            page.ParentId = parentId;
            page.Level = level;
            page.LevelTitle = '...'.repeat(level) + page.Title;
            allPages.push(page);

            for (const child of page.Pages ?? []) {
                runner(child, level + 1, page.Id);
            }
        };

        for (const page of pages ?? []) {
            runner(page, 0, 0);
        }

        this.allPages = allPages;
    }

    _findParentOfPage(pageId, pages = this.pages, parent = null) {
        for (const p of pages) {
            if (p.Id === pageId) return parent;

            if (p.Pages && p.Pages.length) {
                const found = this._findParentOfPage(pageId, p.Pages, p);
                if (found !== undefined) return found;
            }
        }

        return undefined;
    }

    _findListContainingPage(pageId) {
        const parent = this._findParentOfPage(pageId);
        if (parent === undefined) return null;

        return parent === null
            ? this.pages
            : parent.Pages;
    }

    _normalizeViewOrders(pages) {
        pages.forEach((p, i) => { p.ViewOrder = i; });
    }

    _collectSortedIds(pages) {
        const result = [];
        const walk = (list) => {
            for (const p of list) {
                result.push(p.Id);
                if (p.Pages && p.Pages.length) walk(p.Pages);
            }
        };

        walk(pages);
        return result;
    }

    //#endregion
}