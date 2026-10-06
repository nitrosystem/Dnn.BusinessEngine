import template from './set-variable.component.html';

class SetVariableActionController {
  constructor(
  ) {
    'ngInject';
  }

  $onInit() {
  }
}

const SetVariableAction = {
  bindings: {
    action: '<',
  },
  controller: SetVariableActionController,
  controllerAs: '$',
  templateUrl: template,
};

export default SetVariableAction;
