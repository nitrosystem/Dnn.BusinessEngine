using System.Data;
using System.Threading.Tasks;
using Dapper;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database
{
    public class BindEntityService : IBindEntityService
    {
        private readonly IDbConnection _connection;
        private readonly IRepositoryBase _repository;
        private readonly ITypeLoaderFactory _typeLoaderFactory;

        public BindEntityService(IDbConnection connection, IRepositoryBase repository, ITypeLoaderFactory typeLoaderFactory)
        {
            _connection = connection;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            _repository = repository;
            _typeLoaderFactory = typeLoaderFactory;
        }

        public async Task<object> GetBindEntityService(ActionDto action, PortalSettings portalSettings)
        {
            var spParams = DbShared.FillSqlParams(action.Params);
            var data = await _repository.GetByColumnAsync<BindEntityServiceView>("ServiceId", action.ServiceId.Value);
            var type = _typeLoaderFactory.GetTypeFromAssembly(data.TypeRelativePath, data.TypeFullName, data.ScenarioName, portalSettings.HomeSystemDirectoryMapPath);

            var result = await _connection.QuerySingleAsync(type, data.StoredProcedureName, spParams,
                commandType: CommandType.StoredProcedure, commandTimeout: int.MaxValue);

            return result;
        }
    }
}
