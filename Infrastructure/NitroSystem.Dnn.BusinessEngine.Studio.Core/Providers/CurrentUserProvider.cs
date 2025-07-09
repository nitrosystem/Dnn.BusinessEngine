using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers
{
    public static class CurrentUserProvider
    {
        public static int GetCurrentUserId(ICacheService cacheService)
        {
            var cachekey = "BE_Dnn_UserId";
            var result = cacheService.Get<int?>(cachekey);
            if (result == null)
            {
                result = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo()?.UserID ?? -1;

                cacheService.Set<int?>(cachekey, result);
            }

            return result.Value;
        }
    }
}
