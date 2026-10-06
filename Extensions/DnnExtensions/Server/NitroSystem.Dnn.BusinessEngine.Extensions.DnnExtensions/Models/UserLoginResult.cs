using DotNetNuke.Security.Membership;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Models
{
   public class UserLoginResult
    {
        public string Message { get; set; }
        public UserLoginStatus Status { get; set; }
    }
}
