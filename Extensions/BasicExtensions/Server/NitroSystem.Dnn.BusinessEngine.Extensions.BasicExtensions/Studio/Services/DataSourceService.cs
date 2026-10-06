using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    public class DataSourceService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISql _sql;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelService;

        public DataSourceService(
            IUnitOfWork unitOfWork,
            ISql sql,
            IEntityService entityService,
            IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _entityService = entityService;
            _appModelService = appModelService;
            _sql = sql;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objDataSourceServiceInfo = await _sql.GetByColumnAsync<DataSourceServiceInfo>("ServiceId", serviceId);

            return objDataSourceServiceInfo != null
                ? HybridMapper.Map<DataSourceServiceInfo, DataSourceServiceViewModel>(objDataSourceServiceInfo, 
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
            var dataSourceService = JsonConvert.DeserializeObject<DataSourceServiceViewModel>(extensionServiceJson);

            var dataSourceQuery = dataSourceService.BaseQuery;

            var spParams = new List<string>();
            var selectedColumns = new List<string>();
            var entities = new List<string>();
            var filters = new List<string>();
            var sortItems = new List<string>();

            var pagingRegex = new Regex(@"{EnablePaging\}([\s\S]+?)\{\/EnablePaging\}");
            dataSourceQuery = dataSourceService.EnablePaging ? pagingRegex.Replace(dataSourceQuery, "$1") : pagingRegex.Replace(dataSourceQuery, "");

            foreach (var property in dataSourceService.ModelProperties)
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

            if (dataSourceService.JoinRelationships != null && dataSourceService.JoinRelationships.Any())
            {
                var existsEntities = new List<string>();

                var firstJoin = dataSourceService.JoinRelationships.First();

                string itemss = string.Format(" dbo.[{0}] as {1} ", firstJoin.LeftEntityTableName, firstJoin.LeftEntityAliasName);

                foreach (var relationship in dataSourceService.JoinRelationships ?? Enumerable.Empty<EntityJoinRelationViewModel>())
                {
                    itemss += string.Format(" {0} dbo.{1} as {2} on {3} ", relationship.JoinType, relationship.RightEntityTableName, relationship.RightEntityAliasName, relationship.JoinConditions);

                    existsEntities.Add(relationship.LeftEntityAliasName);
                    existsEntities.Add(relationship.RightEntityAliasName);
                }
                entities.Add(itemss);
            }
            else
            {
                foreach (var entity in dataSourceService.Entities ?? Enumerable.Empty<EntityViewModel>())
                {
                    string item = string.Format(" dbo.[{0}] as {1} ", entity.TableName, entity.AliasName);
                    entities.Add(item);
                }
            }

            if (dataSourceService.Filters != null)
            {
                foreach (var group in dataSourceService.Filters.GroupBy(f => f.ConditionGroupName))
                {
                    var queryGroup = new List<string>();
                    foreach (var filter in group)
                    {
                        if (filter.Type == 1) queryGroup.Add(filter.CustomQuery);
                    }

                    if (queryGroup.Count > 0) filters.Add(string.Format("({0})", string.Join(" or ", queryGroup)));
                }
            }

            foreach (var sortItem in dataSourceService.SortItems ?? Enumerable.Empty<SortItemViewModel>())
            {
                if (sortItem.Type == 0)
                {
                    sortItems.Add(string.Format("{0}.{1} {2}", sortItem.EntityAliasName, sortItem.ColumnName, sortItem.SortType));
                }
                else if (sortItem.Type == 1)
                    sortItems.Add(sortItem.CustomColumn);
            }

            if (dataSourceService.EnablePaging)
            {
                dataSourceQuery = dataSourceQuery.Replace("{PagingQuery}", "OFFSET (" + dataSourceService.PageIndexParam + " - 1) * " + dataSourceService.PageSizeParam + " ROWS FETCH NEXT " + dataSourceService.PageSizeParam + " ROWS ONLY OPTION (RECOMPILE);");
            }
            else
            {
                dataSourceQuery = dataSourceQuery.Replace("{PagingQuery}", string.Empty);
            }

            dataSourceQuery = dataSourceQuery.Replace("{Schema}", "dbo");
            dataSourceQuery = dataSourceQuery.Replace("{ProcedureName}", dataSourceService.StoredProcedureName);
            dataSourceQuery = dataSourceQuery.Replace("{SpParams}", string.Join(",\n", spParams));
            dataSourceQuery = dataSourceQuery.Replace("{SelectedColumns}", string.Join(",", selectedColumns));
            dataSourceQuery = dataSourceQuery.Replace("{Entities}", string.Join(",\n", entities));
            dataSourceQuery = dataSourceQuery.Replace("{Filters}", filters.Any() ? "WHERE \n\t\t" + string.Join(" and\n\t\t", filters) : string.Empty);
            dataSourceQuery = dataSourceQuery.Replace("{SortingQuery}",
             (sortItems.Any()
               ? "ORDER BY \n\t\t" + string.Join(",", sortItems)
               : string.Empty));

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", dataSourceService.StoredProcedureName);
            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, dropQuery);

            await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, dataSourceQuery);

            var objDataSourceServiceInfo = HybridMapper.Map<DataSourceServiceViewModel, DataSourceServiceInfo>(dataSourceService);
            objDataSourceServiceInfo.ServiceId = service.Id;

            if (objDataSourceServiceInfo.Id == Guid.Empty)
                objDataSourceServiceInfo.Id = await _sql.InsertAsync<DataSourceServiceInfo>(objDataSourceServiceInfo);
            else
                await _sql.UpdateAsync<DataSourceServiceInfo>(objDataSourceServiceInfo);

            return objDataSourceServiceInfo.Id;
        }
    }
}
