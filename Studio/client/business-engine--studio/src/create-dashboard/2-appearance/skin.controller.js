export class CreateDashboardSkinController {
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

        this.$rootScope.createDashboardValidatedStep.push(3);

        $scope.$on("onCreateDashboardValidateStep3", (e, task, args) => {
            task.wait(() => {
                const $defer = this.$q.defer();

                this.form.validated = true;
                this.form.validator(this.dashboard);

                $defer.resolve(this.form.valid);

                return $defer.promise;
            });
        });

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");
        const parent = this.globalService.getParameterByName("parent");
        const moduleId = parent || id;

        this.running = "get-skins";
        this.awaitAction = {
            title: "Loading Skins",
            subtitle: "Just a moment for loading dashboard skins...",
        };

        this.apiService.get("Module", "GetDashboardAppearance", { moduleId: moduleId || null }).then((data) => {
            this.dashboard = data.Dashboard;
            this.skins = data.Skins;

            _.filter(this.skins, (s) => { return s.SkinName === this.dashboard.Skin }).map((skin) => {
                this.onSelectSkinClick(skin);
            });

            this.onSidebarTabClick('toolbox');

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });

        this.setForm();
    }

    setForm() {
        this.form = this.validationService.init({
            Skin: {
                required: true,
            },
            Template: {
                required: true,
            }
        },
            true,
            this.$scope,
            "$.dashboard"
        );
    }

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
    }

    onPreviousStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 2 });
    }

    onNextStepClick() {
        this.form.validated = true;
        this.form.validator(this.dashboard);
        if (this.form.valid) this.$scope.$emit('onCreateDashboardChangeStep', { step: 4 });
    }

    onSelectSkinClick(skin) {
        if (this.template && this.dashboard.Skin == skin.SkinName) return;

        this.skin = skin;
        this.dashboard.SkinId = skin.Id;
        this.dashboard.Skin = skin.SkinName;
    }

    onTemplateClick(template) {
        if (this.dashboard.Template == template.TemplateName) return;

        this.template = template
        this.dashboard.Template = template.TemplateName;
        this.dashboard.TemplatePath = template.TemplatePath;
    }

    onSaveDashboardSkinClick() {
        this.form.validated = true;
        this.form.validator(this.dashboard);
        if (this.form.valid) {
            this.running = "save-skin";
            this.awaitAction = {
                title: "Saving Skin",
                subtitle: "Just a moment for saving the dashboard skin...",
            };

            this.apiService.post("Module", "SaveDashboardAppearance", this.dashboard).then((data) => {
                this.notifyService.success("Dashboard skin updated has been successfully");

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    onCancelDashboardClick() {
        this.onCloseWindow();
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}