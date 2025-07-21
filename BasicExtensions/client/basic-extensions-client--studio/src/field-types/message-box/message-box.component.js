import template from "./message-box.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class MessageBoxFieldController {
  constructor($scope, $timeout) {
    "ngInject";

    this.$scope = $scope;
    this.$timeout = $timeout;
  }

  $onInit() {
    this.$scope.$on(
      "onBindFieldSettings_" + this.field.FieldName,
      (e, args) => {
        this.field.CustomSettings = sidebarSettingsTemplate;
      }
    );
  }

  onAddMessageClick() {
    if (this.message) return;

    this.field.Settings.Messages = this.field.Settings.Messages || [];
    this.field.Settings.Messages.push({
      IsNew: true,
      IsEdited: true,
    });

    this.messageIndex = this.field.Settings.Messages.length - 1;
    this.message = _.clone(this.field.Settings.Messages[this.messageIndex]);

    this.workingMode = "edit-message";
    this.$scope.$emit("onShowRightWidget");
  }

  onEditMessageClick(message, $index) {
    if (this.message) return;

    message.IsEdited = true;

    this.messageIndex = $index;
    this.message = _.clone(message);
    this.message.IsEdited = true;
    this.message.IsNew = false;

    this.workingMode = "edit-message";
    this.$scope.$emit("onShowRightWidget");
  }

  onSaveMessageClick() {
    this.message.IsEdited = false;

    this.field.Settings.Messages[this.messageIndex] = _.clone(this.message);

    delete this.message;

    this.disposeWorkingMode();
  }

  onCancelEditMessageClick() {
    this.disposeWorkingMode();
  }

  disposeWorkingMode() {
    this.$scope.$emit("onHideRightWidget");

    this.$timeout(() => {
      delete this.workingMode;
    }, 200);
  }
}

const MessageBoxFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: MessageBoxFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default MessageBoxFieldComponent;
