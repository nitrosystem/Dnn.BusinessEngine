using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares
{
    public class SqlDataProviderMiddleware : IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExtensionService _service;
        private readonly IExecuteSqlCommand _sqlCommand;
        public SqlDataProviderMiddleware(IUnitOfWork unitOfWork, IExtensionService service, IExecuteSqlCommand sqlCommand)
        {
            _unitOfWork = unitOfWork;
            _service = service;
            _sqlCommand = sqlCommand;
        }

        public async Task<EngineResult<InstallExtensionResponse>> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<EngineResult<InstallExtensionResponse>>> next)
        {
            var ctx = context as InstallExtensionContext;

            ctx.UnitOfWork = _unitOfWork;
            ctx.UnitOfWork.BeginTransaction();

            try
            {
                ctx.ExtensionManifest.Id = await _service.SaveExtensionAsync(ctx.ExtensionManifest, ctx.UnitOfWork);

                var unzipedPath = ctx.UnzipedPath;
                var sqlProviderFolder = Path.Combine(unzipedPath, "sql-providers");

                StringBuilder queries = new StringBuilder();

                foreach (var item in (ctx.ExtensionManifest.SqlProviders ?? Enumerable.Empty<ExtensionSqlProvider>())
                    .Where(p => p.Type == SqlProviderType.Install && IsValidVersion(ctx.CurrentVersion, p.Version)))
                {
                    var query = await FileUtil.GetFileContentAsync(Path.Combine(sqlProviderFolder, item.File));
                    queries.AppendLine(query);
                    queries.AppendLine(Environment.NewLine);
                }

                string sqlCommands = queries.ToString();
                if (!string.IsNullOrEmpty(sqlCommands))
                {
                    sqlCommands = sqlCommands.Replace("[EXTENSIONID]", ctx.ExtensionManifest.Id.Value.ToString());

                    await _sqlCommand.ExecuteSqlCommandTextAsync(ctx.UnitOfWork, sqlCommands);
                }
            }
            catch (Exception ex)
            {
                ctx.UnitOfWork.Rollback();

                throw ex;
            }

            var result = await next();
            return result;
        }

        private bool IsValidVersion(string currentVersion, string newVersion)
        {
            return string.IsNullOrEmpty(currentVersion) || new Version(currentVersion) < new Version(newVersion);
        }
    }
}
