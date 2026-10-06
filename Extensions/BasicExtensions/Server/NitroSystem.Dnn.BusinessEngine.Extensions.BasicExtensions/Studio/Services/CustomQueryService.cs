using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services
{
    public class CustomQueryService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ISql _sql;
        private readonly IAppModelService _appModelService;

        public CustomQueryService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ISql sql,
            IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _sql = sql;
            _appModelService = appModelService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objCustomQueryServiceInfo = await _sql.GetByColumnAsync<CustomQueryServiceInfo>("ServiceId", serviceId);

            return objCustomQueryServiceInfo != null
                ? await HybridMapper.MapAsync<CustomQueryServiceInfo, CustomQueryServiceViewModel>(objCustomQueryServiceInfo,
                async (src, dest) =>
                {
                    dest.Query = (await _sql.GetStoredProcedureScriptAsync(objCustomQueryServiceInfo.StoredProcedureName))
                    .Replace(objCustomQueryServiceInfo.StoredProcedureName, "{ProcedureName}");
                    dest.Settings = dest.Settings ?? new Dictionary<string, object>();

                })
                : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var appModels = await _appModelService.GetAppModelsListItemAsync(scenarioId, "ModelName");
            return new Dictionary<string, object>
            {
                { "AppModels", appModels }
            };
        }

        public async Task<Guid> SaveService(ServiceViewModel service, string extensionServiceJson)
        {
            var customQueryService = JsonConvert.DeserializeObject<CustomQueryServiceViewModel>(extensionServiceJson);

            var customQuery = customQueryService.Query;
            customQuery = customQuery.Replace("{Schema}", "dbo");
            customQuery = customQuery.Replace("{ProcedureName}", customQueryService.StoredProcedureName);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", customQueryService.StoredProcedureName);
            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, dropQuery);
            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, customQuery);

            var objCustomQueryServiceInfo = HybridMapper.Map<CustomQueryServiceViewModel, CustomQueryServiceInfo>(customQueryService);
            objCustomQueryServiceInfo.ServiceId = service.Id;

            if (objCustomQueryServiceInfo.Id == Guid.Empty)
                objCustomQueryServiceInfo.Id = await _sql.InsertAsync<CustomQueryServiceInfo>(objCustomQueryServiceInfo);
            else
                await _sql.UpdateAsync<CustomQueryServiceInfo>(objCustomQueryServiceInfo);

            return objCustomQueryServiceInfo.Id;
        }
    }
}
