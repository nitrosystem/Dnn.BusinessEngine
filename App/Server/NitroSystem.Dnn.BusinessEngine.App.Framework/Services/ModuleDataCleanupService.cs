using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Framework.Services
{
    //public class ModuleDataCleanupService : BackgroundService
    //{
    //    private readonly IModuleData _moduleData;

    //    public ModuleDataCleanupService(IModuleData moduleData)
    //    {
    //        _moduleData = moduleData;
    //    }

    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            _moduleData.CleanupOldConnections(TimeSpan.FromMinutes(7));
    //            await Task.Delay(TimeSpan.FromMinutes(7), stoppingToken);
    //        }
    //    }
    //}
}
