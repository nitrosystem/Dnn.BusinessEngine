using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Reflection.TypeLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public interface ITypeLoaderFactory
    {
        Type GetTypeFromAssembly(string relativePath, string typeFullName, string scenarioName, PortalSettings portalSettings);

        void ClearAll();
    }
}
