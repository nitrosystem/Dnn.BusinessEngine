import template from './templated-select.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class TemplatedSelectFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_TemplatedSelect', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
        this.field.Settings = this.field.Settings || {};

        this.$scope.$watch('$.field.Settings.LayoutTemplate', (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.field.Settings.Content = this.field.Settings.LayoutTemplate.replace('[item-template]',
                    `b-for="item in [FIELD].DataSource.Items" b-click="[FIELD].onItemClick([FIELD],item)"
                        b-disabled='[[Settings.DisabledConditions]]`
                );
            }
        });
    }
}

const TemplatedSelectFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: TemplatedSelectFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default TemplatedSelectFieldComponent;