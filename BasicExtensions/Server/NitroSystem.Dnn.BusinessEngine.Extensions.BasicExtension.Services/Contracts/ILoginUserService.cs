using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Membership;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts
{
    public interface ILoginUserService
    {
        UserLoginStatus LoginUser(string username, string password, PortalSettings portalSettings);
    }
}
