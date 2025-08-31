using DotNetNuke.Entities.Portals;
using System;
using System.Linq;
using System.Text;
using System.IO;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.Extensions.Manifest;
using DotNetNuke.Entities.Users;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using System.Web.UI;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        #region Install Extension

        public async Task<ExtensionInstallationResultDto> InstallExtension(ExtensionManifest extension, IUnitOfWork unitOfWork, string oldVersion, string unzipedPath)
        {
            await ExecuteSqlProviders(extension, unitOfWork, unzipedPath);

            UnzipAndCopyResources(extension, unzipedPath);

            return new ExtensionInstallationResultDto()
            {
                IsInstalled = true
            };
        }

        /*-----------------------------------------------------------------------
         * ------------------ Step 1 ==> Execute Sql Providers ----------------------
         * -------------------------------------------------------------*/
        private async Task ExecuteSqlProviders(ExtensionManifest extension, IUnitOfWork unitOfWork, string unzipedPath)
        {
            string sqlProviderFolder = unzipedPath + @"\sql-providers\";
            StringBuilder queries = new StringBuilder();
            foreach (var item in (extension.SqlProviders ?? Enumerable.Empty<ExtensionSqlProvider>()).Where(p => p.Type == SqlProviderType.Install && IsValidVersion(extension, p.Version)))
            {
                var query = await FileUtil.GetFileContentAsync(sqlProviderFolder + item.File);
                queries.AppendLine(query);
                queries.AppendLine(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(queries.ToString()))
            {
                //queries = queries.Replace("[USERID]", extension.ExtensionID.ToString());
                queries = queries.Replace("GO", Environment.NewLine);

                var sqlCommand = new ExecuteSqlCommand(unitOfWork);
                await sqlCommand.ExecuteSqlCommandTextAsync(queries.ToString());
            }
        }

        /*-----------------------------------------------------------------------
         * ------------------ Step 2 ==> Copy Resources And Assemblies ----------------------
         * -------------------------------------------------------------*/
        private void UnzipAndCopyResources(ExtensionManifest extension, string unzipedPath)
        {
            var modulePath = HttpContext.Current.Server.MapPath("~/DesktopModules/BusinessEngine/");

            //1-Unzip Resources And Copy To Path Them
            foreach (var item in extension.Resources ?? Enumerable.Empty<ExtensionResource>())
            {
                string filename = unzipedPath + @"\" + item.ZipFile;
                string targetDir = modulePath + item.BasePath;

                ZipUtil.Unzip(filename, targetDir);
            }

            //2-Unzip Assemblies And Copy To bin folder Them
            foreach (var item in extension.Assemblies ?? Enumerable.Empty<ExtensionAssembly>())
            {
                string targetDir = HttpContext.Current.Server.MapPath("~/") + item.BasePath;
                foreach (var file in item.Items)
                {
                    string source = unzipedPath + @"\bin\" + file;
                    File.Copy(source, targetDir + @"\" + file, true);
                }
            }
        }

        #endregion

        #region Uninstall Extension 

        /*-----------------------------------------------------------------------
         * ------------------ Uninstall Extension ----------------------
         * -------------------------------------------------------------*/
        //public void UninstallExtension(Guid extensionID)
        //{
        //    var extension = ExtensionRepository.Instance.GetExtension(extensionID);

        //    var sqlProviders = JsonConvert.DeserializeObject<IEnumerable<ExtensionSqlProvider>>(extension.SqlProviders) ?? Enumerable.Empty<ExtensionSqlProvider>();
        //    var uninstallProvider = sqlProviders.FirstOrDefault(p => p.Type == SqlProviderType.Uninstall);
        //    if (uninstallProvider != null) UninstallQueries(extension, uninstallProvider);

        //    ExtensionRepository.Instance.DeleteExtension(extensionID);
        //}

        //private void UninstallQueries(ExtensionInfo extension, ExtensionSqlProvider uninstallProvider)
        //{
        //    var uninstallQueryFilePath = HttpContext.Current.Server.MapPath(string.Format("~/DesktopModules/business-engine/extensions/{0}/sql-providers/{1}", extension.FolderName, uninstallProvider.File));
        //    var uninstallQueries = FileUtil.GetFileContent(uninstallQueryFilePath);
        //    uninstallQueries = uninstallQueries.Replace("GO", Environment.NewLine);

        //    if (!string.IsNullOrWhiteSpace(uninstallQueries)) DbUtil.ExecuteScalarSqlTransaction(uninstallQueries);
        //}

        #endregion

        #region Private Methods

        private bool IsValidVersion(ExtensionManifest extension, string oldVersion)
        {
            if (extension.IsNewExtension || string.IsNullOrEmpty(oldVersion)) return true;

            return new Version(extension.Version) > new Version(oldVersion);
        }

        #endregion
    }
}
