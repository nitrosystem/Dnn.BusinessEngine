import template from "./content.html";

class ContentFieldController {
  constructor() {
    "ngInject";
  }

  $onInit() {}
}

const ContentFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: ContentFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default ContentFieldComponent;
