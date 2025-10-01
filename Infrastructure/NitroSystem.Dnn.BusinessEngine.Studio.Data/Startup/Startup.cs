using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new SqlConnection(DataProvider.Instance().ConnectionString + ";MultipleActiveResultSets=True;");
                connection.Open();
                return connection;
            });

            services.AddScoped<IRepositoryBase, RepositoryBase>();
            services.AddScoped<IDatabaseMetadataRepository, SqlDatabaseMetadataRepository>();
        }
    }
}
