var SimpleLightbox = require('simple-lightbox');
import "simple-lightbox/dist/simpleLightbox.min.css"

export class CreateModuleTemplateController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.filter = {};

        this.$rootScope.createModuleValidatedStep.push(2);

        $scope.$on("onCreateModuleValidateStep2", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName('id');

        this.running = "get-templates";
        this.awaitAction = {
            title: "Loading Templates",
            subtitle: "Just a moment for loading templates...",
        };

        this.apiService.get("Module", "GetTemplates", { moduleId: id }).then((data) => {
            this.templates = data.Templates;
            this.module = data.Module;
            this.oldModule = angular.copy(this.module);

            this.template = this.module.Template
                ? _.find(this.templates, (t) => { return t.TemplateName == this.module.Template; })
                : null;

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
            Template: {
                required: true,
            },
            LayoutTemplate: {
                required: true,
            }
        },
            true,
            this.$scope,
            "$.module"
        );
    }

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
    }

    onSelectTemplateClick(template) {
        if (this.module.Template == template.TemplateName) return;

        this.template = _.cloneDeep(template);
        this.module.Template = this.template.TemplateName;

        this.apiService.getContent(this.template.TemplatePath, true).then((data) => {
            this.module.LayoutTemplate = data;

            _.filter(this.template.Themes, (t) => {
                return t.ThemeName == this.module.Theme ||
                    ((this.template.Themes ?? []).length && t.Id == this.template.Themes[0].Id)
            }).map((theme) => {
                this.onSelectThemeClick(theme);
            });
        });

        if (this.template.PreviewImages) {
            this.template.PreviewImages = JSON.parse(this.template.PreviewImages);

            this.$timeout(() => {
                new SimpleLightbox({ elements: '.lnk-preview-image' });
            });
        }
    }

    onSelectThemeClick(theme) {
        this.module.Theme = theme.ThemeName;

        this.apiService.getContent(theme.ThemeCssPath, true).then((data) => {
            this.module.LayoutCss = data;
        });
    }

    onApplyTemplateClick() {
        const $defer = this.$q.defer();

        this.form.validated = true;
        this.form.validator(this.module);

        var changes = this.globalService.compareTwoObject(this.module, this.oldModule);
        if (Object.keys(changes).length === 0)
            $defer.resolve(true);
        else if (this.form.valid) {
            this.running = "apply-template";
            this.awaitAction = {
                title: "Apply Template For Module",
                subtitle: "Just a moment for setting the template for this module...",
            };

            this.apiService.post("Module", "SaveModuleTemplate", this.module).then((data) => {
                if (data) this.notifyService.success("The module template has been save successfully");

                $defer.resolve(data);

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

        return $defer.promise;
    }

    onPrevStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 2 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 3 });
    }

    validateStep(task, args) {
        task.wait(() => {
            return this.onApplyTemplateClick();
        });
    }
}