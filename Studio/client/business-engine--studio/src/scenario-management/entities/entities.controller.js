import Swal from 'sweetalert2'

export class EntitiesController {
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
        this.filter = { pageIndex: 1, pageSize: 12 };

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        this.getEntities();
    }

    getEntities() {
        this.running = "get-entities";
        this.awaitAction = {
            title: "Loading Entities",
            subtitle: "Just a moment for loading entities...",
        };

        this.apiService.get("Studio", "GetEntities", {
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            searchText: this.filter.searchText,
            entityType: this.filter.entityType,
            isReadonly: this.filter.IsReadonly,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.entities = data.Entities;

            let $this = this;
            this.paging = {
                visiblePages: 10, totalPages: data.Page.PageCount, onPageClick: (e, pageIndex) => {
                    $this.filter.pageIndex = pageIndex;
                    $this.getEntities();
                }
            }

            this.entitiesColumnsCount = this.entities.reduce((total, e) => total + (e.Columns?.length || 0), 0);

            this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...["entities"]);
        this.$rootScope.explorerCurrentItem = "entities";
    }

    onSortingClick(sortBy) {
        this.filter.sortBy = sortBy;
        this.onApplyFilter();
    }

    onApplyFilter() {
        delete this.entities;

        this.filter.pageIndex = 1;
        this.$timeout(() => this.getEntities());
    }

    onClearFilterClick() {
        this.filter = { pageIndex: 1, pageSize: 10, searchText: '', entityType: '' };
        this.onApplyFilter();
    }

    onAddEntityClick() {
        this.$scope.$emit("onGotoPage", { page: "create-entity" });
    }

    onEditEntityClick(id, title) {
        this.$scope.$emit("onGotoPage", {
            page: "create-entity",
            id: id,
            title: title,
        });
    }

    onDeleteEntityClick(id, index) {
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
                this.running = "delete-entity";
                this.awaitAction = {
                    title: "Delete Entity",
                    subtitle: "Just a moment for deleting entity...",
                };

                this.apiService.post("Studio", "DeleteEntity", { Id: id }).then((data) => {
                    this.entities.splice(index, 1);

                    this.notifyService.success("Entity deleted has been successfully");

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