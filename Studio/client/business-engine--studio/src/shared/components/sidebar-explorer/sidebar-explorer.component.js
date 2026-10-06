import { GlobalHelper, UrlHelper } from '../../helpers/index.js';
import template from './sidebar-explorer.component.html';
import editGroup from '../../../pages/create-scenario/2-groups/create-group.html'

class SidebarExplorerController {
    constructor($scope, $rootScope, validationService, apiService, notificationService, baseOptions) {
        'ngInject';

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.validationService = validationService;
        this.apiService = apiService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        this.editGroup = editGroup;

        $rootScope.$on('onGroupCreated', (e, args) => {
            window['wnEditGroup'].hide();
            const group = args.group;
            if (!group) return;

            const index = $rootScope.groups.findIndex(g => g.Id === group.Id);
            if (index === -1)
                $rootScope.groups.push(group);
            else {
                $rootScope.groups[index] = group;
            }

            $(`#exp${group.GroupType}Items`).addClass('show');
            setTimeout(() => {
                const $ul = $(`#expGroupItems_${group.Id}`);
                if ($ul.length) {
                    $ul.addClass('show');
                    $('#bExplorer').animate({ scrollTop: $ul.offset().top - 50 }, 1000);
                }
            });
        });

        $rootScope.$on('onGroupIsDeleted', (e, args) => {
            window['wnEditGroup'].hide();

            const index = $rootScope.groups.findIndex(g => g.Id === args?.groupId);
            if (index >= 0) $rootScope.groups.splice(index, 1);
        });

        $rootScope.$watch('explorerItems', (newVal, oldVal) => {
            if (newVal !== oldVal) this._setExplorerItems();
        }, true);

        $rootScope.$watch('explorerCurrentItem', (newVal, oldVal) => {
            setTimeout(() => {
                const explorerElement = document.getElementById(newVal);
                if (newVal !== oldVal && explorerElement) {
                    const rect = explorerElement.getBoundingClientRect();
                    $('#bExplorer').animate({ scrollTop: rect.top + $('#bExplorer').scrollTop() - 100 }, 600);
                }
            });
        }, true);

        $rootScope.$watch('groups', (newVal, oldVal) => {
            this.groups = this.$rootScope.groups;
        }, true);

        $rootScope.refreshSidebarExplorerItems = () => {
            this._refreshExplorerItems();
        }
    }

    //#region Event Methods

    $onInit() {
        this._setExplorerItems();
    }

    onItemClick($event, moduleType, parentId, itemId, title, subParams) {
        if (this.ignoreItemClick) return;

        moduleType = this._modifyModuleType(moduleType);
        this.$scope.$emit('onGotoPage', {
            page: moduleType,
            parentId: parentId,
            id: itemId,
            title: title,
            subParams: subParams,
        });

        if ($event) $event.stopPropagation();
    }

    onRefreshItemsClick() {
        this.$rootScope.refreshSidebarExplorerItems();
    }

    onCollapseAllItemsClick() {
        $('.list-explorer>ul ul.collapse').removeClass('show');
        $('.list-explorer>ul li.exp-item > a.exp-label').addClass('collapsed');
    }

    //#endregion

    //#region  Public Methods

    isExpanded(...items) {
        return items.some(i => this.$rootScope.explorerExpandedItems.includes(i));
    }

    //#endregion

    //#region Group Methods

    isExpandedGroup(group) {
        if (group.Items?.some(i => i.ItemId === this.$rootScope.currentTab?.id))
            return true;
        else
            return false;
    }

    onAddGroupClick() {
        const group = {
            ScenarioId: this.baseOptions.scenarioId,
            GroupDomain: 'SidebarExplorer'
        };
        this.$scope.$emit('onEditGroup', { group: group })
        window['wnEditGroup'].show();
    }

    onEditGroupClick(group) {
        const clonedGroup = GlobalHelper.clone(group);
        this.$scope.$emit('onEditGroup', { group: clonedGroup });
        window['wnEditGroup'].show();
    }

    onDeleteGroupClick(groupId) {
        this.$scope.$emit('onDeleteGroup', { groupId });
    }

    //#endregion

    //#region Private Methods

    _processGroups() {
        this.$rootScope.groups.forEach(group => {
            group.Items = [];
            group.Items.push(
                ...(this.$rootScope.explorerItems
                    .filter(e => e.Type === group.GroupType && e.GroupId === group.Id))
            );
        });
    }

    _setExplorerItems() {
        const items = this.$rootScope?.explorerItems ?? [];
        const {
            Entity = [],
            AppModel = [],
            Service = [],
            Dashboard = [],
            Module = [] } = items.reduce((acc, item) => {
                acc[item.Type] = acc[item.Type] ?? [];
                acc[item.Type].push(item);
                return acc;
            }, {});

        this.entities = Entity;
        this.appModels = AppModel;
        this.services = Service;
        this.dashboards = Dashboard;
        this.modules = Module.sort((a, b) => a.Title.localeCompare(b.Title));

        this._processGroups();

        const mode = UrlHelper.getParameterByName('m');
        const id = UrlHelper.getParameterByName('id');
        const hasValidId = id != null && !isNaN(id);

        this.newModule = mode === 'create-module' && hasValidId;
        this.newDashboard = mode === 'create-dashboard' && hasValidId;
    }

    _refreshExplorerItems() {
        this.running = 'refresh-sidebar-explorer-items';
        this.awaitAction = {
            title: 'Refresh Sidebar Explorer Items',
            subtitle: 'Just a moment for refresh sidebar explorer items...',
        };

        this.apiService.get('Studio', 'RefreshSidebarExplorerItems', { scenarioId: this.baseOptions.scenarioId })
            .then((data) => {
                this.$rootScope.explorerItems = data.ExplorerItems;
                this.$rootScope.groups = data.Groups;
                this._setExplorerItems();

                this.running = null;
                this.awaitAction = null;
            });
    }

    _modifyModuleType(moduleType) {
        switch (moduleType) {
            case 'Entity':
                return 'create-entity';
            case 'AppModel':
                return 'create-app-model';
            case 'Service':
                return 'create-service';
            default:
                return moduleType;
        }
    }

    //#endregion
}

const SidebarExplorerComponent = {
    bindings: {
        tabs: '=',
        currentItem: '=',
        currentItemId: '=',
    },
    controller: SidebarExplorerController,
    controllerAs: '$',
    templateUrl: template,
};

export default SidebarExplorerComponent;