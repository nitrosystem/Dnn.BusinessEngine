import template from './right-widget.component.html';

class RightWidgetController {
    constructor() {
        'ngInject';
    }

    onMaximizeClick() {
        this.widgetFull = !this.widgetFull;
    }

    onReloadClick() {
        this.module[this.reloadMethod]();
    }

    onCloseClick() {
        if (this.module[this.closeMethod]) this.module[this.closeMethod]();
    }
}

const RightWidgetComponent = {
    bindings: {
        title: '@',
        subtitle: '@',
        icon: '@',
        reloadMethod: '@',
        closeMethod: '@',
        hideFullscreen: '=',
        hideReload: '=',
        awaitAction: '=',
        module: '<'
    },
    controller: RightWidgetController,
    controllerAs: '$',
    templateUrl: template,
    transclude: true,
};

export default RightWidgetComponent;