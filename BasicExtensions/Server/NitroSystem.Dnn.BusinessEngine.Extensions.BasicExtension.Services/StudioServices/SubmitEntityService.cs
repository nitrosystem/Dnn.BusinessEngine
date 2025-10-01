using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices
{
    public class SubmitEntityService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IEntityService _entityService;

        public SubmitEntityService(IUnitOfWork unitOfWork, IRepositoryBase repository, IEntityService entityService)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _entityService = entityService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objSubmitEntityServiceInfo = await _repository.GetByColumnAsync<SubmitEntityServiceInfo>("ServiceId", serviceId);

            return objSubmitEntityServiceInfo != null
                ? HybridMapper.Map<SubmitEntityServiceInfo, SubmitEntityServiceViewModel>(objSubmitEntityServiceInfo)
                : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var entities = await _entityService.GetEntitiesViewModelAsync(scenarioId, 1, 1000, null, null, null, "EntityName");

            return new Dictionary<string, object>
            {
                { "Entities", entities.Items },
            };
        }

        public async Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson)
        {
            var service = parentService as ServiceViewModel;
            var submitEntityService = JsonConvert.DeserializeObject<SubmitEntityServiceViewModel>(extensionServiceJson);

            var submitEntityQuery = submitEntityService.BaseQuery;
            var entity = submitEntityService.Entity;

            var serviceParams = new List<ServiceParamInfo>();
            var spParams = new List<string>();
            var insertConditions = new List<string>();
            var insertColumns = new List<string>();
            var insertParams = new List<string>();
            var updateConditions = new List<string>();
            var updateParams = new List<string>();

            //Insert Action
            if (submitEntityService.ActionType == ActionType.InsertAndUpdate || submitEntityService.ActionType == ActionType.Insert)
            {
                //Insert Conditions
                foreach (var group in entity.InsertConditions.GroupBy(f => f.GroupName))
                {
                    var queryGroup = new List<string>();
                    foreach (var filter in group)
                    {
                        queryGroup.Add(filter.SqlQuery);
                    }

                    if (queryGroup.Count > 0) insertConditions.Add(string.Format("({0})", string.Join(" or ", queryGroup)));
                }

                //Insert Columns
                foreach (var column in entity.InsertColumns.Where(c => c.IsSelected && !string.IsNullOrEmpty(c.ColumnValue)))
                {
                    insertColumns.Add(column.ColumnName);
                }

                //Insert Params
                foreach (var column in entity.InsertColumns.Where(c => c.IsSelected && !string.IsNullOrEmpty(c.ColumnValue)))
                {
                    insertParams.Add(column.ColumnValue);
                }
            }

            //Update Action
            if (submitEntityService.ActionType == ActionType.InsertAndUpdate || submitEntityService.ActionType == ActionType.Update)
            {
                //Update Conditions
                foreach (var group in entity.UpdateConditions.GroupBy(f => f.GroupName))
                {
                    var queryGroup = new List<string>();
                    foreach (var filter in group)
                    {
                        queryGroup.Add(filter.SqlQuery);
                    }

                    if (queryGroup.Count > 0) updateConditions.Add(string.Format("({0})", string.Join(" or ", queryGroup)));
                }

                //Update Params
                foreach (var column in entity.UpdateColumns.Where(c => c.IsSelected && !string.IsNullOrEmpty(c.ColumnValue)))
                {
                    updateParams.Add(column.ColumnName + " = " + column.ColumnValue);
                }
            }

            //Service Params
            if (service.Params != null)
            {
                foreach (var serviceParam in service.Params)
                {
                    spParams.Add(serviceParam.ParamName + " " + serviceParam.ParamType);
                }
            }

            submitEntityQuery = submitEntityQuery.Replace("{InsertConditions}", string.Join(" and ", insertConditions));
            submitEntityQuery = submitEntityQuery.Replace("{InsertColumns}", string.Join(",\n\t\t\t\t", insertColumns));
            submitEntityQuery = submitEntityQuery.Replace("{InsertParams}", string.Join(",\n\t\t\t\t", insertParams));
            submitEntityQuery = submitEntityQuery.Replace("{UpdateConditions}", string.Join(",\n\t\t\t\t", updateConditions));
            submitEntityQuery = submitEntityQuery.Replace("{UpdateParams}", string.Join(",\n\t\t\t\t", updateParams));
            submitEntityQuery = submitEntityQuery.Replace("{PrimaryKeyParam}", entity.PrimaryKeyParam);
            submitEntityQuery = submitEntityQuery.Replace("{TableName}", entity.TableName);
            submitEntityQuery = submitEntityQuery.Replace("{Schema}", "dbo");
            submitEntityQuery = submitEntityQuery.Replace("{SpParams}", string.Join(",\n", spParams));
            submitEntityQuery = submitEntityQuery.Replace("{ProcedureName}", submitEntityService.StoredProcedureName);

            var sqlCommand = new ExecuteSqlCommand(_unitOfWork);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", submitEntityService.StoredProcedureName);
            await sqlCommand.ExecuteSqlCommandTextAsync(dropQuery);

            await sqlCommand.ExecuteSqlCommandTextAsync(submitEntityQuery);

            var objSubmitEntityServiceInfo = HybridMapper.Map<SubmitEntityServiceViewModel, SubmitEntityServiceInfo>(submitEntityService);
            objSubmitEntityServiceInfo.ServiceId = service.Id;

            if (objSubmitEntityServiceInfo.Id == Guid.Empty)
                objSubmitEntityServiceInfo.Id = await _repository.AddAsync<SubmitEntityServiceInfo>(objSubmitEntityServiceInfo);
            else
                await _repository.UpdateAsync<SubmitEntityServiceInfo>(objSubmitEntityServiceInfo);

            return objSubmitEntityServiceInfo.Id;
        }
    }
}
