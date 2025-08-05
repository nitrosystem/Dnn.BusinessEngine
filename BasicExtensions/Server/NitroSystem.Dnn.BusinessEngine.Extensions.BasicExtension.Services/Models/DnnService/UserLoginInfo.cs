using DotNetNuke.Security.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.DnnService
{
    public class ServiceUserLoginInfo
    {
        public UserLoginStatus Status { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}