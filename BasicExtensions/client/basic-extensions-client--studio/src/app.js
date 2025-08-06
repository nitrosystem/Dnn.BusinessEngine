import "./app.css";

//import fields options widget
import TextboxOptions from "./field-types/textbox/textbox-options.html";
import TextareaOptions from "./field-types/textarea/textarea-options.html";
import SwitchButtonOptions from "./field-types/switch-button/switch-button-options.html";
import GridOptions from "./field-types/grid/grid-options.html";
import ButtonOptions from "./field-types/button/button-options.html";
import DropdownListOptions from "./field-types/dropdown-list/dropdown-options.html";
import CheckboxOptions from "./field-types/checkbox/checkbox-options.html";
import CheckboxListOptions from "./field-types/checkbox-list/checkbox-list-options.html";
import RadioButtonListOptions from "./field-types/radio-button-list/radio-button-list-options.html";
import UploadFileOptions from "./field-types/upload-file/upload-file-options.html";
import UploadImageOptions from "./field-types/upload-image/upload-image-options.html";

//database services
import SubmitEntityService from "./services-types/database/submit-entity/submit-entity.component";
import BindEntityService from "./services-types/database/bind-entity/bind-entity.component";
import DataSourceService from "./services-types/database/data-source/data-source.component";
import DataRowService from "./services-types/database/data-row/data-row.component";
import CustomQueryService from "./services-types/database/custom-qeury/custom-query.component";
import DeleteEntityRowService from "./services-types/database/delete-entity-row/delete-entity-row.component";

//dnn services
import LoginUserService from "./services-types/dnn-services/login-user/login-user.component";
import RegisterUserService from "./services-types/dnn-services/register-user/register-user.component";
import ApproveUserService from "./services-types/dnn-services/approve-user/approve-user.component";
import ResetPasswordService from "./services-types/dnn-services/reset-password/reset-password.component";

//public services
import SendSmsService from "./services-types/public-services/send-sms/send-sms.component";
import SendEmailService from "./services-types/public-services/send-email/send-email.component";

//run service actions
import RunServiceAction from "./action-types/run-service/run-service.component";

//programming actions
import JavascriptAction from "./action-types/programming/javascript.component";

//form actions
import SetVariableAction from "./action-types/form/set-variable.component";
import UpdateFieldDataSource from "./action-types/form/update-field-data-source.component";
import CallAction from "./action-types/form/call-action.component";

import GroupFieldComponent from "./field-types/group/group-studio.component";
import TextboxFieldComponent from "./field-types/textbox/textbox.component";
import TextareaFieldComponent from "./field-types/textarea/textarea.component";
import TextEditorFieldComponent from "./field-types/text-editor/editor.component";
import ContentFieldComponent from "./field-types/content/content.component";
import SwitchButtonComponent from "./field-types/switch-button/switch-button.compoment";
import GridFieldComponent from "./field-types/grid/grid.component";
import ButtonFieldComponent from "./field-types/button/button.component";
import DropdownListFieldComponent from "./field-types/dropdown-list/dropdown-list.component";
import CheckboxFieldComponent from "./field-types/checkbox/checkbox.component";
import CheckboxListFieldComponent from "./field-types/checkbox-list/checkbox-list.component";
import RadioButtonListFieldComponent from "./field-types/radio-button-list/radio-button-list.component";
import MessageBoxFieldComponent from "./field-types/message-box/message-box.component";
import UploadFileFieldComponent from "./field-types/upload-file/upload-file.component";
import UploadImageFieldComponent from "./field-types/upload-image/upload-image.component";

const app = window["app"];

// register business engine service components
app.component("bServiceCustomQuery", CustomQueryService);
app.component("bServiceSubmitEntity", SubmitEntityService);
app.component("bServiceDataSource", DataSourceService);
app.component("bServiceDataRow", DataRowService);
app.component("bServiceDeleteEntityRow", DeleteEntityRowService);
app.component("bServiceBindEntity", BindEntityService);
app.component("bServiceSendSms", SendSmsService);
app.component("bServiceSendEmail", SendEmailService);
app.component("bLoginUser", LoginUserService);
app.component("bRegisterUser", RegisterUserService);
app.component("bApproveUser", ApproveUserService);
app.component("bResetPassword", ResetPasswordService);
app.component("bActionRunService", RunServiceAction);
app.component("bActionJavascript", JavascriptAction);
app.component("bActionSetVariable", SetVariableAction);
app.component("bActionCallAction", CallAction);
app.component("bActionUpdateFieldDataSource", UpdateFieldDataSource);
// register business engine field components
app.component("bFieldGroup", GroupFieldComponent);
app.component("bFieldContent", ContentFieldComponent);
app.component("bFieldTextbox", TextboxFieldComponent);
app.component("bFieldTextarea", TextareaFieldComponent);
app.component("bFieldTextEditor", TextEditorFieldComponent);
app.component("bFieldDropdownList", DropdownListFieldComponent);
app.component("bFieldSwitchButton", SwitchButtonComponent);
app.component("bFieldCheckbox", CheckboxFieldComponent);
app.component("bFieldCheckboxList", CheckboxListFieldComponent);
app.component("bFieldRadioButtonList", RadioButtonListFieldComponent);
app.component("bFieldUploadFile", UploadFileFieldComponent);
app.component("bFieldUploadImage", UploadImageFieldComponent);
app.component("bFieldMessageBox", MessageBoxFieldComponent);
app.component("bFieldGrid", GridFieldComponent);
app.component("bFieldButton", ButtonFieldComponent);

// set base options when add field
window['TextboxOptions'] = TextboxOptions;
window['TextareaOptions'] = TextareaOptions;
window['SwitchButtonOptions'] = SwitchButtonOptions;
window['GridOptions'] = GridOptions;
window['buttonOptions'] = ButtonOptions;
window['DropdownListOptions'] = DropdownListOptions;
window['CheckboxOptions'] = CheckboxOptions;
window['CheckboxListOptions'] = CheckboxListOptions;
window['RadioButtonListOptions'] = RadioButtonListOptions;
window['UploadFileOptions'] = UploadFileOptions;
window['UploadImageOptions'] = UploadImageOptions;


