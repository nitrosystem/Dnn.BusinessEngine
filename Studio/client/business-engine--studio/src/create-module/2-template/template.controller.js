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
        this.filter = { pageIndex: 1, pageSize: 10 };

        this.$rootScope.createModuleValidatedStep.push(2);

        $scope.$on("onCreateModuleValidateStep2", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        this.onPageLoad();
    }

    onPageLoad() {
        this.onSidebarTabClick('toolbox');

        this.moduleId = this.globalService.getParameterByName('id');

        this.getTemplates();
    }

    getTemplates() {
        this.running = "get-templates";
        this.awaitAction = {
            title: "Loading Templates",
            subtitle: "Just a moment for loading templates...",
        };

        this.apiService.get("Module", "GetTemplates", {
            moduleId: this.moduleId,
            searchText: this.filter.searchText
        }).then((data) => {
            this.module = data.Module;
            this.installedTemplates = data.InstalledTemplates;

            if (this.module.Template) {
                _.filter(this.installedTemplates, (t) => { return t.TemplateName == this.module.Template }).map((template) => {
                    this.onSelectTemplateClick(template);
                });
            }

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

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
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

    onSelectTemplateClick(template) {
        if (this.template && this.template.TemplateName == template.TemplateName) return;

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

        this.running = "apply-template";
        this.awaitAction = {
            title: "Apply Template For Module",
            subtitle: "Just a moment for setting the template for this module...",
        };

        this.apiService.post("Module", "SaveModuleTemplate", {
            ModuleId: this.module.Id,
            Template: this.module.Template,
            Theme: this.module.Theme,
            LayoutTemplate: this.module.LayoutTemplate,
            LayoutCss: this.module.LayoutCss
        }).then((data) => {
            this.notifyService.success(
                "The module template has been save successfully"
            );

            $defer.resolve(true);

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });

        return $defer.promise;
    }

    onAddTemplateClick() {
        this.$scope.$emit("onGotoPage", { page: "create-template" });
    }

    onEditTemplateClick(id, title) {
        this.$scope.$emit("onGotoPage", {
            page: "create-template",
            id: id,
            title: title,
        });
    }

    onDeleteTemplateClick(id, index) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary template!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "get-templates";
                this.awaitAction = {
                    title: "Remove Template",
                    subtitle: "Just a moment for removing template...",
                };

                this.apiService.post("Studio", "DeleteTemplate", { Id: id }).then(
                    (data) => {
                        this.templates.splice(index, 1);

                        this.notifyService.success("Template deleted has been successfully");

                        this.$rootScope.refreshSidebarExplorerItems();

                        delete this.awaitAction;
                        delete this.running;
                    }, (error) => {
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
}