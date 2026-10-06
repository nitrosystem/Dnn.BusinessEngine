import template from './send-email.html';

class SendEmailActionController {
  constructor(
  ) {
    'ngInject';
  }

  $onInit() {
  }
}

const SendEmailAction = {
  bindings: {
    action: '<',
    services: '<',
    suggestions: '<',
  },
  controller: SendEmailActionController,
  controllerAs: '$',
  templateUrl: template,
};

export default SendEmailAction;
