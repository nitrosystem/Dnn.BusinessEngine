using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Net.WebSockets;
using System;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using DotNetNuke.Data;


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
        }
    }
}
