using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System.Net;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using System.Runtime;
using System.Text.RegularExpressions;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices
{
    public class DataRowService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelService;

        public DataRowService(IUnitOfWork unitOfWork, IRepositoryBase repository, IEntityService entityService, IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _entityService = entityService;
            _appModelService = appModelService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var dataRowService = await _repository.GetByColumnAsync<DataRowServiceInfo>("ServiceId", serviceId);

            return dataRowService != null
                ? HybridMapper.MapWithConfig<DataRowServiceInfo, DataRowServiceViewModel>(dataRowService,
                (src, dest) =>
                {
                    dest.Entities = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.EntityInfo>>(dataRowService.Entities);
                    dest.JoinRelationships = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.EntityJoinRelationInfo>>(dataRowService.JoinRelationships);
                    dest.ModelProperties = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.ModelPropertyInfo>>(dataRowService.ModelProperties);
                    dest.Filters = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.FilterItemInfo>>(dataRowService.Filters);
                    dest.Settings = TypeCasting.TryJsonCasting<IDictionary<string, object>>(dataRowService.Settings);
                })
            : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var entities = await _entityService.GetEntitiesViewModelAsync(scenarioId, 1, 1000, null, null, null, "EntityName");
            var appModels = await _appModelService.GetAppModelsAsync(scenarioId, 1, 1000, null, "ModelName");

            return new Dictionary<string, object>
            {
                { "Entities", entities.Items },
                { "AppModels", appModels.Items},
            };
        }

        public async Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson)
        {
            var service = parentService as ServiceViewModel;
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
                    spParams.Add(string.Format("{0} {1} {2}", serviceParam.ParamName, serviceParam.ParamType));
                }
            }

            if (dataRowService.JoinRelationships != null && dataRowService.JoinRelationships.Any())
            {
                var existsEntities = new List<string>();

                var firstJoin = dataRowService.JoinRelationships.First();

                string itemss = string.Format(" dbo.[{0}] as {1} ", firstJoin.LeftEntityTableName, firstJoin.LeftEntityAliasName);

                foreach (var relationship in dataRowService.JoinRelationships ?? Enumerable.Empty<Models.Database.EntityJoinRelationInfo>())
                {
                    itemss += string.Format(" {0} dbo.{1} as {2} on {3} ", relationship.JoinType, relationship.RightEntityTableName, relationship.RightEntityAliasName, relationship.JoinConditions);

                    existsEntities.Add(relationship.LeftEntityAliasName);
                    existsEntities.Add(relationship.RightEntityAliasName);
                }
                entities.Add(itemss);
            }
            else
            {
                foreach (var entity in dataRowService.Entities ?? Enumerable.Empty<Models.Database.EntityInfo>())
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

            var sqlCommand = new ExecuteSqlCommand(_unitOfWork);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", dataRowService.StoredProcedureName);
            await sqlCommand.ExecuteSqlCommandTextAsync(dropQuery);

            await sqlCommand.ExecuteSqlCommandTextAsync(dataSourceQuery);

            var objDataRowServiceInfo = HybridMapper.MapWithConfig<DataRowServiceViewModel, DataRowServiceInfo>(
                dataRowService, (src, dest) =>
                {
                    dest.Entities = JsonConvert.SerializeObject(dataRowService.Entities);
                    dest.JoinRelationships = JsonConvert.SerializeObject(dataRowService.JoinRelationships);
                    dest.ModelProperties = JsonConvert.SerializeObject(dataRowService.ModelProperties);
                    dest.Filters = JsonConvert.SerializeObject(dataRowService.Filters);
                    dest.Settings = JsonConvert.SerializeObject(dataRowService.Settings);
                });

            objDataRowServiceInfo.ServiceId = service.Id;

            if (objDataRowServiceInfo.Id == Guid.Empty)
                objDataRowServiceInfo.Id = await _repository.AddAsync<DataRowServiceInfo>(objDataRowServiceInfo);
            else
                await _repository.UpdateAsync<DataRowServiceInfo>(objDataRowServiceInfo);

            return objDataRowServiceInfo.Id;
        }
    }
}
