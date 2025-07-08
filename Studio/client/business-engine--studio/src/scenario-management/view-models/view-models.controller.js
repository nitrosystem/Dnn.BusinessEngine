
export class ViewModelsController {
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
        this.getViewModels()
    }

    getViewModels() {
        this.running = "get-view-models";
        this.awaitAction = {
            title: "Loading View Models",
            subtitle: "Just a moment for loading view models...",
        };

        this.apiService.get("Studio", "GetViewModels", {
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.viewModels = data.ViewModels;

            let $this = this;
            this.paging = {
                visiblePages: 10, totalPages: data.Page.PageCount, onPageClick: (e, pageIndex) => {
                    $this.filter.pageIndex = pageIndex;
                    $this.getViewModels();
                }
            }

            this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        });
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...["view-models"]);
        this.$rootScope.explorerCurrentItem = "view-models";
    }

    onSortingClick(sortBy) {
        this.filter.sortBy = sortBy;
        this.onApplyFilter();
    }

    onApplyFilter() {
        delete this.viewModels;

        this.filter.pageIndex = 1;
        this.$timeout(() => this.getViewModels());
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10, searchText: '' };
        this.onApplyFilter();
    }

    onAddViewModelClick() {
        this.$scope.$emit("onGotoPage", { page: "create-view-model" });
    }

    onEditViewModelClick(id, title) {
        this.$scope.$emit("onGotoPage", {
            page: "create-view-model",
            id: id,
            title: title,
        });
    }

    onDeleteViewModelClick(id, index) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary view model!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "get-viewModels";
                this.awaitAction = {
                    title: "Remove ViewModel",
                    subtitle: "Just a moment for removing viewModel...",
                };

                this.apiService.post("Studio", "DeleteViewModel", { ID: id }).then(
                    (data) => {
                        this.viewModels.splice(index, 1);

                        this.notifyService.success("ViewModel deleted has been successfully");

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
}