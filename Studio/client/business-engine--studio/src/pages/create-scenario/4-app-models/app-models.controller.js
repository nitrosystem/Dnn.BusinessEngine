import Swal from 'sweetalert2';
import { GlobalUtil } from '../../../shared/utils';

export class AppModelsController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        notificationService,
        baseOptions
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        this.filter = { pageIndex: 1, pageSize: 10 };
        this.paging = {
            visiblePages: 10,
            onPageClick: (e, pageIndex) => {
                this.filter.pageIndex = pageIndex;
                this._getAppModels();
            }
        }

        const currentTabKey = this.$rootScope.currentTab.key;
        $scope.$on(`onTab--${currentTabKey}--Selected`, (e, args) => {
            this.onFocusModule();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        this._getAppModels(true)
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...['app-models']);
        this.$rootScope.explorerCurrentItem = 'app-models';
    }

    onBoxModeClick() {
        this.displayMode = 'box';
    }

    onTableModeClick() {
        this.displayMode = 'table';
    }

    onSortingClick(sortBy) {
        this.filter.sortBy = sortBy;
        this.onApplyFilter();
    }

    onApplyFilter() {
        this.entities=null;
        this.filter.filtered = true;
        this.filter.pageIndex = 1;
        this._getAppModels(true);
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10 };
        this._getAppModels(true);
    }

    onAddAppModelClick() {
        this.$scope.$emit('onGotoPage', { page: 'create-app-model' });
    }

    onEditAppModelClick(id, title) {
        this.$scope.$emit('onGotoPage', {
            page: 'create-app-model',
            id: id,
            title: title,
        });
    }

    onDeleteAppModelClick(id, $index) {
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
                this.running = 'delete-appModels';
                this.awaitAction = {
                    title: 'Delete AppModel',
                    subtitle: 'Just a moment for deleting App Model...',
                };

                this.apiService.post('Studio', 'DeleteAppModel', { Id: id }).then((data) => {
                    if (data) {
                        this.notifyService.info('AppModel has been deleted successfully.');
                        this.appModels.splice($index, 1);
                        this.$rootScope.refreshSidebarExplorerItems();
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

    _getAppModels(reinitPagination) {
        this.running = 'get-app-models';
        this.awaitAction = {
            title: 'Loading App Models',
            subtitle: 'Just a moment for loading view models...',
        };

        this.apiService.get('Studio', 'GetAppModels', {
            scenarioId: this.baseOptions.scenarioId,
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            modelType: this.filter.modelType,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.appModels = data.AppModels;

            this.paging.totalPages = data.Page.PageCount;
            if (reinitPagination)
                this.$timeout(() => {
                    this.$rootScope.$emit('onReinitPagination')
                }, 500);

            this.onFocusModule();

            this.running = null;
            this.awaitAction = null;
        });
    }

    //#endregion
}