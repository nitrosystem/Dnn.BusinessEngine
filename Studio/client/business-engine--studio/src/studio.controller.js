import { GlobalSettings } from "./angular/angular-configs/global.settings";
import { activityBarItems } from "./angular/angular-configs/activity-bar.config";

import createScenarioTemplate from "./create-scenario/scenarios/create-scenario.html";
import entitiesTemplate from "./create-scenario/entities/entities.html";
import createEntityTemplate from "./create-scenario/entities/create-entity.html";
import appModelsTemplate from "./create-scenario/app-models/app-models.html";
import createAppModelTemplate from "./create-scenario/app-models/create-app-model.html";
import servicesTemplate from "./create-scenario/services/services.html";
import createServiceTemplate from "./create-scenario/services/create-service.html";
import createDashboardTemplate from "./create-dashboard/create-dashboard.html";
import createModuleTemplate from "./create-module/create-module.html";
import createActionTemplate from "./create-module/6-actions/create-action.html";
import extensionsTemplate from "./extensions/extensions.html";

export class StudioController {
    constructor($scope, $rootScope, $timeout, $q, $compile, globalService, apiService, eventService, notificationService) {
        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.$compile = $compile;
        this.globalService = globalService;
        this.apiService = apiService;
        this.eventService = eventService;
        this.notifyService = notificationService;

        $scope.$on("onGotoPage", (e, args) => {
            const subParamsUrl = args.subParams ? this.globalService.getUrlQueryFromObject(args.subParams) : "";

            this.createOrGotoTab(
                args.page,
                args.parentId,
                args.id,
                args.title,
                subParamsUrl,
                args.activityBar,
            );
        });

        $scope.$on("onUnauthorized401", (e, args) => {
            location.reload();
        });

        $scope.$on("onUpdateCurrentTab", (e, args) => {
            if (args && this.$rootScope.currentTab && (args.id ?? '').toLowerCase() == (this.$rootScope.currentTab.id ?? '').toLowerCase())
                this.updateCurrentTab(args.id, args.title);
            else if (args && args.key == this.$rootScope.currentTab.key)
                this.updateCurrentTab(args.id, args.title, args.newKey);
            else
                this.updateTabInfo(args.id, args.title, args.key);

            this.setTabsContentHeight();
        });

        $scope.$on("onChangeActivityBar", (e, args) => {
            if (args) this.onActivityBarItemClick(args.name, args.title, args.disableActivityBarCallback);
        });

        $scope.$on("onShowRightWidget", (e, args) => {
            if (args && args.controller) args.controller.currentFieldFocused = false;

            $("body").addClass("overflow-hidden");

            this.$timeout(() => {
                $(".b-right-widget").addClass("visible");
            });
        });

        $scope.$on("onHideRightWidget", (e, args) => {
            $("body").removeClass("overflow-hidden");

            $(".b-right-widget").removeClass("visible");
        });

        $scope.$on("onUpdateExplorerItems", (e, args) => {
            this.onGetStudioOptions();
        });

        $scope.$on("onCloseModule", (e, args) => {
            this.onCloseTabClick(this.$rootScope.currentTab.key)
        });

        $scope.$on('onCurrentTabChange', (e, args) => {
            this.$rootScope.currentTab.isChanged = true;
        });

        this.$rootScope.activityBarItems = activityBarItems;
        this.onActivityBarItemClick("explorer");
        this.$rootScope.explorerExpandedItems = [];

        this.onPageLoad();
    }

    onPageLoad() {
        this.returnUrl = this.globalService.getParameterByName("ru");

        this.onGetStudioOptions().then((data) => {
            if (!GlobalSettings.scenarioName) {
                if (this.$rootScope.scenarios.length)
                    this.$timeout(() => window["wnSelectScenario"].show());
                else {
                    this.$rootScope.tabs = [];
                    this.createOrGotoTab("create-scenario");
                }
            }
            else {
                const currentTabs = this.globalService.getJsonString(
                    sessionStorage.getItem("bEngineCurrentTabs_" + GlobalSettings.scenarioName) || "[]"
                );
                currentTabs.forEach((tab) => delete tab.isLoaded);
                this.$rootScope.tabs = currentTabs;

                const moduleType = this.globalService.getParameterByName("m");
                const parentId = this.globalService.getParameterByName("parent");
                const id = this.globalService.getParameterByName("id");
                const newKey = this.globalService.getParameterByName("key");
                const subParams = this.getSubParams();

                if (moduleType)
                    this.createOrGotoTab(moduleType, parentId, id, "", subParams, null, newKey);
                else if (!moduleType && this.$rootScope.tabs.length)
                    this.onTabClick(this.$rootScope.tabs[0]);
            }
        });
    }

    onGetStudioOptions() {
        const defer = this.$q.defer();

        this.running = "get-studio-options";
        this.awaitAction = {
            title: "Loading Studio Options",
            subtitle: "Just a moment for loading studio options...",
        };

        this.apiService.get("Studio", "GetStudioOptions").then((data) => {
            this.$rootScope.scenarios = data.Scenarios;
            this.$rootScope.scenario = data.Scenario;
            this.$rootScope.roles = data.Roles;
            this.$rootScope.groups = data.Groups || [];
            this.$rootScope.explorerItems = data.ExplorerItems;

            delete this.running;
            delete this.awaitAction;

            defer.resolve(data);
        });

        return defer.promise;
    }

    createOrGotoTab(moduleType, parentId, id, title, subParamsUrl, activityBar, newKey) {
        var tab = _.find(this.$rootScope.tabs, (tab) => {
            return (
                (newKey && tab.newKey == newKey) || (tab.moduleType == moduleType && tab.parentId == parentId && tab.id == id)
            );
        });

        if (tab) {
            tab.subParamsUrl = subParamsUrl || tab.subParamsUrl;

            this.$rootScope.currentTab = tab;
        } else {
            const module = this.getModuleContent(moduleType);

            if (!module.content) {
                this.onTabClick(this.$rootScope.tabs[0]);
                return;
            }

            tab = {
                key: this.getTabKey(),
                moduleType: moduleType,
                parentId: parentId,
                id: id,
                title: title ? title : module.title,
                icon: module.icon,
                content: module.content,
                subParamsUrl: subParamsUrl,
                isLoaded: true,
                isNewItem: !id,
            };

            this.$rootScope.tabs.push(tab);
            sessionStorage.setItem("bEngineCurrentTabs_" + GlobalSettings.scenarioName, JSON.stringify(this.$rootScope.tabs));

            this.$rootScope.currentTab = tab;
        }

        let url = this.getBaseUrl(tab.moduleType, id);
        if (tab.parentId) url += "&parent=" + tab.parentId;
        if (tab.id) url += "&id=" + tab.id;
        if (tab.subParamsUrl) url += "&" + tab.subParamsUrl;
        this.globalService.pushState(url);

        this.$rootScope.currentTab.isLoaded = true;
        this.onActivityBarItemClick(activityBar ? activityBar : "explorer", '', this.globalService.getParameterByName('disableActivityBarCallback', subParamsUrl));
        this.$rootScope.$broadcast(`onTab--${tab.key}Selected`);

        this.setTabsContentHeight();
    }

    getTabKey() {
        while (true) {
            const key = "tab--" + Math.floor(Math.random() * 1000).toString();
            if (_.filter(this.$rootScope.tabs, (t) => t.key == key).length == 0)
                return key;
        }
    }

    updateCurrentTab(id, title, newKey) {
        if (this.$rootScope.currentTab) {
            var tab = this.$rootScope.tabs[this.$rootScope.tabs.indexOf(this.$rootScope.currentTab)];
            tab.id = id;
            tab.title = title;
            tab.isNewItem = false;

            if (newKey) tab.newKey = newKey;

            sessionStorage.setItem("bEngineCurrentTabs_" + GlobalSettings.scenarioName,
                JSON.stringify(this.$rootScope.tabs)
            );

            const url = this.globalService.replaceUrlParam("id", tab.id);
            this.globalService.pushState(url);
        }
    }

    updateTabInfo(id, title, key) {
        _.filter(this.$rootScope.tabs, (t) => { return t.key == key }).map((tab) => {
            tab.id = id;
            tab.title = title;
            tab.isNewItem = false;
        });
    }

    onTabClick(tab) {
        if (this.$rootScope.currentTab != tab)
            this.createOrGotoTab(
                tab.moduleType,
                tab.parentId,
                tab.id,
                tab.title,
                tab.subParamsUrl
            );
    }

    onCloseTabClick(tabKey, $event) {
        const tab = _.find(this.$rootScope.tabs, (t) => {
            return t.key == tabKey;
        });
        const $index = this.$rootScope.tabs.indexOf(tab);
        const closedCurrentTab =
            this.$rootScope.tabs.indexOf(this.$rootScope.currentTab) == $index;

        this.$rootScope.tabs.splice($index, 1);

        if (closedCurrentTab && this.$rootScope.tabs.length && $index > 0)
            this.onTabClick(this.$rootScope.tabs[$index - 1]);
        else if (closedCurrentTab && this.$rootScope.tabs.length && $index == 0)
            this.onTabClick(this.$rootScope.tabs[0]);

        if (!this.$rootScope.tabs.length)
            this.onActivityBarItemClick("explorer");

        sessionStorage.setItem(
            "bEngineCurrentTabs_" + GlobalSettings.scenarioName,
            JSON.stringify(this.$rootScope.tabs)
        );

        this.setTabsContentHeight();

        if ($event) $event.stopPropagation();
    }

    onCloseAllTabsClick() {
        sessionStorage.removeItem(
            "bEngineCurrentTabs_" + GlobalSettings.scenarioName
        );

        this.$rootScope.tabs = [];
        delete this.$rootScope.currentTab;
    }

    onActivityBarItemClick(name, title, disableActivityBarCallback) {
        if (this.$rootScope.currentActivityBar == name) return;

        _.filter(this.$rootScope.activityBarItems, (i) => { return i.name == name; }).map((i) => {
            if (!i.sidebarPaneDisabled) this.$rootScope.currentActivityBar = name;

            if (title) i.title = title;

            if (i.callback && !disableActivityBarCallback) this[i.callback].apply(this, i);
        });
    }

    onGotoExtensions() {
        this.$scope.$emit("onGotoPage", { page: "extensions", activityBar: "extensions" });
    }

    onGotoPageResources() {
        this.$scope.$emit("onGotoPage", { page: "page-resources", activityBar: "page-resources" });
    }

    onGotoLibraries() {
        this.$scope.$emit("onGotoPage", { page: "libraries", activityBar: "libraries" });
    }

    onSelectScenarioClick() {
        _.filter(this.$rootScope.scenarios, (s) => {
            return s.Id == this.scenarioId;
        }).map((s) => {
            const url = this.globalService.replaceUrlParam("s", s.ScenarioName);
            this.globalService.pushState(url);

            this.$timeout(() => location.reload());
        });
    }

    onAddScenarioClick() {
        this.$rootScope.tabs = this.$rootScope.tabs || [];

        window["wnSelectScenario"].hide();

        this.createOrGotoTab("create-scenario");
    }

    setTabsContentHeight() {
        setTimeout(() => {
            $('#workspaceTabsContent').css('margin-top', $('#workspaceTabs').height().toString() + 'px');
        });
    }

    getBaseUrl(moduleType, id) {
        var baseUrl = "/DesktopModules/BusinessEngine/Studio.aspx?s={s}&sr={sr}&m={m}";
        baseUrl = baseUrl.replace("{s}", GlobalSettings.scenarioName);
        baseUrl = baseUrl.replace("{sr}", GlobalSettings.siteRoot);
        baseUrl = baseUrl.replace("{m}", moduleType);

        return baseUrl;
    }

    getSubParams() {
        var result = [];

        const params = this.globalService.getUrlParams(document.URL, true);
        for (const param in params) {
            if (
                param == "dashboard" ||
                param == "module" ||
                param == "field" ||
                param == "type" ||
                param == "mode" ||
                param == "key" ||
                param == "st" ||
                param == "mt" ||
                param == "d" ||
                param == "ru"
            ) {
                const paramValue = this.globalService.getParameterByName(param);
                if (paramValue) result.push(`${param}=${paramValue}`);
            }
        }

        return result.join("&");
    }

    getModuleContent(moduleType) {
        var result = {};

        switch (moduleType) {
            case "create-scenario":
                result.title = "New Scenario";
                result.icon = "archive";
                result.content = createScenarioTemplate;
                break;
            case "entities":
                result.title = "Entities";
                result.icon = "combine";
                result.content = entitiesTemplate;
                break;
            case "create-entity":
                result.title = "New Entity";
                result.icon = "table";
                result.content = createEntityTemplate;
                break;
            case "app-models":
                result.title = "App Models";
                result.icon = "references";
                result.content = appModelsTemplate;
                break;
            case "create-app-model":
                result.title = "New App Model";
                result.icon = "table";
                result.content = createAppModelTemplate;
                break;
            case "services":
                result.title = "Services";
                result.icon = "versions";
                result.content = servicesTemplate;
                break;
            case "create-service":
                result.title = "New Service";
                result.icon = "table";
                result.content = createServiceTemplate;
                break;
            case "create-dashboard":
                result.title = "New Dashboard";
                result.icon = "layout-sidebar-left";
                result.content = createDashboardTemplate;
                break;
            case "create-dashboard-page":
                result.title = "Edit Page";
                result.icon = "copy";
                result.content = createDashboardPageTemplate;
                break;
            case "create-module":
                result.title = "Create Module";
                result.content = createModuleTemplate;
                result.icon = "window";
                break;
            case "create-action":
                result.title = "New Action";
                result.icon = "symbol-event";
                result.content = createActionTemplate;
                break;
            case "extensions":
                result.title = "Extensions";
                result.icon = "extensions";
                result.content = extensionsTemplate;
                break;
        }

        return result;
    }

    onClearChaceClick() {
        this.running = "clear-cache";
        this.awaitAction = {
            title: "Clear Cache",
            subtitle: "Just a moment for clear cache and add host version...",
        };

        this.apiService.post("Studio", "ClearCacheAndAddCmsVersion").then((data) => {
            this.notifyService.success("Dnn cache and increasing host version has been successfully");

            delete this.awaitAction;
            delete this.running;

            if (confirm("Do you want to refresh the page?")) location.reload();
        }, (error) => {
            $defer.reject(error);

            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }
}