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
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services
{
    public class DataRowService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISql _sql;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelService;

        public DataRowService(
            IUnitOfWork unitOfWork,
            ISql sql,
            IEntityService entityService,
            IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _sql = sql;
            _entityService = entityService;
            _appModelService = appModelService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var dataRowService = await _sql.GetByColumnAsync<DataRowServiceInfo>("ServiceId", serviceId);

            return dataRowService != null
                ? HybridMapper.Map<DataRowServiceInfo, DataRowServiceViewModel>(dataRowService,
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
                { "AppModels", appModels},
            };
        }

        public async Task<Guid> SaveService(ServiceViewModel service, string extensionServiceJson)
        {
            var dataRowService = JsonConvert.DeserializeObject<DataRowServiceViewModel>(extensionServiceJson);

            var dataSourceQuery = dataRowService.BaseQuery;

            var spParams = new List<string>();
            var selectedColumns = new List<string>();
            var entities = new List<string>();
            var filters = new List<string>();
            var sortItems = new List<string>();

            foreach (var property in dataRowService.ModelProperties)
            {
                if (!property.IsSelected) continue;

                string value = string.Empty;

                if (property.ValueType == "DataSource" && !string.IsNullOrEmpty(property.EntityAliasName) && !string.IsNullOrEmpty(property.ColumnName))
                    value = property.EntityAliasName + "." + property.ColumnName;
                else if (property.ValueType == "Custom" && !string.IsNullOrEmpty(property.Value))
                    value = property.Value;

                if (!string.IsNullOrEmpty(value)) selectedColumns.Add(string.Format("{0} as [{1}]", value, property.PropertyName));
            }

            if (service.Params != null)
            {
                foreach (var serviceParam in service.Params)
                {
                    spParams.Add(string.Format("{0} {1}", serviceParam.ParamName, serviceParam.ParamType));
                }
            }

            if (dataRowService.JoinRelationships != null && dataRowService.JoinRelationships.Any())
            {
                var existsEntities = new List<string>();

                var firstJoin = dataRowService.JoinRelationships.First();

                string itemss = string.Format(" dbo.[{0}] as {1} ", firstJoin.LeftEntityTableName, firstJoin.LeftEntityAliasName);

                foreach (var relationship in dataRowService.JoinRelationships ?? Enumerable.Empty<EntityJoinRelationViewModel>())
                {
                    itemss += string.Format(" {0} dbo.{1} as {2} on {3} ", relationship.JoinType, relationship.RightEntityTableName, relationship.RightEntityAliasName, relationship.JoinConditions);

                    existsEntities.Add(relationship.LeftEntityAliasName);
                    existsEntities.Add(relationship.RightEntityAliasName);
                }
                entities.Add(itemss);
            }
            else
            {
                foreach (var entity in dataRowService.Entities ?? Enumerable.Empty<EntityViewModel>())
                {
                    string item = string.Format(" dbo.[{0}] as {1} ", entity.TableName, entity.AliasName);
                    entities.Add(item);
                }
            }

            if (dataRowService.Filters != null)
            {
                foreach (var group in dataRowService.Filters.GroupBy(f => f.ConditionGroupName))
                {
                    var queryGroup = new List<string>();
                    foreach (var filter in group)
                    {
                        if (filter.Type == 1) queryGroup.Add(filter.CustomQuery);
                    }

                    if (queryGroup.Count > 0) filters.Add(string.Format("({0})", string.Join(" or ", queryGroup)));
                }
            }

            dataSourceQuery = dataSourceQuery.Replace("{Schema}", "dbo");
            dataSourceQuery = dataSourceQuery.Replace("{ProcedureName}", dataRowService.StoredProcedureName);
            dataSourceQuery = dataSourceQuery.Replace("{SpParams}", string.Join(",\n", spParams));
            dataSourceQuery = dataSourceQuery.Replace("{SelectedColumns}", string.Join(",", selectedColumns));
            dataSourceQuery = dataSourceQuery.Replace("{Entities}", string.Join(",\n", entities));
            dataSourceQuery = dataSourceQuery.Replace("{Filters}", filters.Any() ? "WHERE \n\t\t" + string.Join(" and\n\t\t", filters) : string.Empty);



            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", dataRowService.StoredProcedureName);
            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, dropQuery);

            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, dataSourceQuery);

            var objDataRowServiceInfo = HybridMapper.Map<DataRowServiceViewModel, DataRowServiceInfo>(dataRowService);
            objDataRowServiceInfo.ServiceId = service.Id;

            if (objDataRowServiceInfo.Id == Guid.Empty)
                objDataRowServiceInfo.Id = await _sql.InsertAsync<DataRowServiceInfo>(objDataRowServiceInfo);
            else
                await _sql.UpdateAsync<DataRowServiceInfo>(objDataRowServiceInfo);

            return objDataRowServiceInfo.Id;
        }
    }
}
