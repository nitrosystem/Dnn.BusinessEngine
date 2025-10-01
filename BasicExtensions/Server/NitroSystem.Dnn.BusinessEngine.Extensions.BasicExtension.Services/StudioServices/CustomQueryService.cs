using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices
{
    public class CustomQueryService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;
        private readonly IDatabaseMetadataRepository _databaseMetadata;


        public CustomQueryService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository, IDatabaseMetadataRepository databaseMetadata)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
            _databaseMetadata = databaseMetadata;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objCustomQueryServiceInfo = await _repository.GetByColumnAsync<CustomQueryServiceInfo>("ServiceId", serviceId);

            return objCustomQueryServiceInfo != null
                ? await HybridMapper.MapAsync<CustomQueryServiceInfo, CustomQueryServiceViewModel>(objCustomQueryServiceInfo,
                async (src, dest) =>
                {
                    dest.Query = (await _databaseMetadata.GetSpScript(objCustomQueryServiceInfo.StoredProcedureName))
                    .Replace(objCustomQueryServiceInfo.StoredProcedureName, "{ProcedureName}");
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
            var customQueryService = JsonConvert.DeserializeObject<CustomQueryServiceViewModel>(extensionServiceJson);

            var customQuery = customQueryService.Query;
            customQuery = customQuery.Replace("{Schema}", "dbo");
            customQuery = customQuery.Replace("{ProcedureName}", customQueryService.StoredProcedureName);

            var sqlCommand = new ExecuteSqlCommand(_unitOfWork);

            string dropQuery = string.Format("IF OBJECT_ID('{0}.{1}', 'P') IS NOT NULL \n\t DROP PROCEDURE {0}.{1};", "dbo", customQueryService.StoredProcedureName);
            await sqlCommand.ExecuteSqlCommandTextAsync(dropQuery);

            await sqlCommand.ExecuteSqlCommandTextAsync(customQuery);

            var objCustomQueryServiceInfo = HybridMapper.Map<CustomQueryServiceViewModel, CustomQueryServiceInfo>(customQueryService);
            objCustomQueryServiceInfo.ServiceId = service.Id;

            if (objCustomQueryServiceInfo.Id == Guid.Empty)
                objCustomQueryServiceInfo.Id = await _repository.AddAsync<CustomQueryServiceInfo>(objCustomQueryServiceInfo);
            else
                await _repository.UpdateAsync<CustomQueryServiceInfo>(objCustomQueryServiceInfo);

            return objCustomQueryServiceInfo.Id;
        }
    }
}
