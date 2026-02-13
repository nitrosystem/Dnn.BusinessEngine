using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Components;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using DotNetNuke.Entities.Users;
using System.IO;
using DotNetNuke.Data;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import
{
    public static class ImportWorker
    {
        public static void ImportScenario(string zipFilePath, PortalSettings portalSettings, UserInfo user,IDataContext dataContext, HttpContext httpContext)
        {
            string tempPath = portalSettings.HomeSystemDirectoryMapPath + @"business-engine\temp\" + Path.GetFileNameWithoutExtension(zipFilePath) + @"\";
            Directory.CreateDirectory(tempPath);

            var framework = new ImportFramework(zipFilePath, tempPath, dataContext,  httpContext);
            framework.RegisterComponent(new ScenarioComponent(tempPath, portalSettings, user));

            framework.CreateWorkflow();

            framework.Init(framework.Work, progress =>
            {
                framework.ProgressChanged(progress);
            });
        }
    }
}
