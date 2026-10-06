import template from './redirect-url.component.html';

class RedirectUrlActionController {
  constructor(
  ) {
    'ngInject';
  }

  $onInit() {
    if (!this.action.Settings) this.action.Settings = { DslScript: '_Url = ' / '' };

    this.suggestions['_Url'] = '';
    this.suggestions['_PageParam'].returnurl = ''
  }
}

const RedirectUrlAction = {
  bindings: {
    action: '<',
    suggestions: '<'
  },
  controller: RedirectUrlActionController,
  controllerAs: '$',
  templateUrl: template,
};

export default RedirectUrlAction;
