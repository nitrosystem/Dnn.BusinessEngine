import Swal from 'sweetalert2';
import { GlobalUtil } from '../../../shared/utils';

export class ServicesController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        notificationService
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.notifyService = notificationService;

        this.filter = { pageIndex: 1, pageSize: 10 };
        this.paging = {
            visiblePages: 10,
            onPageClick: (e, pageIndex) => {
                this.filter.pageIndex = pageIndex;
                this._getServices();
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
        this._getServices(true);
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...['services']);
        this.$rootScope.explorerCurrentItem = 'services';
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
        this.entities = null;
        this.filter.filtered = true;
        this.filter.pageIndex = 1;
        this._getServices(true);
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10 };
        this._getServices(true);
    }

    onAddServiceClick() {
        this.$scope.$emit('onGotoPage', { page: 'create-service' });
    }

    onEditServiceClick(id, title) {
        this.$scope.$emit('onGotoPage', {
            page: 'create-service',
            id: id,
            title: title,
        });
    }

    onDeleteServiceClick(id, index) {
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
                this.running = 'delete-services';
                this.awaitAction = {
                    title: 'Delete Service',
                    subtitle: 'Just a moment for deleting service...',
                };

                this.apiService.post('Studio', 'DeleteService', { Id: id }).then((data) => {
                    this.services.splice(index, 1);

                    this.notifyService.info('Service has been deleted successfully.');

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
        });
    }

    //#endregion

    //#region Private Methods

    _getServices(reinitPagination) {
        this.running = 'get-services';
        this.awaitAction = {
            title: 'Loading Services',
            subtitle: 'Just a moment for loading services...',
        };

        this.apiService.get('Studio', 'GetServices', {
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            serviceDomain: this.filter.serviceDomain,
            serviceType: this.filter.serviceType,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.serviceDomains = [...new Set(data.ServiceTypes.map(x => x.ServiceDomain))];
            this.serviceTypes = data.ServiceTypes;
            this.services = data.Services;

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