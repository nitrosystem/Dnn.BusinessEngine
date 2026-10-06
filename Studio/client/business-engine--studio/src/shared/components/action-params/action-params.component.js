import { GlobalHelper } from '../../helpers';
import { GlobalUtil } from '../../utils';
import template from './action-params.html';

class ActionParamListController {
    constructor($scope, $timeout) {
        'ngInject';

        this.$scope = $scope;
        this.$timeout = $timeout;

        // Watch for params changes: reset edit state when list changes
        $scope.$watch('$.params', (newVal, oldVal) => {
            if (this.param && oldVal && newVal !== oldVal) {
                this.param = null;
            }
            newVal?.forEach((param) => (param.isEdited = null));
        });

        // Watch for actionId changes: reinitialize params
        $scope.$watch('$.actionId', (newVal, oldVal) => {
            if (newVal !== oldVal) this._reinitParams();
        });
    }

    $onInit() {
        if (this.actionId) this._reinitParams();
    }

    /**
     * Handle click on a param row to enter edit mode.
     * @param {Object} param - The param being edited
     * @param {Number} $index - Index of the param in the list
    */
    onRowItemClick(param, $index) {
        // If another param is currently being edited, commit it first
        if (this.param) {
            this.param.isEdited = false;

            const index = this.params.findIndex(p => p.ParamName === this.param.ParamName);
            if (index >= 0) {
                this.params[index] = GlobalHelper.clone(this.param);
            }

            this.param = null;
        }

        this.param = param;
        this.param.isEdited = true;
        this.param.orderId = $index + 1;
        this.$timeout(() => this.$scope.$broadcast('onEditParam'), 500);
    }

    /**
     * Commit the currently edited param.
    */
    onDoneClick() {
        if (!this.param) return;

        this.param.isEdited = false;

        const index = this.params.findIndex(p => p.ParamName === this.param.ParamName);
        if (index >= 0) {
            this.params[index] = GlobalHelper.clone(this.param);
        }

        this.param = null;
    }

    /**
     * Refresh params from the selected action definition.
    */
    onRefreshActionParamsClick() {
        this.params = this.params ?? [];
        const action = this.actions?.find(a => a.Id === this.actionId);
        if (action) GlobalUtil.bindParams(this.params, action.Params);
    }

    /**
     * Reinitialize params state when the action changes.
     * @private
    */
    _reinitParams() {
        this.params?.forEach(p => p.isEdited = false);
        this.param = null;
        this.onRefreshActionParamsClick();
    }
}

const ActionParamListComponent = {
    bindings: {
        params: '<',
        actionId: '<',
        actions: '<'
    },
    controller: ActionParamListController,
    controllerAs: '$',
    templateUrl: template,
    transclude: true,
};

export default ActionParamListComponent;