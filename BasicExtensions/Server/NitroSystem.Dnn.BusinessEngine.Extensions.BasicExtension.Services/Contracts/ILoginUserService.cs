using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Security.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts
{
    public interface ILoginUserService
    {
        UserLoginStatus LoginUser(string username, string password, IPortalSettings portalSettings);
    }
}
