using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services
{
    public class DeleteEntityRowService : IExtensionServiceFactory, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISql _sql;
        private readonly IEntityService _entityService;

        public DeleteEntityRowService(
            IUnitOfWork unitOfWork,
            ISql sql,
            IEntityService entityService)
        {
            _unitOfWork = unitOfWork;
            _sql = sql;
            _entityService = entityService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objDeleteEntityRowServiceInfo = await _sql.GetByColumnAsync<DeleteEntityRowServiceInfo>("ServiceId", serviceId);

            return objDeleteEntityRowServiceInfo != null
                ? HybridMapper.Map<DeleteEntityRowServiceInfo, DeleteEntityRowServiceViewModel>(objDeleteEntityRowServiceInfo,
                    (src, dest) => dest.Settings = dest.Settings ?? new Dictionary<string, object>())
                : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var entities = await _entityService.GetEntitiesListItemAsync(scenarioId, "EntityName");

            return new Dictionary<string, object>
            {
                { "Entities", entities }
            };
        }

        public async Task<Guid> SaveService(ServiceViewModel service, string extensionServiceJson)
        {
            var deleteEntityRowService = JsonConvert.DeserializeObject<DeleteEntityRowServiceViewModel>(extensionServiceJson);

            var deleteEntityRowQuery = deleteEntityRowService.BaseQuery;

            var spParams = new List<string>();
            var filters = new List<string>();

            if (service.Params != null)
            {
                foreach (var serviceParam in service.Params)
                {
                    spParams.Add(string.Format("{0} {1}", serviceParam.ParamName, serviceParam.ParamType));
                }
            }

            if (deleteEntityRowService.Conditions != null)
            {
                foreach (var group in deleteEntityRowService.Conditions.GroupBy(f => f.ConditionGroupName))
                {
                    var queryGroup = new List<string>();
                    foreach (var filter in group)
                    {
                        if (filter.Type == 1) queryGroup.Add(filter.CustomQuery);
                    }

                    if (queryGroup.Count > 0) filters.Add(string.Format("({0})", string.Join(" or ", queryGroup)));
                }
            }

            deleteEntityRowQuery = deleteEntityRowQuery.Replace("{Schema}", "dbo");
            deleteEntityRowQuery = deleteEntityRowQuery.Replace("{ProcedureName}", deleteEntityRowService.StoredProcedureName);
            deleteEntityRowQuery = deleteEntityRowQuery.Replace("{SpParams}", string.Join(",\n", spParams));
            deleteEntityRowQuery = deleteEntityRowQuery.Replace("{Entity}", string.Join(",", deleteEntityRowService.EntityTableName));
            deleteEntityRowQuery = deleteEntityRowQuery.Replace("{Conditions}", filters.Any() ? "WHERE \n\t\t" + string.Join(" and\n\t\t", filters) : string.Empty);



            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", deleteEntityRowService.StoredProcedureName);
            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, dropQuery);

            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, deleteEntityRowQuery);

            var objDeleteEntityRowServiceInfo = HybridMapper.Map<DeleteEntityRowServiceViewModel, DeleteEntityRowServiceInfo>(deleteEntityRowService);
            objDeleteEntityRowServiceInfo.ServiceId = service.Id;

            if (objDeleteEntityRowServiceInfo.Id == Guid.Empty)
                objDeleteEntityRowServiceInfo.Id = await _sql.InsertAsync<DeleteEntityRowServiceInfo>(objDeleteEntityRowServiceInfo);
            else
                await _sql.UpdateAsync<DeleteEntityRowServiceInfo>(objDeleteEntityRowServiceInfo);

            return objDeleteEntityRowServiceInfo.Id;
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
            var services = JsonConvert.DeserializeObject<IEnumerable<DeleteEntityRowServiceInfo>>(items[0].ToString());
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
            var deleteEntityRowServices = new List<DeleteEntityRowServiceInfo>();
            var spScripts = new List<string>();

            var services = await _sql.GetItemsByColumnsAsync<ServiceInfo>(
                new string[2] { "ScenarioId", "ServiceType" },
                new
                {
                    ScenarioId = scenarioId,
                    ServiceType = "DeleteEntityRow"
                }
            );

            foreach (var service in services)
            {
                var deleteEntityRowService = await _sql.GetByColumnAsync<DeleteEntityRowServiceInfo>("ServiceId", service.Id);
                if (deleteEntityRowService != null)
                {
                    deleteEntityRowServices.Add(deleteEntityRowService);

                    spScripts.Add(await _sql.GetStoredProcedureScriptAsync(deleteEntityRowService.StoredProcedureName));
                }
            }

            return new List<object>() { deleteEntityRowServices, spScripts };
        }

        private async Task BulkInsertServicesAndParamsAsync(IEnumerable<DeleteEntityRowServiceInfo> services)
        {
            await _sql.BulkInsertAsync<DeleteEntityRowServiceInfo>(services);
        }

        #endregion
    }
}
