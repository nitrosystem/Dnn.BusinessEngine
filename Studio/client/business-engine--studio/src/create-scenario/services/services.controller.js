import Swal from "sweetalert2";

export class ServicesController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        studioService,
        globalService,
        apiService,
        notificationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.globalService = globalService;
        this.apiService = apiService;
        this.notifyService = notificationService;

        this.filter = { pageIndex: 1, pageSize: 10 };

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        this.getServices();
    }

    getServices() {
        this.running = "get-services";
        this.awaitAction = {
            title: "Loading Services",
            subtitle: "Just a moment for loading services...",
        };
        this.apiService.get("Studio", "GetServices", {
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            serviceDomain: this.filter.serviceDomain,
            serviceType: this.filter.serviceType,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.serviceDomains = [...new Set(data.ServiceTypes.map(x => x.ServiceDomain))];
            this.serviceTypes = [...new Set(data.ServiceTypes.map(x => x.ServiceType))];
            this.services = data.Services;

            let $this = this;
            this.paging = {
                visiblePages: 10, totalPages: data.Page.PageCount, onPageClick: (e, pageIndex) => {
                    $this.filter.pageIndex = pageIndex;
                    $this.getServices();
                }
            }

            this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        });
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...["services"]);
        this.$rootScope.explorerCurrentItem = "services";
    }

    onSortingClick(sortBy) {
        this.filter.sortBy = sortBy;
        this.onApplyFilter();
    }

    onApplyFilter() {
        delete this.entities;

        this.filter.filtered = true;
        this.filter.pageIndex = 1;
        this.getServices();
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10 };
        this.getServices();
    }

    onAddServiceClick() {
        this.$scope.$emit("onGotoPage", { page: "create-service" });
    }

    onEditServiceClick(id, title) {
        this.$scope.$emit("onGotoPage", {
            page: "create-service",
            id: id,
            title: title,
        });
    }

    onDeleteServiceClick(id, index) {
        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary entity!</p><b></b>',
            icon: "warning",
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector("b");
                timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                this.running = "delete-services";
                this.awaitAction = {
                    title: "Delete Service",
                    subtitle: "Just a moment for deleting service...",
                };

                this.apiService.post("Studio", "DeleteService", { Id: id }).then((data) => {
                    this.services.splice(index, 1);

                    this.notifyService.success("Service deleted has been successfully");

                    this.$rootScope.refreshSidebarExplorerItems();

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
        });
    }

    onTableModeClick() {
        this.displayMode = 'table';
    }

    onBoxModeClick() {
        this.displayMode = 'box';
    }
}