export class DashboardController {
    constructor($scope, $rootScope, $timeout, $compile, globalService, apiService) {
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$compile = $compile;
        this.globalService = globalService;
        this.apiService = apiService;

        this.$scope.$rootScope = $rootScope;
        this.$scope.$rootScope.isLoadingDashboardModule = true;
        this.$scope.userId = window.bEngineGlobalSettings.userId;
        this.$scope.userDisplayName = window.bEngineGlobalSettings.userDisplayName;
        this.$scope.userPhoto = `/DnnImageHandler.ashx?mode=profilepic&userId=${window.bEngineGlobalSettings.userId}&h=48&w=48`;
        //this.$scope.userRoles = window.bEngineGlobalSettings.userRoles;

        $scope.$on("onGotoDashboardPage", (e, args) => {
            if (args.pageName && !args.pageId) {
                const page = this.getPageByPageName(args.pageName);
                args.pageId = (page || {}).PageId;
            }

            this.gotoDashboardPage(args.pageId, args.params, args.isUpdatePageParams);
        });

        window.addEventListener("popstate", (event) => {
            var pageName = this.globalService.getParameterByName("page");
            if (pageName) {
                const params = this.globalService.getUrlParams(document.URL, true);
                var paramList = [];
                for (const param in params) {
                    if (param == "d" || param == "page") continue;

                    const paramValue = this.globalService.getParameterByName(param);
                    if (paramValue) paramList.push(`${param}=${paramValue}`);
                }

                const urlParams = paramList.join("&");
                const page = this.getPageByPageName(pageName);
                this.gotoDashboardPage(page.PageId, urlParams, false);
            }
        }, false);
    }

    onInitModule(moduleId, moduleName, connectionID, now) {
        this.module = { moduleId: moduleId, moduleName: moduleName };
        this.$scope.connectionID = connectionID;
        this.dateNow = this.$scope.dateNow = now;

        this.onPageLoad();
    }

    onPageLoad() {
        this.apiHeader = { ModuleId: this.module.moduleId };

        this.apiService.get("Module", "GetDashboardData", {}, this.apiHeader).then((data) => {
            this.baseUrl = data.BaseUrl;
            this.$scope.pages = data.Pages;
            this.$scope.pageList = { name: 'abc' };

            this.$timeout(() => {
                this.$scope.loadedDashboard = true;
            });

            if (data.Dashboard.DashboardType == 1) this.paramChar = "&";
            else if (data.Dashboard.DashboardType == 2) this.paramChar = "?";

            const pageName = this.globalService.getParameterByName("page");
            const page = this.getPageByPageName(pageName);
            if (page) {
                this.$timeout(() => {
                    this.gotoDashboardPage(page.PageId);
                }, 1000);
            }
        }, (error) => {
            console.error(error);
        });
    }

    gotoDashboardPage(pageId, params, isUpdatePageParams) {
        this.$scope.currentPageId = pageId;
        const page = this.$scope.currentPage = this.getPageByPageId(pageId);
        const moduleId = page.Module ? page.Module.ModuleId : null;
        const moduleName = page.Module ? page.Module.ModuleName : null;
        const paramsQuery = params ? "&" + params : "";

        if (isUpdatePageParams) {
            const url = this.baseUrl + this.paramChar + "page=" + page.PageName + paramsQuery;
            this.globalService.pushState(url);
        }

        if (moduleId && moduleName) {
            const template = this.getAngularModuleTemplate(moduleId, moduleName);
            $("#dashboardPageModule").html(this.$compile(template)(this.$scope));
        }
    }

    getPageByPageId(pageId) {
        var result;

        const findNestedPage = (pages) => {
            _.forEach(pages, (p) => {
                if (p.PageId == pageId) result = p;
                else return findNestedPage(p.Pages);
            });
        };

        findNestedPage(this.$scope.pages);

        return result;
    }

    getPageByPageName(pageName) {
        var result;

        const findNestedPage = (pages) => {
            _.forEach(pages, (p) => {
                if (p.PageName == pageName) result = p;
                else return findNestedPage(p.Pages || []);
            });
        };

        findNestedPage(this.$scope.pages);

        return result;
    }

    getAngularModuleTemplate(moduleId, moduleName) {
        const result = `
        <div id="pnlBusinessEngine${moduleId}" data-module="${moduleId}" ng-controller="moduleController as $"
          ng-init="$.onInitModule(-1,'${moduleId}', '${moduleName}','${this.$scope.connectionID}',false,false,true)">
        </div>`;

        return result;
    }
}