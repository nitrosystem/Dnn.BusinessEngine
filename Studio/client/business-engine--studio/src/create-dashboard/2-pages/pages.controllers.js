import editPageWidget from "./create-page.html";

export class CreateDashboardPagesController {
    constructor(
        $scope,
        $rootScope,
        $compile,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService,
        $deferredBroadcast
    ) {
        "ngInject";

        this.$rootScope = $rootScope;
        this.$compile = $compile;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.$deferredBroadcast = $deferredBroadcast;

        this.editPageWidget = editPageWidget;

        this.$rootScope.createDashboardValidatedStep.push(2);

        $scope.$on("onCreateDashboardValidateStep2", (e, task, args) => {
            task.wait(() => {
                const $defer = this.$q.defer();

                $defer.resolve(true);

                return $defer.promise;
            });
        });

        $scope.$on("onHideRightWidget", (e, args) => {
            delete this.workingMode;
        });

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");
        const parent = this.globalService.getParameterByName("parent");
        const moduleId = parent || id;

        this.running = "get-dashboard-pages";
        this.awaitAction = {
            title: "Loading Dashboard Pages",
            subtitle: "Just a moment for loading dashboard pages...",
        };

        this.apiService.get("Module", "GetDashboardPages", { moduleId: moduleId, }).then((data) => {
            this.dashboard = data.Dashboard;
            this.pages = data.Pages;
            this.roles = data.Roles;

            this.populatePages(this.pages);

            delete this.running;
            delete this.awaitAction;
        });
    }

    onPreviousStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 1 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 3 });
    }

    onEditPageClick(page, parent) {
        let url = this.globalService.replaceUrlParam('page', page ?? '');
        this.globalService.pushState(url);

        if (parent) {
            url = this.globalService.replaceUrlParam('page-parent', parent);
            this.globalService.pushState(url);
        }

        this.page = page ?? {};
        this.workingMode = "edit-page";
        this.$scope.$emit("onShowRightWidget");
    }

    onUpPageClick(page, $index) {
        const $page = $(`li[data-page-id="${page.Id}"]`);
        const parentId = $page.parent().data("page-id");
        const parentPage = _.sortBy(
            parentId ? this.getPageByPageId(parentId).Pages : this.pages, ["ViewOrder"]
        );
        const preViewOrder = parentPage[$index - 1].ViewOrder;

        parentPage[$index - 1].ViewOrder = parentPage[$index].ViewOrder;
        parentPage[$index].ViewOrder = preViewOrder;

        var sortedPageIds = [];
        _.forEach(_.sortBy(parentPage, ["ViewOrder"]), (p) =>
            sortedPageIds.push(p.Id)
        );

        const sortedPages = {
            DashboardId: this.dashboard.DashboardId,
            SortedPageIds: sortedPageIds,
        };

        this.sortPages(sortedPages);
    }

    onDownPageClick(page, $index) {
        const $page = $(`li[data-page-id="${page.Id}"]`);
        const parentId = $page.parent().data("page-id");
        const parentPage = _.sortBy(
            parentId ? this.getPageByPageId(parentId).Pages : this.pages, ["ViewOrder"]
        );
        const nextViewOrder = parentPage[$index + 1].ViewOrder;

        parentPage[$index + 1].ViewOrder = parentPage[$index].ViewOrder;
        parentPage[$index].ViewOrder = nextViewOrder;

        var sortedPageIds = [];
        _.forEach(_.sortBy(parentPage, ["ViewOrder"]), (p) =>
            sortedPageIds.push(p.Id)
        );

        const sortedPages = {
            DashboardId: this.dashboard.DashboardId,
            SortedPageIds: sortedPageIds,
        };

        this.sortPages(sortedPages);
    }

    onLeftPageClick(page) {
        const $page = $(`li[data-page-id="${page.Id}"]`);
        const parentId = $page.parent().data("page-id");
        const $parent = $(`li[data-page-id="${parentId}"]`);
        const parentPage = this.getPageByPageId(parentId);
        parentPage.Pages.splice(parentPage.Pages.indexOf(page), 1);
        const newParentId = $parent.parent().attr("data-page-id");
        const newParent = newParentId ?
            this.getPageByPageId(newParentId).Pages :
            this.pages;

        _.filter(newParent, (p) => {
            return p.ViewOrder > parentPage.ViewOrder;
        }).map((p) => p.ViewOrder++);

        page.ViewOrder = parentPage.ViewOrder + 1;
        page.IsChild = !!newParentId;
        newParent.push(page);

        var movedPage = {
            Id: page.Id,
            ParentId: newParentId,
            ViewOrder: page.ViewOrder,
        };
        var sortedPageIds = [];
        _.forEach(newParent, (p) => sortedPageIds.push(p.Id));

        const sortedPages = {
            DashboardId: this.dashboard.DashboardId,
            MovedPage: movedPage,
            SortedPageIds: sortedPageIds,
        };

        this.sortPages(sortedPages);
    }

    onRightPageClick(page) {
        const $page = $(`li[data-page-id="${page.Id}"]`);
        const parentId = $page.parent().data("page-id");
        const parentPage = parentId ?
            this.getPageByPageId(parentId).Pages :
            this.pages;
        parentPage.splice(parentPage.indexOf(page), 1);

        const newParentId = $page.prev().data("page-id");
        const newParent = this.getPageByPageId(newParentId);
        page.IsChild = true;
        newParent.Pages.push(page);

        var movedPage = {
            Id: page.Id,
            ParentId: newParentId,
            ViewOrder: newParent.Pages.length - 1,
        };

        const sortedPages = {
            DashboardId: this.dashboard.DashboardId,
            MovedPage: movedPage,
        };

        this.sortPages(sortedPages);
    }

    onPageSwapClick(index, swappedIndex) {
        const pages = this.dashboard.Pages;

        if (swappedIndex > -1 && swappedIndex < pages.length) {
            [pages[index], pages[swappedIndex]] = [pages[swappedIndex], pages[index]];

            pages.map((c) => (c.ViewOrder = this.page.Pages.indexOf(c) + 1));
        }

        var swapedPages = _.filter(pages, (c) => {
            return c.Id, c.ViewOrder != pages.indexOf(c) + 1;
        }).map((i) => _.pick(i, "Id", "ViewOrder"));

        this.running = "swap-pages";

        this.awaitAction = {
            title: "Swaping Pages",
            subtitle: "Just a moment for swap page pages...",
        };

        this.apiService.post("Module", "SortPages", swapedPages).then(() => {
            delete this.running;
        });
    }

    onDeletePageClick(page) {
        swal({
            title: "Are you sure from delete this page?",
            text: "Once deleted, you will not be able to recover this imaginary page!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "delete-dashboard-page";
                this.awaitAction = {
                    title: "Deleting Dashboard page",
                    subtitle: "Just a moment for deleting dashboard page...",
                };

                this.apiService.post("Module", "DeleteDashboardPage", page).then((data) => {
                    this.notifyService.success(
                        "Dashboard page deleted has been successfully"
                    );

                    this.pages = data;

                    this.$rootScope.refreshSidebarExplorerItems();

                    // const $page = $(`li[data-page-id="${pageId}"]`);
                    // const parentId = $page.parent().data("page-id");
                    // const parentPage = parentId ?
                    //     this.getPageByPageId(parentId).Pages :
                    //     this.pages;
                    // parentPage.splice($index, 1);

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

    sortPages(sortedPages) {
        this.apiService.post("Module", "SortDashboardPages", sortedPages).then(
            (data) => {
                this.notifyService.success("Sorted pages has been successfully");

                delete this.awaitAction;
                delete this.running;
            },
            (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            }
        );
    }

    isLastPage(page) {
        const $page = $(`li[data-page-id="${page.Id}"]`);
        const parentId = $page.parent().data("page-id");
        const parentPage = _.sortBy(
            parentId ? this.getPageByPageId(parentId).Pages : this.pages, ["ViewOrder"]
        );

        return parentPage.indexOf(page) == parentPage.length - 1 ? true : false;
    }

    populatePages(pages) {
        var allPages = [{ Id: null, LevelTitle: "None" }];

        _.forEach(pages, function (c) {
            c.ParentId = 0;
            runner(c, 0);
        });

        this.allPages = allPages;

        function runner(page, level) {
            var preTitle = "";
            for (var i = 0; i < level; i++) {
                preTitle += "...";
            }
            page.LevelTitle = preTitle + page.Title;
            page.Level = level;

            allPages.push(page);

            _.forEach(page.Pages, function (c) {
                c.ParentId = page.Id;
                runner(c, level + 1);
            });
        }
    }

    getPageByPageId(pageId) {
        var result;

        const findNestedPage = (pages) => {
            _.forEach(pages, (p) => {
                if (p.Id == pageId) result = p;
                else return findNestedPage(p.Pages);
            });
        };

        findNestedPage(this.pages);

        return result;
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    disposeWorkingMode() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }
}