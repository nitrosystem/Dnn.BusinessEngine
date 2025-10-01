using DotNetNuke.Security.Membership;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.DnnService
{
    public class ServiceUserLoginInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public UserLoginStatus Status { get; set; }
    }
}