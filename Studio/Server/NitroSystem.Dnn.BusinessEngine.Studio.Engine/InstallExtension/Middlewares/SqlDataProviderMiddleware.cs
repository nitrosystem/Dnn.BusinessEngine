using System;
using System.IO;
using System.Linq;
using System.Text;
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

        public async Task<InstallExtensionResponse> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<InstallExtensionResponse>> next, Action< string, double> progress = null)
        {
            var isNewExtension = context.Get<bool>("IsNewExtension");

            try
            {
                progress("Start sql queries", 30);

                var extension = HybridMapper.Map<ExtensionManifest, ExtensionViewModel>(request.Manifest,
                    (src, dest) =>
                    {
                        dest.Id = src.Id.Value;
                        dest.Owner = src.Owner.OwnerName;
                        dest.Url = src.Owner.Url;
                        dest.Email = src.Owner.Email;
                    });

                context.Set<IUnitOfWork>("UnitOfWork", _unitOfWork);

                _unitOfWork.BeginTransaction();

                 await _service.SaveExtensionAsync(extension, isNewExtension);

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
                    sqlCommands = sqlCommands.Replace("[EXTENSIONID]", request.Manifest.Id.Value.ToString());

                    await _sqlCommand.ExecuteSqlCommandTextAsync(_unitOfWork, sqlCommands);
                }

                progress("End sql queries", 60);
            }
            catch (Exception ex)
            {
                progress("sql queries has error", 60);
                progress(ex.Message, 60);

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
