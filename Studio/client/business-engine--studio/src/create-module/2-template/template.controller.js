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

        this.$scope.$parent.createModuleValidatedStep.push(2);

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
        this.template = _.cloneDeep(template);
        this.module.Template = this.template.TemplateName;

        const files = [
            this.template.TemplatePath,
            ...(this.template.TemplateCssPath ? [this.template.TemplateCssPath] : [])
        ];

        this.apiService.getContents(files, true).then((contents) => {
            const [html, css, pp] = contents;
            this.module.LayoutTemplate = html;
            this.module.LayoutCss = css;
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
    }

    onApplyTemplateClick() {
        const $defer = this.$q.defer();

        this.form.validated = true;
        this.form.validator(this.module);
        if (this.form.valid) {
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