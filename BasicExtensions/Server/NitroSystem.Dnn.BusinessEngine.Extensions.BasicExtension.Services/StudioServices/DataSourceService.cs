using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DB.Entities;
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
using NitroSystem.Dnn.BusinessEngine.Core.General;
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
    public class DataSourceService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelService;

        public DataSourceService(IUnitOfWork unitOfWork, IRepositoryBase repository, IEntityService entityService, IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _entityService = entityService;
            _appModelService = appModelService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var dataSourceService = await _repository.GetByColumnAsync<DataSourceServiceInfo>("ServiceId", serviceId);

            return dataSourceService != null
                ? HybridMapper.MapWithConfig<DataSourceServiceInfo, DataSourceServiceViewModel>(dataSourceService,
                (src, dest) =>
                {
                    dest.Entities = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.EntityInfo>>(dataSourceService.Entities);
                    dest.JoinRelationships = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.EntityJoinRelationInfo>>(dataSourceService.JoinRelationships);
                    dest.ModelProperties = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.ModelPropertyInfo>>(dataSourceService.ModelProperties);
                    dest.Filters = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.FilterItemInfo>>(dataSourceService.Filters);
                    dest.SortItems = TypeCasting.TryJsonCasting<IEnumerable<Models.Database.SortItemInfo>>(dataSourceService.SortItems);
                    dest.Settings = TypeCasting.TryJsonCasting<IDictionary<string, object>>(dataSourceService.Settings);
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
            var dataSourceService = JsonConvert.DeserializeObject<DataSourceServiceViewModel>(extensionServiceJson);

            var dataSourceQuery = dataSourceService.BaseQuery;

            var spParams = new List<string>();
            var selectedColumns = new List<string>();
            var entities = new List<string>();
            var filters = new List<string>();
            var sortItems = new List<string>();

            var pagingRegex = new Regex("(\\[STARTPAGING\\])(.*?)(\\[ENDPAGING\\])", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);

            dataSourceQuery = dataSourceService.EnablePaging ? pagingRegex.Replace(dataSourceQuery, "\t$2") : pagingRegex.Replace(dataSourceQuery, "");

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
                    spParams.Add(string.Format("{0} {1} {2}", serviceParam.ParamName, serviceParam.ParamType));
                }
            }

            if (dataSourceService.JoinRelationships != null && dataSourceService.JoinRelationships.Any())
            {
                var existsEntities = new List<string>();

                var firstJoin = dataSourceService.JoinRelationships.First();

                string itemss = string.Format(" dbo.[{0}] as {1} ", firstJoin.LeftEntityTableName, firstJoin.LeftEntityAliasName);

                foreach (var relationship in dataSourceService.JoinRelationships ?? Enumerable.Empty<Models.Database.EntityJoinRelationInfo>())
                {
                    itemss += string.Format(" {0} dbo.{1} as {2} on {3} ", relationship.JoinType, relationship.RightEntityTableName, relationship.RightEntityAliasName, relationship.JoinConditions);

                    existsEntities.Add(relationship.LeftEntityAliasName);
                    existsEntities.Add(relationship.RightEntityAliasName);
                }
                entities.Add(itemss);
            }
            else
            {
                foreach (var entity in dataSourceService.Entities ?? Enumerable.Empty<Models.Database.EntityInfo>())
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

            foreach (var sortItem in dataSourceService.SortItems ?? Enumerable.Empty<Models.Database.SortItemInfo>())
            {
                if (sortItem.Type == 0)
                {
                    sortItems.Add(string.Format("{0}.[{1}] {2}", sortItem.EntityAliasName, sortItem.ColumnName, sortItem.SortType));
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
            dataSourceQuery = dataSourceQuery.Replace("{SortingQuery}", "ORDER BY \n\t\t" + string.Join(",", sortItems));
            dataSourceQuery = dataSourceQuery.Replace("{TotalCountColumnName}", dataSourceService.TotalCountColumnName);

            var sqlCommand = new ExecuteSqlCommand(_unitOfWork);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", dataSourceService.StoredProcedureName);
            await sqlCommand.ExecuteSqlCommandTextAsync(dropQuery);

            await sqlCommand.ExecuteSqlCommandTextAsync(dataSourceQuery);

            var objDataSourceServiceInfo = HybridMapper.MapWithConfig<DataSourceServiceViewModel, DataSourceServiceInfo>(
                dataSourceService, (src, dest) =>
                {
                    dest.Entities = JsonConvert.SerializeObject(dataSourceService.Entities);
                    dest.JoinRelationships = JsonConvert.SerializeObject(dataSourceService.JoinRelationships);
                    dest.ModelProperties = JsonConvert.SerializeObject(dataSourceService.ModelProperties);
                    dest.Filters = JsonConvert.SerializeObject(dataSourceService.Filters);
                    dest.SortItems = JsonConvert.SerializeObject(dataSourceService.SortItems);
                    dest.Settings = JsonConvert.SerializeObject(dataSourceService.Settings);
                });

            objDataSourceServiceInfo.ServiceId = service.Id;

            if (objDataSourceServiceInfo.Id == Guid.Empty)
                objDataSourceServiceInfo.Id = await _repository.AddAsync<DataSourceServiceInfo>(objDataSourceServiceInfo);
            else
                await _repository.UpdateAsync<DataSourceServiceInfo>(objDataSourceServiceInfo);

            return objDataSourceServiceInfo.Id;
        }
    }
}
