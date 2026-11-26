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
import MatrixFieldComponent from "./field-types/matrix/matrix.component";
import WizardFieldComponent from "./field-types/wizard/wizard.component";
import ContactInputFieldComponent from "./field-types/contact-input/contact-input.component";
import BookingCalendarFieldComponent from "./field-types/booking-calendar/booking-calendar.component";
//end fields components 
//-----------------------------------------------------------------

app.component("bFieldWizard", WizardFieldComponent);
app.component("bFieldMatrix", MatrixFieldComponent);
app.component("bFieldContactInput", ContactInputFieldComponent);
app.component("bFieldBookingCalendar", BookingCalendarFieldComponent);