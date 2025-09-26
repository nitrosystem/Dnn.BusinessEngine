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
using NitroSystem.Dnn.BusinessEngine.Data.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System.Net;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using System.Runtime;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices
{
    public class CustomQueryService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public CustomQueryService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var customQueryService = await _repository.GetByColumnAsync<CustomQueryServiceInfo>("ServiceId", serviceId);

            return customQueryService != null
                ? HybridMapper.MapWithConfig<CustomQueryServiceInfo, ViewModels.Database.LoginUserService>(customQueryService,
                (src, dest) =>
                {
                    dest.Query = DbUtil.GetSpScript(customQueryService.StoredProcedureName).Replace(
                        customQueryService.StoredProcedureName, "{ProcedureName}");
                    dest.Settings = TypeCasting.TryJsonCasting<IDictionary<string, object>>(customQueryService.Settings);
                })
            : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            await Task.Yield();

            return null;
        }

        public async Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson)
        {
            var service = parentService as ServiceViewModel;
            var customQueryService = JsonConvert.DeserializeObject<ViewModels.Database.LoginUserService>(extensionServiceJson);

            var customQuery = customQueryService.Query;
            customQuery = customQuery.Replace("{Schema}", "dbo");
            customQuery = customQuery.Replace("{ProcedureName}", customQueryService.StoredProcedureName);

            var sqlCommand = new ExecuteSqlCommand(_unitOfWork);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", customQueryService.StoredProcedureName);
            await sqlCommand.ExecuteSqlCommandTextAsync(dropQuery);

            await sqlCommand.ExecuteSqlCommandTextAsync(customQuery);

            var objCustomQueryServiceInfo = HybridMapper.MapWithConfig<ViewModels.Database.LoginUserService, CustomQueryServiceInfo>(
                customQueryService, (src, dest) =>
                {
                    dest.Settings = JsonConvert.SerializeObject(customQueryService.Settings);
                });

            objCustomQueryServiceInfo.ServiceId = service.Id;

            if (objCustomQueryServiceInfo.Id == Guid.Empty)
                objCustomQueryServiceInfo.Id = await _repository.AddAsync<CustomQueryServiceInfo>(objCustomQueryServiceInfo);
            else
                await _repository.UpdateAsync<CustomQueryServiceInfo>(objCustomQueryServiceInfo);

            return objCustomQueryServiceInfo.Id;
        }
    }
}
