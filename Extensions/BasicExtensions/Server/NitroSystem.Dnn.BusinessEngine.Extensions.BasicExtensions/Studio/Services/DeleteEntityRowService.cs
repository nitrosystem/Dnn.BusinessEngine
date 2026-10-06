using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Tables;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services
{
    public class DeleteEntityRowService : IExtensionServiceFactory
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
    }
}
