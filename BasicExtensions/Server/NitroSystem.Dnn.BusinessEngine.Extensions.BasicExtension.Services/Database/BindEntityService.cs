using Dapper;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using DotNetNuke.Entities.Portals;
using System.Web;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Views;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader;

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
			var type = _typeLoaderFactory.GetTypeFromAssembly(data.TypeRelativePath, data.TypeFullName, data.ScenarioName, portalSettings);

			var result = await _connection.QuerySingleAsync(type, data.StoredProcedureName, spParams,
				commandType: CommandType.StoredProcedure, commandTimeout: int.MaxValue);

			return result;
		}
	}
}
