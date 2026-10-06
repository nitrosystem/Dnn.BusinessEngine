import { GlobalUtil } from '../../../shared/utils';
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';
import { Lightbox } from '../../../shared/libs';

export class CreateModuleTemplateController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        validationService,
        notificationService
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        $scope.$parent.createModuleValidatedStep.push(2);
        $scope.$on('onCreateModuleValidateStep2', (e, defer) => {
            defer.resolve(true);
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');

        this.running = 'get-templates';
        this.awaitAction = {
            title: 'Loading Templates',
            subtitle: 'Just a moment for loading templates...',
        };

        this.apiService.get('Module', 'GetTemplates', { moduleId: id }).then((data) => {
            this.templates = data.Templates;
            this.module = data.Module;
            this.template = this.module.Template
                ? this.templates.find(t => t.TemplateName === this.module.Template)
                : null;
            if (this.template)
                this.onSelectTemplateClick(this.template);
            else
                this.module.hasNotTemplate = true;

            this.onSidebarTabClick('toolbox');

            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
    }

    onSelectTemplateClick(template) {
        this.template = GlobalHelper.clone(template);
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
            this.template.PreviewImages = typeof this.template?.PreviewImages === 'string'
                ? JSON.parse(this.template.PreviewImages)
                : this.template.PreviewImages;

            this.$timeout(() => {
                const element = document.getElementById('templatePreviewImages');
                new Lightbox(element, {});
            });
        }
    }

    onSelectThemeClick(theme) {
        this.module.Theme = theme.ThemeName;
        this.module.ThemeCssClass = theme.ThemeCssClass;
    }

    onSaveTemplateClick() {
        this.running = 'save-template';
        this.awaitAction = {
            title: 'Save Template For Module',
            subtitle: 'Just a moment for setting the template for this module...',
        };

        this.apiService.post('Module', 'SaveModuleTemplate', this.module).then((data) => {
            if (this.module.hasNotTemplate) {
                this.notifyService.success('Module template has been created successfully.');
                delete this.module.hasNotTemplate;
            }
            else
                this.notifyService.info('Module template has been updated successfully.');

            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onPreviousStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 2 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 3 });
    }

    //#endregion
}