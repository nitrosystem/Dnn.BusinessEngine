import "./app.css";

const app = window["app"];

//start service components 
//-----------------------------------------------------------------
//-----------------------------------------------------------------
//end service components 


//start action components 
//-----------------------------------------------------------------
//-----------------------------------------------------------------
//end action components 

//start field components 
//-----------------------------------------------------------------
import ChosenDropdownFieldComponent from "./field-types/chosen-dropdown/chosen-dropdown";
import MatrixFieldComponent from "./field-types/matrix/matrix.component";
import Select2DropdownFieldComponent from "./field-types/select2-dropdown/select2-dropdown";
import WizardFieldComponent from "./field-types/wizard/wizard.component";
import BootstrapTabsFieldComponent from "./field-types/bootstrap-tabs/bootstrap-tabs.component";
import ContactInputFieldComponent from "./field-types/contact-input/contact-input.component";
//end fields components 
//-----------------------------------------------------------------

app.component("bFieldChosenDropdown", ChosenDropdownFieldComponent);
app.component("bFieldSelect2Dropdown", Select2DropdownFieldComponent);
app.component("bFieldWizard", WizardFieldComponent);
app.component("bFieldMatrix", MatrixFieldComponent);
app.component("bFieldBootstrapTabs", BootstrapTabsFieldComponent);
app.component("bFieldContactInput", ContactInputFieldComponent);