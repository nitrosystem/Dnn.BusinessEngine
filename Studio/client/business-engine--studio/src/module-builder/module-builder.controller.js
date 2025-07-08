import formDesignerTemplate from "./form-designer/form-designer.html";
import htmlEditorTemplate from "./html-editor/html-editor.html";

export class ModuleBuilderController {
    constructor($scope, globalService, apiService, notificationService) {
        "ngInject";

        this.$scope = $scope;
        this.globalService = globalService;
        this.apiService = apiService;
        this.notifyService = notificationService;

        this.formDesignerTemplate = formDesignerTemplate;
        this.htmlEditorTemplate = htmlEditorTemplate;

        this.onPageLoad();
    }

    onPageLoad() {
        const moduleId = this.globalService.getParameterByName("id");

        this.running = "get-module-lite";
        this.awaitAction = {
            title: "Get Module Lite Data",
            subtitle: "Just a moment for loading module lie data...",
        };

        this.apiService.get("Module", "GetModuleBuilderType", { moduleId: (moduleId || null) }).then((data) => {
            this.moduleBuilderType = data;

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
}