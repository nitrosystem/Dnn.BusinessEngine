export class StudioService {
    constructor($rootScope, globalService) {
        "ngInject";

        this.$rootScope = $rootScope;
        this.globalService = globalService;
    }

    getCurrentTab() {
        const moduleType = this.globalService.getParameterByName("m");
        const parentId = this.globalService.getParameterByName("parent");
        const id = this.globalService.getParameterByName("id");
        const newKey = this.globalService.getParameterByName("key");

        return this.getTab(moduleType, parentId, id, newKey);
    }

    getTab(moduleType, parentId, id, newKey) {
        const tab = _.find(this.$rootScope.tabs, (tab) => {
            return (
                (newKey && newKey == tab.key) || (tab.moduleType == moduleType && tab.parentId == parentId && tab.id == id)
            );
        });

        return tab;
    }

    setFocusModuleDelegate(controller, callback) {
        const tab = this.getCurrentTab();
        this.$rootScope.$on(`onTab--${tab.key}Selected`, (e, args) => {
            callback.apply(controller);

            const $elem = $(`#bExplorer [data-items*='${tab.moduleType}']`);
            if ($elem.length) {
                $elem.addClass("show");
                $elem.prev().removeClass("collapsed");
            }
        });
    }

    createSidebarExplorerPath(itemId, type) {
        var result = [itemId];
        const findPages = (id) => {
            const item = _.find(this.$rootScope.explorerItems, (i) => {
                return i.ItemId == id;
            });
            if (item) {
                result.push(item.ItemId);

                if (item.ParentId && !item.DashboardPageParentId) {
                    const dashboard = _.find(this.$rootScope.explorerItems, (i) => {
                        return i.Type == "Dashboard" && i.ParentId == item.ParentId;
                    });
                    result.push(dashboard.ItemId);
                    result.push("dashboards");
                    if (type == "Page" || type == "Module")
                        result.push("pages-" + dashboard.ItemId);
                }

                findPages(item.DashboardPageParentId);
            }
        };

        _.filter(this.$rootScope.explorerItems, (i) => {
            return i.ItemId == itemId;
        }).map((item) => {
            if (!item.ParentId) {
                result.push(itemId);
            } else {
                findPages(itemId);
            }
        });

        console.log(result);
        return result;
    }
}