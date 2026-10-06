using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;


namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares
{
    public class SqlDataProviderMiddleware : IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExtensionService _service;
        private readonly ISql _sql;

        public SqlDataProviderMiddleware(IUnitOfWork unitOfWork, IExtensionService service, ISql sql)
        {
            _unitOfWork = unitOfWork;
            _service = service;
            _sql = sql;
        }

        public async Task<InstallExtensionResponse> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<InstallExtensionResponse>> next, Action<string, double> progress = null)
        {
            var isNewExtension = context.Get<bool>("IsNewExtension");

            try
            {
                progress("Start sql queries", 30);

                var extension = HybridMapper.Map<ExtensionManifest, ExtensionViewModel>(request.Manifest);

                context.Set<IUnitOfWork>("UnitOfWork", _unitOfWork);
                _unitOfWork.BeginTransaction();

                await _service.CreateExtensionAsync(extension, isNewExtension);

                var sqlProviderFolder = Path.Combine(request.ExtractPath, "sql-providers");
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
                    sqlCommands = sqlCommands.Replace("[EXTENSION_ID]", request.Manifest.Id.ToString());
                    await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, sqlCommands);
                }

                progress("End sql queries", 60);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
