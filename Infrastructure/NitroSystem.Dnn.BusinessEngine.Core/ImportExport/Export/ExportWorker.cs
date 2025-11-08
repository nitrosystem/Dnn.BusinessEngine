using System;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export.Components;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ImportExport.Export;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public static class ExportWorker
    {
        public static void ExportScenario(IServiceProvider services, ManifestModel manifest, string basePath, Guid scenarioId)
        {
            string manifestPath = $@"{basePath}business-engine\import-export\export\{manifest.PackageName.ToLower()}\";

            var framework = new ExportFramework(manifestPath, manifest);
            framework.RegisterComponent(new ScenarioComponent(services.GetRequiredService<IBaseService>(), manifestPath, scenarioId));
            framework.CreateWorkflow();
            framework.Init(framework.Work, progress =>
            {
                framework.ProgressChanged(progress);
            });
        }
    }
}
