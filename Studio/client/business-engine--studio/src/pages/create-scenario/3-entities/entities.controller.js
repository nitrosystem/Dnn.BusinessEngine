import Swal from 'sweetalert2'
import { GlobalUtil } from '../../../shared/utils';

export class EntitiesController {
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

        this.filter = { pageIndex: 1, pageSize: 12 };
        this.paging = {
            visiblePages: 10,
            onPageClick: (e, pageIndex) => {
                this.filter.pageIndex = pageIndex;
                this._getEntities();
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
        this._getEntities(true);
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...['entities']);
        this.$rootScope.explorerCurrentItem = 'entities';
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

    onIsReadonlyChange() {
        if (this.filter.IsReadonly === false)
            this.filter.IsReadonly = null;

        this.onApplyFilter();
    }

    onApplyFilter() {
        this.entities = null;
        this.filter.filtered = true;
        this.filter.pageIndex = 1;
        this._getEntities(true);
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10 };
        this._getEntities(true);
    }

    onAddEntityClick() {
        this.$scope.$emit('onGotoPage', { page: 'create-entity' });
    }

    onEditEntityClick(id, title) {
        this.$scope.$emit('onGotoPage', {
            page: 'create-entity',
            id: id,
            title: title,
        });
    }

    onDeleteEntityClick(id, $index) {
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

                this.apiService.post('Studio', 'DeleteEntity', { Id: id }).then((data) => {
                    if (data) {
                        this.notifyService.info('Entity has been deleted successfully.');
                        this.entities.splice($index, 1);
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

    _getEntities(reinitPagination) {
        this.running = 'get-entities';
        this.awaitAction = {
            title: 'Loading Entities',
            subtitle: 'Just a moment for loading entities...',
        };

        this.apiService.get('Studio', 'GetEntities', {
            scenarioId: this.baseOptions.scenarioId,
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            entityType: this.filter.entityType,
            isReadonly: this.filter.IsReadonly,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.entities = data.Entities;

            this.paging.totalPages = data.Page.PageCount;
            if (reinitPagination)
                this.$timeout(() => {
                    this.$rootScope.$emit('onReinitPagination')
                }, 500);

            this.entitiesColumnsCount = this.entities.reduce((total, e) => total + (e.Columns?.length || 0), 0);

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

    //#endregion
}