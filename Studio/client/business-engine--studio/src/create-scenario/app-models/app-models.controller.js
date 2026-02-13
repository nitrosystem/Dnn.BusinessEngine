import Swal from "sweetalert2";

export class AppModelsController {
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
        this.getAppModels()
    }

    getAppModels() {
        this.running = "get-app-models";
        this.awaitAction = {
            title: "Loading App Models",
            subtitle: "Just a moment for loading view models...",
        };

        this.apiService.get("Studio", "GetAppModels", {
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.appModels = data.AppModels;

            let $this = this;
            this.paging = {
                visiblePages: 10, totalPages: data.Page.PageCount, onPageClick: (e, pageIndex) => {
                    $this.filter.pageIndex = pageIndex;
                    $this.getAppModels();
                }
            }

            this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        });
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...["app-models"]);
        this.$rootScope.explorerCurrentItem = "app-models";
    }

    onSortingClick(sortBy) {
        this.filter.sortBy = sortBy;
        this.onApplyFilter();
    }

    onApplyFilter() {
        delete this.entities;

        this.filter.filtered = true;
        this.filter.pageIndex = 1;
        this.getAppModels();
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10 };
        this.getAppModels();
    }

    onAddAppModelClick() {
        this.$scope.$emit("onGotoPage", { page: "create-app-model" });
    }

    onEditAppModelClick(id, title) {
        this.$scope.$emit("onGotoPage", {
            page: "create-app-model",
            id: id,
            title: title,
        });
    }

    onDeleteAppModelClick(id, index) {
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
                this.running = "delete-appModels";
                this.awaitAction = {
                    title: "Delete AppModel",
                    subtitle: "Just a moment for deleting App Model...",
                };

                this.apiService.post("Studio", "DeleteAppModel", { Id: id }).then((data) => {
                    this.appModels.splice(index, 1);

                    this.notifyService.success("AppModel deleted has been successfully");

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

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}