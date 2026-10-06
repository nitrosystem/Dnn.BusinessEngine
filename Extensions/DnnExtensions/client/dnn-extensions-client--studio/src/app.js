//services
import LoginUserService from './services-types/login-user/login-user.component';
import RegisterUserService from './services-types/register-user/register-user.component';
import ApproveUserService from './services-types/approve-user/approve-user.component';
import ResetPasswordService from './services-types/reset-password/reset-password.component';
import UpdateUserService from './services-types/update-user/update-user.component';

const app = window['app'];

// register service components
app.component('bLoginUser', LoginUserService);
app.component('bRegisterUser', RegisterUserService);
app.component('bApproveUser', ApproveUserService);
app.component('bResetPassword', ResetPasswordService);
app.component('bUpdateUser', UpdateUserService);
