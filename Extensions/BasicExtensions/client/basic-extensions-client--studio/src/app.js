import './app.css';

//services
import BindEntityService from './services-types/bind-entity/bind-entity.component';
import DataRowService from './services-types/data-row/data-row.component';
import DataSourceService from './services-types/data-source/data-source.component';
import SubmitEntityService from './services-types/submit-entity/submit-entity.component';
import DeleteEntityRowService from './services-types/delete-entity-row/delete-entity-row.component';
import CustomQueryService from './services-types/custom-qeury/custom-query.component';

//actions
import SetVariableAction from './action-types/form/set-variable.component';
import RedirectUrlAction from './action-types/redirect-url/redirect-url.component';

//fields
import TextboxFieldComponent from './field-types/textbox/textbox.component';
import TextareaFieldComponent from './field-types/textarea/textarea.component';
import CheckboxFieldComponent from './field-types/checkbox/checkbox.component';
import CheckboxListFieldComponent from './field-types/checkbox-list/checkbox-list.component';
import RadioButtonListFieldComponent from './field-types/radio-button-list/radio-button-list.component';
import DropdownListFieldComponent from './field-types/dropdown-list/dropdown-list.component';
import SwitchButtonComponent from './field-types/switch-button/switch-button.compoment';
import DatePickerFieldComponent from './field-types/date-picker/date-picker.component';
import ContentFieldComponent from './field-types/content/content.component';
import GridFieldComponent from './field-types/grid/grid.component';
import ButtonFieldComponent from './field-types/button/button.component';
import LinkButtonFieldComponent from './field-types/Link-button/Linkbutton.component';

//fields options
import TextboxOptions from './field-types/textbox/textbox-options.html';
import TextareaOptions from './field-types/textarea/textarea-options.html';
import CheckboxOptions from './field-types/checkbox/checkbox-options.html';
import CheckboxListOptions from './field-types/checkbox-list/checkbox-list-options.html';
import RadioButtonListOptions from './field-types/radio-button-list/radio-button-list-options.html';
import DropdownListOptions from './field-types/dropdown-list/dropdown-options.html';
import SwitchButtonOptions from './field-types/switch-button/switch-button-options.html';
import DatePickerOptions from './field-types/date-picker/date-picker-options.html';
import ButtonOptions from './field-types/button/button-options.html';
import LinkButtonOptions from './field-types/link-button/link-button-options.html';

const app = window['app'];

// register service components
app.component('bServiceCustomQuery', CustomQueryService);
app.component('bServiceSubmitEntity', SubmitEntityService);
app.component('bServiceDataSource', DataSourceService);
app.component('bServiceDataRow', DataRowService);
app.component('bServiceDeleteEntityRow', DeleteEntityRowService);
app.component('bServiceBindEntity', BindEntityService);

// register action components
app.component('bActionSetVariable', SetVariableAction);
app.component('bActionRedirectUrl', RedirectUrlAction);

// register field components
app.component('bFieldTextbox', TextboxFieldComponent);
app.component('bFieldTextarea', TextareaFieldComponent);
app.component('bFieldCheckbox', CheckboxFieldComponent);
app.component('bFieldCheckboxList', CheckboxListFieldComponent);
app.component('bFieldRadioButtonList', RadioButtonListFieldComponent);
app.component('bFieldDropdownList', DropdownListFieldComponent);
app.component('bFieldSwitchButton', SwitchButtonComponent);
app.component('bFieldDatePicker', DatePickerFieldComponent);
app.component('bFieldContent', ContentFieldComponent);
app.component('bFieldGrid', GridFieldComponent);
app.component('bFieldButton', ButtonFieldComponent);
app.component('bFieldLinkButton', LinkButtonFieldComponent);

//field options widget, show when adding new field
window['TextboxOptions'] = TextboxOptions;
window['TextareaOptions'] = TextareaOptions;
window['CheckboxOptions'] = CheckboxOptions;
window['CheckboxListOptions'] = CheckboxListOptions;
window['RadioButtonListOptions'] = RadioButtonListOptions;
window['DropdownListOptions'] = DropdownListOptions;
window['SwitchButtonOptions'] = SwitchButtonOptions;
window['DatePickerOptions'] = DatePickerOptions;
window['ButtonOptions'] = ButtonOptions;
window['LinkButtonOptions'] = LinkButtonOptions;