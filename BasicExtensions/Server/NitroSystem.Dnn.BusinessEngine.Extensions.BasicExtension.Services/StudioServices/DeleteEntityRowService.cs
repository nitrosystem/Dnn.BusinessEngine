using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices
{
    public class DeleteEntityRowService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IEntityService _entityService;

        public DeleteEntityRowService(IUnitOfWork unitOfWork, IRepositoryBase repository, IEntityService entityService)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _entityService = entityService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objDeleteEntityRowServiceInfo = await _repository.GetByColumnAsync<DeleteEntityRowServiceInfo>("ServiceId", serviceId);

            return objDeleteEntityRowServiceInfo != null
                ? HybridMapper.Map<DeleteEntityRowServiceInfo, DeleteEntityRowServiceViewModel>(objDeleteEntityRowServiceInfo)
                : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var entities = await _entityService.GetEntitiesViewModelAsync(scenarioId, 1, 1000, null, null, null, "EntityName");

            return new Dictionary<string, object>
            {
                { "Entities", entities.Items }
            };
        }

        public async Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson)
        {
            var service = parentService as ServiceViewModel;
            var deleteEntityRowService = JsonConvert.DeserializeObject<DeleteEntityRowServiceViewModel>(extensionServiceJson);

            var deleteEntityRowQuery = deleteEntityRowService.BaseQuery;

            var spParams = new List<string>();
            var filters = new List<string>();

            if (service.Params != null)
            {
                foreach (var serviceParam in service.Params)
                {
                    spParams.Add(string.Format("{0} {1} {2}", serviceParam.ParamName, serviceParam.ParamType));
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

            var sqlCommand = new ExecuteSqlCommand(_unitOfWork);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", deleteEntityRowService.StoredProcedureName);
            await sqlCommand.ExecuteSqlCommandTextAsync(dropQuery);

            await sqlCommand.ExecuteSqlCommandTextAsync(deleteEntityRowQuery);

            var objDeleteEntityRowServiceInfo = HybridMapper.Map<DeleteEntityRowServiceViewModel, DeleteEntityRowServiceInfo>(deleteEntityRowService);
            objDeleteEntityRowServiceInfo.ServiceId = service.Id;

            if (objDeleteEntityRowServiceInfo.Id == Guid.Empty)
                objDeleteEntityRowServiceInfo.Id = await _repository.AddAsync<DeleteEntityRowServiceInfo>(objDeleteEntityRowServiceInfo);
            else
                await _repository.UpdateAsync<DeleteEntityRowServiceInfo>(objDeleteEntityRowServiceInfo);

            return objDeleteEntityRowServiceInfo.Id;
        }
    }
}
