using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Shared.Models.Shared;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System.Net;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using System.Runtime;
using NitroSystem.Dnn.BusinessEngine.Shared.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices
{
    public class BindEntityService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelServices;

        public BindEntityService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IRepositoryBase repository,
            IEntityService entityService,
            IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
            _entityService = entityService;
            _appModelServices = appModelService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var bindEntityService = await _repository.GetByColumnAsync<BindEntityServiceInfo>("ServiceId", serviceId);

            return bindEntityService != null
                ? HybridMapper.MapWithConfig<BindEntityServiceInfo, BindEntityServiceViewModel>(bindEntityService,
                (src, dest) =>
                {
                    dest.ModelProperties = TypeCasting.TryJsonCasting<IEnumerable<ModelPropertyInfo>>(bindEntityService.ModelProperties);
                    dest.Filters = TypeCasting.TryJsonCasting<IEnumerable<FilterItemInfo>>(bindEntityService.Filters);
                    dest.Settings = TypeCasting.TryJsonCasting<IDictionary<string, object>>(bindEntityService.Settings);
                })
            : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var entities = await _entityService.GetEntitiesViewModelAsync(scenarioId, 1, 1000, null, null, null, "EntityName");
            var appModels = await _appModelServices.GetAppModelsAsync(scenarioId, 1, 1000, null, "ModelName");

            return new Dictionary<string, object>
            {
                { "Entities", entities.Items },
                { "AppModels", appModels.Items }
            };
        }

        public async Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson)
        {
            var service = parentService as ServiceViewModel;
            var bindEntityService = JsonConvert.DeserializeObject<BindEntityServiceViewModel>(extensionServiceJson);

            var bindEntityQuery = bindEntityService.BaseQuery;

            var spParams = new List<string>();
            var selectedColumns = new List<string>();
            var filters = new List<string>();

            foreach (var property in bindEntityService.ModelProperties)
            {
                if (!property.IsSelected) continue;

                selectedColumns.Add(string.Format("[{0}] as [{1}]", property.ColumnName, property.PropertyName));
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

                    if (queryGroup.Count > 0) filters.Add(string.Format("({0})", string.Join(" or ", queryGroup)));
                }
            }

            bindEntityQuery = bindEntityQuery.Replace("{Schema}", "dbo");
            bindEntityQuery = bindEntityQuery.Replace("{ProcedureName}", bindEntityService.StoredProcedureName);
            bindEntityQuery = bindEntityQuery.Replace("{SelectedColumns}", string.Join(",", selectedColumns));
            bindEntityQuery = bindEntityQuery.Replace("{SpParams}", string.Join(",\n", spParams));
            bindEntityQuery = bindEntityQuery.Replace("{Entity}", string.Join(",", bindEntityService.EntityTableName));
            bindEntityQuery = bindEntityQuery.Replace("{Filters}", filters.Any() ? "WHERE \n\t\t" + string.Join(" and\n\t\t", filters) : string.Empty);

            var sqlCommand = new ExecuteSqlCommand(_unitOfWork);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", bindEntityService.StoredProcedureName);
            await sqlCommand.ExecuteSqlCommandTextAsync(dropQuery);

            await sqlCommand.ExecuteSqlCommandTextAsync(bindEntityQuery);

            var objBindEntityServiceInfo = HybridMapper.MapWithConfig<BindEntityServiceViewModel, BindEntityServiceInfo>(
                bindEntityService, (src, dest) =>
                {
                    dest.ModelProperties = JsonConvert.SerializeObject(bindEntityService.ModelProperties);
                    dest.Filters = JsonConvert.SerializeObject(bindEntityService.Filters);
                    dest.Settings = JsonConvert.SerializeObject(bindEntityService.Settings);
                });

            objBindEntityServiceInfo.ServiceId = service.Id;

            if (objBindEntityServiceInfo.Id == Guid.Empty)
                objBindEntityServiceInfo.Id = await _repository.AddAsync<BindEntityServiceInfo>(objBindEntityServiceInfo);
            else
                await _repository.UpdateAsync<BindEntityServiceInfo>(objBindEntityServiceInfo);

            return objBindEntityServiceInfo.Id;
        }
    }
}
