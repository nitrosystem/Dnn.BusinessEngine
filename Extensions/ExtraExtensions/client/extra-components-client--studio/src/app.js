//services
import SendSmsService from './services-types/send-sms/send-sms.component';
import SendEmailService from './services-types/send-email/send-email.component';

//actions
import SendEmailAction from './action-types/send-email/send-email.component';

//fields
import UploadFileFieldComponent from './field-types/upload-file/upload-file.component';
import UploadImageFieldComponent from './field-types/upload-image/upload-image.component';
import TextEditorFieldComponent from './field-types/text-editor/editor.component';
import TimerTextFieldComponent from './field-types/timer-text/timer-text.component';
import SlimSelectDropdownFieldComponent from './field-types/slimselect-dropdown/slimselect-dropdown.component';
import CaptchaFieldComponent from './field-types/captcha/captcha.component';
import BootstrapModalFieldComponent from './field-types/bootstrap-modal/bootstrap-modal.component';
import GroupFieldComponent from './field-types/group/group-studio.component';

//fields options
import UploadFileOptions from './field-types/upload-file/upload-file-options.html';
import UploadImageOptions from './field-types/upload-image/upload-image-options.html';
import SlimSelectDropdownFieldOptions from './field-types/slimselect-dropdown/slimselect-dropdown-options.html';
import TextEditorOptions from './field-types/text-editor/text-editor-options.html';
import TimerTextOptions from './field-types/timer-text/timer-text-options.html';
import CaptchaOptions from './field-types/captcha/captcha-options.html';
import BootstrapModalOptions from './field-types/bootstrap-modal/bootstrap-modal-options.html';

const app = window['app'];

// register business engine service components
app.component('bServiceSendSms', SendSmsService);
app.component('bServiceSendEmail', SendEmailService);

// register action components
app.component('bActionSendEmail', SendEmailAction);

// register business engine field components
app.component('bFieldUploadFile', UploadFileFieldComponent);
app.component('bFieldUploadImage', UploadImageFieldComponent);
app.component('bFieldTextEditor', TextEditorFieldComponent);
app.component('bFieldTimerText', TimerTextFieldComponent);
app.component('bFieldSlimSelectDropdown', SlimSelectDropdownFieldComponent);
app.component('bFieldCaptcha', CaptchaFieldComponent);
app.component('bFieldBootstrapModal', BootstrapModalFieldComponent);
app.component('bFieldGroup', GroupFieldComponent);

// set base options when add field
window['UploadFileOptions'] = UploadFileOptions;
window['UploadImageOptions'] = UploadImageOptions;
window['SlimSelectDropdownOptions'] = SlimSelectDropdownFieldOptions;
window['TextEditorOptions'] = TextEditorOptions;
window['TimerTextOptions'] = TimerTextOptions;
window['CaptchaOptions'] = CaptchaOptions;
window['BootstrapModalOptions'] = BootstrapModalOptions;

