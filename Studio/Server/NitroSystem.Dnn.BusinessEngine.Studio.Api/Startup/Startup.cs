using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Net.WebSockets;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using System;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;


namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IExecuteSqlCommand, ExecuteSqlCommand>();

            services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new SqlConnection(DotNetNuke.Data.DataProvider.Instance().ConnectionString + ";MultipleActiveResultSets=True;");
                connection.Open();
                return connection;
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepositoryBase, RepositoryBase>();

            services.AddScoped<IGlobalService, GlobalService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IViewModelService, ViewModelService>();
            services.AddScoped<IServiceFactory, ServiceFactory>();
            services.AddScoped<IDefinedListService, DefinedListService>();

            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ITemplateService, TemplateService>();

            services.AddScoped<IBuildModuleService, BuildModuleService>();
            services.AddScoped<IBuildModuleLayout, BuildModuleLayout>();
            services.AddScoped<IModuleBuildLockService, ModuleBuildLockService>();

            services.AddTransient<IResourceMachine, ResourceMachine>();

            //GlobalConfiguration.Configuration.Filters.Add(new BasicAuthenticationAttribute());
        }
    }
}
