using DotNetNuke.Security.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.ViewModels.DnnService
{
    public class UserLoginViewModel
    {
        public UserLoginStatus Status { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}