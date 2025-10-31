import moduleBuilderTemplate from "../../create-module/5-module-builder/module-builder.html";

export class CreateDashboardModulesController {
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

        this.moduleBuilderTemplate = moduleBuilderTemplate;

        this.$rootScope.createDashboardValidatedStep.push(4);

        $scope.$on("onCreateDashboardValidateStep3", (e, task, args) => {
            task.wait(() => {
                const $defer = this.$q.defer();

                $defer.resolve(true);

                return $defer.promise;
            });
        });

        _.filter(this.$rootScope.activityBarItems, (i) => { return i.name == 'dashboard-modules' }).map((item) => {
            item.visible = true;
            this.$rootScope.currentActivityBar = "dashboard-modules";
        });

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");
        const parent = this.globalService.getParameterByName("parent");
        this.moduleId = parent || id;

        /*-----------------------------------------------------------------------
            keydown detect
        -------------------------------------------------------------------*/
        document.addEventListener('keydown', ($event) => {
            //Esc -- hide sidebar menu - dashboard modules
            if (($event.key == 'Escape') && $(`#sidebarMenu${this.moduleId}`).hasClass('show-menu')) {
                this.onHideSidebarMenuClick()

                $event.preventDefault();
            }
        });

        this.running = "get-modules";
        this.awaitAction = {
            title: "Loading Modules",
            subtitle: "Just a moment for loading dashboard modules...",
        };

        this.apiService.get("Module", "GetDashboardModules", { moduleId: this.moduleId || null }).then((data) => {
            this.modules = data;

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

    onPreviousStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 4 });
    }

    onDashboardModuleClick() {
        if (this.modules.length) {
            let module = { ModuleBuilderType: 0, ModuleId: this.modules[0].ParentId }
            this.onModuleClick(module);
        }
    }

    onModuleClick(dashboardPagemodule) {
        const newKey = 'dashboard--' + dashboardPagemodule.ModuleName;

        var newUrl = this.globalService.replaceUrlParam("id", dashboardPagemodule.ModuleId);
        newUrl = this.globalService.replaceUrlParam("key", newKey, newUrl);
        if (dashboardPagemodule.ParentId) newUrl = this.globalService.replaceUrlParam("parent", dashboardPagemodule.ParentId, newUrl);
        this.globalService.pushState(newUrl);

        this.currentTabKey = this.$rootScope.currentTab.key;
        this.$scope.$emit("onUpdateCurrentTab", {
            id: dashboardPagemodule.ModuleId,
            title: 'dashboard--' + dashboardPagemodule.ModuleName,
            key: this.currentTabKey,
            newKey: newKey
        });

        this.$scope.$broadcast('onReloadModuleBuilder', { type: module.ModuleBuilderType, id: dashboardPagemodule.ModuleId });
    }

    onHideSidebarMenuClick() {
        $(`#sidebarMenu${this.moduleId}`).removeClass('show-menu');
    }
}