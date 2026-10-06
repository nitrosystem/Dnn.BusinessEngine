using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Data;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.ORM.Dapper.Startup
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

            services.AddScoped<ISql, Sql.Sql>();
        }
    }
}
