import template from "./wizard.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";
import editStepWidget from "./edit-step.html";

class WizardFieldController {
    constructor($scope, $timeout, $q) {
        "ngInject";

        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$q = $q;

        this.editStepWidget = editStepWidget;

        $scope.$on("onGetWizardFieldPanes", (e, task, args) => {
            this.getWizardPanes.apply(this, [task, args]);
        });
    }

    $onInit() {
        this.$scope.$on(
            "onBindFieldSettings_" + this.field.FieldName,
            (e, args) => {
                this.field.CustomSettings = sidebarSettingsTemplate;
            }
        );

        this.field.Settings = this.field.Settings || {};
        this.field.Settings.ValidationMethod = 'bWizard_onValidateMatrix';
    }

    onAddStepClick() {
        this.workingMode = "edit-step";
        this.$scope.$emit("onShowRightWidget");

        this.step = { IsNew: true };
    }

    onEditStepClick(step) {
        this.stepBackup = step;
        this.step = _.clone(step);

        this.workingMode = "edit-step";
        this.$scope.$emit("onShowRightWidget");
    }

    onSaveStepClick() {
        this.field.Settings.Steps = this.field.Settings.Steps || [];

        if (this.step.IsNew) {
            delete this.step.IsNew;
            this.field.Settings.Steps.push(this.step);
        } else {
            this.field.Settings.Steps[
                this.field.Settings.Steps.indexOf(this.stepBackup)
            ] = this.step;
        }

        console.log(this.field.Settings.Steps);

        let newList = [];
        newList.push(...(_.orderBy(this.field.Settings.Steps, ['ViewOrder'])));

        this.field.Settings.Steps = _.clone(newList);

        console.log(this.field.Settings.Steps);

        this.disposeWorkingMode();
    }

    onCancelStepClick() {
        this.field.Settings.Steps[
            this.field.Settings.Steps.indexOf(this.stepBackup)
        ] = this.stepBackup;

        this.disposeWorkingMode();
    }

    disposeWorkingMode() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }

    getWizardPanes(task, args) {
        task.wait(() => {
            const defer = this.$q.defer();
            const field = args.field;
            const layout = args.layout;

            var panes = [];

            if (layout.indexOf('[[[') >= 0) {
                const itemHtml = this.getHtmlfromLayout(1, layout);
                const paneHtml = this.getHtmlfromLayout(2, layout);

                var index = 0;
                _.forEach(field.Settings.Steps, (step) => {
                    let pane = paneHtml.replace(/\${(.[^}]+)}/gm, function(match, group) {
                        return eval(group)
                    });

                    var item = itemHtml.replace(/\[STEP\]/g, `[FIELD].Settings.Steps[${index++}]`);
                    item = item.replace('[STEPPANE]', pane);
                    panes.push(item);
                });

                const renderedLayout = this.getRenderedLayout(layout, panes.join(" "));

                defer.resolve({ type: 0, html: renderedLayout });
            } else {
                _.forEach(field.Settings.Steps, (step) => {
                    const pane = `<div id="WizardStep_${step.Name}" data-pane="WizardStep_${step.Name}" data-pane-title="${step.Name} Pane" data-parent-id="${field.FieldID}" ng-show="[FIELD].Settings.CurrentStep=='${step.Name}'" class="${step.PaneCssClass}" ng-class="{'active':[FIELD].Settings.CurrentStep=='${step.Name}','disabled':![FIELD].Step['${step.Name}'].IsEnable}"></div>`;

                    panes.push(pane);
                });

                defer.resolve({ type: 1, html: panes.join(" ") });
            }

            return defer.promise;
        });
    }

    getHtmlfromLayout(type, layout) {
        var startBroketType = type == 1 ? '-[[[' : '*[[[';
        var endBroketType = type == 1 ? ']]]-' : ']]]*';

        const start = layout.indexOf(startBroketType);
        const end = layout.indexOf(endBroketType);
        const html = layout.substr(start + 4, end - start - 4);

        return html;
    }

    getRenderedLayout(layout, panesHtml) {
        const startItemBroket = layout.indexOf('-[[[');
        const endItemBroket = layout.indexOf(']]]-');
        const startPaneBroket = layout.indexOf('*[[[');

        const header = layout.substr(0, startItemBroket);
        const footer = layout.substr(endItemBroket + 4, startPaneBroket - endItemBroket);

        return header + panesHtml + footer;
    }
}

const WizardFieldComponent = {
    bindings: {
        field: "<",
    },
    controller: WizardFieldController,
    controllerAs: "$",
    templateUrl: template,
};

export default WizardFieldComponent;