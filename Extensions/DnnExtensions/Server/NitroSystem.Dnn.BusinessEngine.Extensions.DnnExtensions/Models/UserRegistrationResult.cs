using DotNetNuke.Security.Membership;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Models
{
    public class UserRegistrationResult
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public UserCreateStatus Status { get; set; }
    }
}
