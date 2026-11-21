import { GlobalUtil } from '../shared/utils';
import { UrlHelper, JsonHelper } from '../shared/helpers';
import { activityBarItems } from '../shared/configs/activity-bar.config';
import selectScenarioTemplate from './create-scenario/1-scenarios/select-scenario.html';
import createScenarioTemplate from './create-scenario/1-scenarios/create-scenario.html';
import entitiesTemplate from './create-scenario/3-entities/entities.html';
import createEntityTemplate from './create-scenario/3-entities/create-entity.html';
import appModelsTemplate from './create-scenario/4-app-models/app-models.html';
import createAppModelTemplate from './create-scenario/4-app-models/create-app-model.html';
import servicesTemplate from './create-scenario/5-services/services.html';
import createServiceTemplate from './create-scenario/5-services/create-service.html';
import createDashboardTemplate from './create-dashboard/create-dashboard.html';
import createModuleTemplate from './create-module/create-module.html';
import createActionTemplate from './create-module/6-actions/create-action.html';
import extensionsTemplate from './extensions/extensions.html';

export class StudioController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        $q,
        apiService,
        actionCenterService,
        sseClientService,
        notificationService,
        baseOptions
    ) {
        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.apiService = apiService;
        this.actionCenterService = actionCenterService;
        this.sseClientService = sseClientService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;
        this.selectScenarioTemplate = selectScenarioTemplate;
        this.actionCenterTasks = actionCenterService.tasks;

        $scope.$on('onGotoPage', (e, args) => {
            const subParamsUrl = new URLSearchParams(args.subParams).toString()
            this._createOrGotoTab(
                args.page,
                args.parentId,
                args.id,
                args.title,
                subParamsUrl,
                args.activityBar,
            );
        });

        $scope.$on('onUpdateCurrentTab', (e, args) => {
            if (!args) return;

            if ($rootScope.currentTab && (args.id ?? '').toLowerCase() === ($rootScope.currentTab.id ?? '').toLowerCase())
                this._updateCurrentTab(args.id, args.title);
            else if (args.key === $rootScope.currentTab.key)
                this._updateCurrentTab(args.id, args.title, args.newKey);
            else
                this._updateTabInfo(args.id, args.title, args.key);

            this._setTabsContentHeight();
        });

        $scope.$on('onChangeActivityBar', (e, args) => {
            if (args) this.onActivityBarItemClick(args.name, args.title, args.disableActivityBarCallback);
        });

        $scope.$on('onShowRightWidget', (e, args) => {
            if (args?.controller) args.controller.currentFieldFocused = false;

            $('html').addClass('overflow-hidden');

            this.$timeout(() => {
                $('.b-right-widget').addClass('visible');
            });
        });

        $scope.$on('onHideRightWidget', (e, args) => {
            $('html').removeClass('overflow-hidden');

            $('.b-right-widget').removeClass('visible');
        });

        $scope.$on('onUpdateExplorerItems', (e, args) => {
            this._getStudioOptions();
        });

        $scope.$on('onCloseModule', (e, args) => {
            this.onCloseTabClick($rootScope.currentTab.key)
        });

        $scope.$on('onCurrentTabChange', (e, args) => {
            $rootScope.currentTab.isChanged = true;
        });

        $scope.$on('onShowHideActionCenterWidget', (e, args) => {
            this.showActionCenterWidget = args.show;
        })

        $rootScope.activityBarItems = activityBarItems;
        $rootScope.explorerExpandedItems = [];

        this.onActivityBarItemClick('explorer');
        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        this.returnUrl = UrlHelper.getParameterByName('ru');

        const scenarioName = this.baseOptions.scenarioName;
        if (scenarioName)
            this.sseClientService.init(scenarioName);

        this._getStudioOptions().then((data) => {
            if (!scenarioName) {
                this.$timeout(() => window['wnSelectScenario'].show());
            }
            else {
                const currentTabs = JsonHelper.getJsonString(
                    sessionStorage.getItem('bEngineCurrentTabs_' + scenarioName) || '[]'
                );
                currentTabs.forEach(tab => tab.isLoaded = null);
                this.$rootScope.tabs = currentTabs;

                const moduleType = UrlHelper.getParameterByName('m');
                const parentId = UrlHelper.getParameterByName('parent');
                const id = UrlHelper.getParameterByName('id');
                const newKey = UrlHelper.getParameterByName('key');
                const subParams = this._getSubParams();

                if (moduleType)
                    this._createOrGotoTab(moduleType, parentId, id, '', subParams, null, newKey);
                else if (!moduleType && this.$rootScope.tabs.length)
                    this.onTabClick(this.$rootScope.tabs[0]);
            }
        });
    }

    onTabClick(tab) {
        if (this.$rootScope.currentTab !== tab)
            this._createOrGotoTab(
                tab.moduleType,
                tab.parentId,
                tab.id,
                tab.title,
                tab.subParamsUrl
            );
    }

    onCloseTabClick($event, tabKey) {
        const index = this.$rootScope.tabs.findIndex(t => t.key === tabKey);
        const currentTaIndex = this.$rootScope.tabs.findIndex(t => t.key === this.$rootScope.currentTab?.key);
        const closedCurrentTab = currentTaIndex === index;

        this.$rootScope.tabs.splice(index, 1);

        if (closedCurrentTab && this.$rootScope.tabs.length && index > 0)
            this.onTabClick(this.$rootScope.tabs[index - 1]);
        else if (closedCurrentTab && this.$rootScope.tabs.length && index === 0)
            this.onTabClick(this.$rootScope.tabs[0]);

        if (!this.$rootScope.tabs.length)
            this.onActivityBarItemClick('explorer');

        sessionStorage.setItem(
            'bEngineCurrentTabs_' + this.baseOptions.scenarioName,
            JSON.stringify(this.$rootScope.tabs)
        );

        this._setTabsContentHeight();

        if ($event) $event.stopPropagation();
    }

    onCloseAllTabsClick() {
        sessionStorage.removeItem(
            'bEngineCurrentTabs_' + this.baseOptions.scenarioName
        );

        this.$rootScope.tabs = [];
        this.$rootScope.currentTab = null;
    }

    onActivityBarItemClick(name, title, disableActivityBarCallback) {
        if (this.$rootScope.currentActivityBar === name) return;

        const item = this.$rootScope.activityBarItems.find(i => i.name === name);
        if (item) {
            if (!item.sidebarPaneDisabled) this.$rootScope.currentActivityBar = name;
            if (title) item.title = title;
            if (item.callback && !disableActivityBarCallback) this[item.callback].apply(this, item);
        };
    }

    onGotoExtensions() {
        this.$scope.$emit('onGotoPage', {
            page: 'extensions',
            activityBar: 'extensions'
        });
    }

    onShowSettingsWidget() {
        this.workingMode = 'global-settings';
        this.$scope.$emit('onShowRightWidget');
    }

    onChangeStudioSizeClick(size) {
        if (this.studioSize) $('html').removeClass('b-size-' + this.studioSize);

        $('html').addClass('b-size-' + size);
        this.studioSize = size;
    }

    onClearChaceClick() {
        this.running = 'clear-cache';
        this.awaitAction = {
            title: 'Clear Cache',
            subtitle: 'Just a moment for clear cache and add host version...',
        };

        this.apiService.post('Studio', 'ClearCacheAndAddCmsVersion').then((data) => {
            this.notifyService.info('Clear cache and increasing host version has been successfully.');

            this.awaitAction = null;
            this.running = null;

            if (confirm('Do you want to refresh the page?')) location.reload();
        }, (error) => {
            $defer.reject(error);

            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onHideActionCenterWidgetClick() {
        this.showActionCenterWidget = false;
    }

    onCloseSettingsWidgetClick() {
        this.$scope.$emit('onHideRightWidget');
        this.$timeout(() => this.workingMode = null, 200);
    }

    //#endregion

    //#region Private Methods

    _getStudioOptions() {
        const defer = this.$q.defer();

        this.running = 'get-studio-options';
        this.awaitAction = {
            title: 'Loading Studio Options',
            subtitle: 'Just a moment for loading studio options...',
        };

        const scenarioId = this.baseOptions.scenarioId;
        this.apiService.get('Studio', 'GetStudioOptions', { scenarioId }).then((data) => {
            this.$rootScope.scenarios = data?.Scenarios;
            this.$rootScope.scenario = data?.Scenario;
            this.$rootScope.groups = data?.Groups;
            this.$rootScope.explorerItems = data?.ExplorerItems;
            this.$rootScope.roles = data.Roles;

            this.running = null;
            this.awaitAction = null;

            defer.resolve(data ?? {});
        });

        return defer.promise;
    }

    _createOrGotoTab(moduleType, parentId, id, title, subParamsUrl, activityBar, newKey) {
        let tab = this.$rootScope.tabs.find(t => (newKey && t.newKey === newKey) ||
            (t.moduleType === moduleType && t.parentId == parentId && t.id == id));
        if (tab) {
            tab.subParamsUrl = subParamsUrl || tab.subParamsUrl;
            this.$rootScope.currentTab = tab;
        } else {
            const module = this._getModuleContent(moduleType);
            if (!module.content) {
                this.onTabClick(this.$rootScope.tabs[0]);
                return;
            }

            tab = {
                key: this._getTabKey(),
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
            sessionStorage.setItem('bEngineCurrentTabs_' + this.baseOptions.scenarioName, JSON.stringify(this.$rootScope.tabs));

            this.$rootScope.currentTab = tab;
        }

        let url = this._getBaseUrl(tab.moduleType, id);
        if (tab.parentId) url += '&parent=' + tab.parentId;
        if (tab.id) url += '&id=' + tab.id;
        if (tab.subParamsUrl) url += '&' + tab.subParamsUrl;
        UrlHelper.pushState(url);

        this.$rootScope.currentTab.isLoaded = true;
        this.onActivityBarItemClick(activityBar ? activityBar : 'explorer', '', false, subParamsUrl);
        this.$rootScope.$broadcast(`onTab--${tab.key}--Selected`);

        const $elem = $(`#bExplorer [data-items*='${tab.moduleType}']`);
        if ($elem.length) {
            $elem.addClass("show");
            $elem.prev().removeClass("collapsed");
        }

        this._setTabsContentHeight();
    }

    _getTabKey() {
        const tabs = this.$rootScope.tabs;

        while (true) {
            const key = `tab--${Math.floor(Math.random() * 1000)}`;
            if (!tabs.some(t => t.key === key)) {
                return key;
            }
        }
    }

    _updateCurrentTab(id, title, newKey) {
        if (this.$rootScope.currentTab) {
            const tab = this.$rootScope.tabs.find(t => t.key === this.$rootScope.currentTab?.key);
            Object.assign(tab, { id, title, isNewItem: false });

            if (newKey) tab.newKey = newKey;

            sessionStorage.setItem('bEngineCurrentTabs_' + this.baseOptions.scenarioName,
                JSON.stringify(this.$rootScope.tabs)
            );

            const url = UrlHelper.replaceUrlParam('id', tab.id);
            UrlHelper.pushState(url);
        }
    }

    _updateTabInfo(id, title, key) {
        const tab = this.$rootScope.tabs.find(t => t.key === key);
        if (tab) {
            Object.assign(tab, { id, title, isNewItem: false });
        }
    }

    _getModuleContent(moduleType) {
        const result = {};

        switch (moduleType) {
            case 'create-scenario':
                result.title = 'New Scenario';
                result.icon = 'archive';
                result.content = createScenarioTemplate;
                break;
            case 'entities':
                result.title = 'Entities';
                result.icon = 'combine';
                result.content = entitiesTemplate;
                break;
            case 'create-entity':
                result.title = 'New Entity';
                result.icon = 'table';
                result.content = createEntityTemplate;
                break;
            case 'app-models':
                result.title = 'App Models';
                result.icon = 'references';
                result.content = appModelsTemplate;
                break;
            case 'create-app-model':
                result.title = 'New App Model';
                result.icon = 'table';
                result.content = createAppModelTemplate;
                break;
            case 'services':
                result.title = 'Services';
                result.icon = 'versions';
                result.content = servicesTemplate;
                break;
            case 'create-service':
                result.title = 'New Service';
                result.icon = 'table';
                result.content = createServiceTemplate;
                break;
            case 'create-dashboard':
                result.title = 'New Dashboard';
                result.icon = 'layout-sidebar-left';
                result.content = createDashboardTemplate;
                break;
            case 'create-dashboard-page':
                result.title = 'Edit Page';
                result.icon = 'copy';
                result.content = createDashboardPageTemplate;
                break;
            case 'create-module':
                result.title = 'Create Module';
                result.content = createModuleTemplate;
                result.icon = 'window';
                break;
            case 'create-action':
                result.title = 'New Action';
                result.icon = 'symbol-event';
                result.content = createActionTemplate;
                break;
            case 'extensions':
                result.title = 'Extensions';
                result.icon = 'extensions';
                result.content = extensionsTemplate;
                break;
        }

        return result;
    }

    _setTabsContentHeight() {
        setTimeout(() => {
            $('#workspaceTabsContent').css('margin-top', $('#workspaceTabs').height().toString() + 'px');
        });
    }

    _getSubParams() {
        const result = [];
        const params = UrlHelper.getUrlParams(document.URL);
        for (const param in params) {
            if (
                param === 'dashboard' ||
                param === 'module' ||
                param === 'field' ||
                param === 'type' ||
                param === 'mode' ||
                param === 'key' ||
                param === 'st' ||
                param === 'mt' ||
                param === 'd' ||
                param === 'ru'
            ) {
                const paramValue = UrlHelper.getParameterByName(param);
                if (paramValue) result.push(`${param}=${paramValue}`);
            }
        }

        return result.join('&');
    }

    _getBaseUrl(moduleType, id) {
        let baseUrl = this.baseOptions.baseUrl + '/?s={s}&{sr}&m={m}';
        baseUrl = baseUrl.replace('{s}', this.baseOptions.scenarioName);
        baseUrl = baseUrl.replace('{m}', moduleType);
        baseUrl = baseUrl.replace('{sr}', this.baseOptions.siteRoot === '/'
            ? ''
            : 'sr=' + this.baseOptions.siteRoot
        );

        return baseUrl;
    }

    //#endregion
}