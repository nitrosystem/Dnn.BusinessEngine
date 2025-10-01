using System;
using DotNetNuke.Entities.Portals;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader
{
    public interface ITypeLoaderFactory
    {
        Type GetTypeFromAssembly(string relativePath, string typeFullName, string scenarioName, PortalSettings portalSettings);

        void ClearAll();
    }
}
