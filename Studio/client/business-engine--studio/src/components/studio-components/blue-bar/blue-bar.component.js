import template from "./blue-bar.component.html";
import moment from "moment";

class BlueBarController {
    constructor($rootScope) {
        "ngInject";

        this.$rootScope = $rootScope;
    }

    $onInit() {
        this.handler = this;
    }
   
    onNotificationMouseenter() {
        if (this.notification && this.notification.showTaskWidgetOnMouseEnter)
            this.form.showTaskWidget = true;
    }
}

const BlueBarComponent = {
    bindings: {
        handler: '=',
        notification: '<',
        action: '<',
        form: '='
    },
    controller: BlueBarController,
    controllerAs: "$",
    templateUrl: template,
    transclude: true,
};

export default BlueBarComponent;