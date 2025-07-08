import { GlobalSettings } from "../../angular-configs/global.settings";

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
        this.apiService.get("Studio","GetServices", {
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            serviceType: this.filter.serviceType,
            serviceSubtype: this.filter.serviceSubtype,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.serviceTypes = [...new Set(data.ServiceTypes.map(item => item.ServiceType))];
            this.serviceSubtypes = data.ServiceTypes;
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
        delete this.services;

        this.filter.pageIndex = 1;
        this.$timeout(() => this.getServices());
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10, searchText: '' };
        this.onApplyFilter();
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
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary service!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "get-services";
                this.awaitAction = {
                    title: "Remove Service",
                    subtitle: "Just a moment for removing service...",
                };

                this.apiService.post("Studio", "DeleteService", { ID: id }).then(
                    (data) => {
                        this.services.splice(index, 1);

                        this.notifyService.success("Service deleted has been successfully");

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

    onTableModeClick() {
        this.displayMode = 'table';
    }

    onBoxModeClick() {
        this.displayMode = 'box';
    }
}