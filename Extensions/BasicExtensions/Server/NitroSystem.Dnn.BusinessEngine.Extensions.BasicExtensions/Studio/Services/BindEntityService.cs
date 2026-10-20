using DotNetNuke.Data;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Tables;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;


namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services
{
    public class BindEntityService : IExtensionServiceFactory, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ISql _sql;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelService;

        public BindEntityService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ISql sql,
            IEntityService entityService,
            IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _sql = sql;
            _entityService = entityService;
            _appModelService = appModelService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objBindEntityServiceInfo = await _sql.GetByColumnAsync<BindEntityServiceInfo>("ServiceId", serviceId);

            return objBindEntityServiceInfo != null
                ? HybridMapper.Map<BindEntityServiceInfo, BindEntityServiceViewModel>(objBindEntityServiceInfo,
                    (src, dest) => dest.Settings = dest.Settings ?? new Dictionary<string, object>())
                : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var entities = await _entityService.GetEntitiesListItemAsync(scenarioId, "EntityName");
            var appModels = await _appModelService.GetAppModelsListItemAsync(scenarioId, "ModelName");

            return new Dictionary<string, object>
            {
                { "Entities", entities },
                { "AppModels", appModels }
            };
        }

        public async Task<Guid> SaveService(ServiceViewModel service, string extensionServiceJson)
        {
            var bindEntityService = JsonConvert.DeserializeObject<BindEntityServiceViewModel>(extensionServiceJson);
            var bindEntityQuery = bindEntityService.BaseQuery;
            var spParams = new List<string>();
            var selectedColumns = new List<string>();
            var filters = new List<string>();

            if (!bindEntityService.Settings.TryGetValue("BaseQueryModified", out var value) || !(value is bool baseQueryModified) || !baseQueryModified)
            {
                bindEntityService.BaseQuery = null;
            }

            foreach (var property in bindEntityService.ModelProperties)
            {
                if (!property.IsSelected) continue;

                selectedColumns.Add(string.Format("[{0}] AS [{1}]", property.ColumnName, property.PropertyName));
            }

            if (service.Params != null)
            {
                foreach (var serviceParam in service.Params)
                {
                    spParams.Add(string.Format("{0} {1}", serviceParam.ParamName, serviceParam.ParamType));
                }
            }

            if (bindEntityService.Filters != null)
            {
                foreach (var group in bindEntityService.Filters.GroupBy(f => f.ConditionGroupName))
                {
                    var queryGroup = new List<string>();
                    foreach (var filter in group)
                    {
                        if (filter.Type == 1) queryGroup.Add(filter.CustomQuery);
                    }

                    if (queryGroup.Count > 0) filters.Add(string.Format("({0})", string.Join(" OR ", queryGroup)));
                }
            }

            bindEntityQuery = bindEntityQuery.Replace("{Schema}", "dbo");
            bindEntityQuery = bindEntityQuery.Replace("{ProcedureName}", bindEntityService.StoredProcedureName);
            bindEntityQuery = bindEntityQuery.Replace("{SelectedColumns}", string.Join(",", selectedColumns));
            bindEntityQuery = bindEntityQuery.Replace("{SpParams}", string.Join(",\n", spParams));
            bindEntityQuery = bindEntityQuery.Replace("{Entity}", bindEntityService.EntityTableName);
            bindEntityQuery = bindEntityQuery.Replace("{Filters}", filters.Any() ? "WHERE \n\t\t" + string.Join(" AND\n\t\t", filters) : string.Empty);

            string dropQuery = string.Format("IF OBJECT_ID('[{0}].[{1}]', 'P') IS NOT NULL \n\t DROP PROCEDURE [{0}].[{1}];", "dbo", bindEntityService.StoredProcedureName);
            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, dropQuery);
            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, bindEntityQuery);

            var objBindEntityServiceInfo = HybridMapper.Map<BindEntityServiceViewModel, BindEntityServiceInfo>(bindEntityService);
            objBindEntityServiceInfo.ServiceId = service.Id;

            if (objBindEntityServiceInfo.Id == Guid.Empty)
                objBindEntityServiceInfo.Id = await _sql.InsertAsync<BindEntityServiceInfo>(objBindEntityServiceInfo);
            else
                await _sql.UpdateAsync<BindEntityServiceInfo>(objBindEntityServiceInfo);

            return objBindEntityServiceInfo.Id;
        }

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var items = await GetServicesAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = items,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            var items = JsonConvert.DeserializeObject<List<object>>(json);
            var services = JsonConvert.DeserializeObject<IEnumerable<BindEntityServiceInfo>>(items[0].ToString());
            var spScripts = JsonConvert.DeserializeObject<List<string>>(items[1].ToString());

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertServicesAndParamsAsync(services);

                foreach (var spScript in spScripts)
                {
                    await _sql.ExecuteSqlCommandTextAsync(context.UnitOfWork, spScript);
                }
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetServicesAsync(Guid scenarioId)
        {
            var bindEntityServices = new List<BindEntityServiceInfo>();
            var spScripts = new List<string>();

            var services = await _sql.GetItemsByColumnsAsync<ServiceInfo>(
                new string[2] { "ScenarioId", "ServiceType" },
                new
                {
                    ScenarioId = scenarioId,
                    ServiceType = "BindEntity"
                }
            );

            foreach (var service in services)
            {
                var bindEntityService = await _sql.GetByColumnAsync<BindEntityServiceInfo>("ServiceId", service.Id);
                if (bindEntityService != null)
                {
                    bindEntityServices.Add(bindEntityService);

                    spScripts.Add(await _sql.GetStoredProcedureScriptAsync(bindEntityService.StoredProcedureName));
                }
            }

            return new List<object>() { bindEntityServices, spScripts };
        }

        private async Task BulkInsertServicesAndParamsAsync(IEnumerable<BindEntityServiceInfo> services)
        {
            await _sql.BulkInsertAsync<BindEntityServiceInfo>(services);
        }

        #endregion
    }
}
