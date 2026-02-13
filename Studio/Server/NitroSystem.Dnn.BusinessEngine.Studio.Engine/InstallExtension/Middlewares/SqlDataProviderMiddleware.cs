using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Extension;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Enums;


namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares
{
    public class SqlDataProviderMiddleware : IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>
    {
        private readonly IExtensionService _service;
        private readonly IExecuteSqlCommand _sqlCommand;

        public SqlDataProviderMiddleware(IUnitOfWork unitOfWork, IExtensionService service, IExecuteSqlCommand sqlCommand)
        {
            _service = service;
            _sqlCommand = sqlCommand;
        }

        public async Task<InstallExtensionResponse> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<InstallExtensionResponse>> next, Action<string, string, double> progress = null)
        {
            var unzipedPath = context.Get<string>("UnzipedPath");
            var unitOfWork = context.Get<IUnitOfWork>("UnitOfWork");

            try
            {
                var extension = HybridMapper.Map<ExtensionManifest, ExtensionViewModel>(request.Manifest,
                    (src, dest) =>
                    {
                        dest.Owner = src.Owner.OwnerName;
                        dest.Url = src.Owner.Url;
                        dest.Email = src.Owner.Email;
                    });

                request.Manifest.Id = await _service.SaveExtensionAsync(extension, request.Manifest.IsNewExtension);

                var sqlProviderFolder = Path.Combine(unzipedPath, "sql-providers");

                StringBuilder queries = new StringBuilder();

                foreach (var item in (request.Manifest.SqlProviders ?? Enumerable.Empty<ExtensionSqlProvider>())
                    .Where(p => p.Type == SqlProviderType.Install && IsValidVersion(context.Get<string>("CurrentVersion"), p.Version)))
                {
                    var query = await FileUtil.GetFileContentAsync(Path.Combine(sqlProviderFolder, item.File));
                    queries.AppendLine(query);
                    queries.AppendLine(Environment.NewLine);
                }

                string sqlCommands = queries.ToString();
                if (!string.IsNullOrEmpty(sqlCommands))
                {
                    sqlCommands = sqlCommands.Replace("[EXTENSIONID]", request.Manifest.Id.Value.ToString());

                    await _sqlCommand.ExecuteSqlCommandTextAsync(unitOfWork, sqlCommands);
                }
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();

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
