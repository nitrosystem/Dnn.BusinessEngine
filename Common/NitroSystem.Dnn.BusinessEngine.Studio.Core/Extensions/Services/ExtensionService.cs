using DotNetNuke.Entities.Portals;
using System;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.Extensions.Manifest;
using DotNetNuke.Entities.Users;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using System.Web.UI;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Common;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.Extensions.Services
{
    public class ExtensionService
    {
        private readonly Guid _scenarioID;
        private readonly PortalSettings _portalSettings;
        private readonly UserInfo _currentUser;

        public ExtensionService()
        {
        }

        public ExtensionService(Guid scenarioID, PortalSettings portalSettings, UserInfo currentUser)
        {
            _portalSettings = portalSettings;
            _scenarioID = scenarioID;
            _currentUser = currentUser;
        }

        #region  Properties

        public static ExtensionService Instance
        {
            get
            {
                return new ExtensionService();
            }
        }

        private ExtensionManifest Extension { get; set; }
        private ExtensionInfo OldExtension { get; set; }

        private string ExtensionUnzipedPath { get; set; }

        private string ModuleMapPath { get; set; }

        private string BaseMapPath { get; set; }

        private ProgressMonitoring MonitoringInstance { get; set; }

        private bool IsNewExtension { get; set; }

        #endregion

        #region Install Extension

        public static string UnzipExtensionFile(string extensionUnzipedPath, string filename)
        {
            FastZip fastZip = new FastZip();
            fastZip.ExtractZip(filename, extensionUnzipedPath, null); //Will always overwrite if target filenames already exist

            File.Delete(filename);

            var files = Directory.GetFiles(extensionUnzipedPath);
            var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");

            return manifestFile;
        }

        public void InstallExtension(ExtensionManifest extension, string extensionUnzipedPath, ProgressMonitoring monitoringInstance)
        {
            this.Extension = extension;
            this.ExtensionUnzipedPath = extensionUnzipedPath;
            this.ModuleMapPath = HttpContext.Current.Server.MapPath("~/DesktopModules/BusinessEngine/");
            this.BaseMapPath = HttpContext.Current.Server.MapPath("~/");
            this.MonitoringInstance = monitoringInstance;

            this.IsNewExtension = ExtensionRepository.Instance.GetExtensionByName(this.Extension.ExtensionName) == null;

            try
            {
                /* 1- */
                CreateExtension();
                /* 2- */
                ExecuteSqlProviders();
                /* 3- */
                CreateExtensionItems();
                /* 4- */
                UnzipAndCopyResources();
            }
            catch (Exception ex)
            {
                if (this.IsNewExtension)
                    ExtensionRepository.Instance.DeleteExtension(this.Extension.ExtensionID);
                else
                {
                    ExtensionRepository.Instance.UpdateExtension(this.OldExtension);
                }

                throw ex;
            }

            // Remove temp folder & files
            Directory.Delete(this.ExtensionUnzipedPath, true);
            monitoringInstance.End();
        }

        /*-----------------------------------------------------------------------
         * ------------------ Step 1 ==> Create or Update Extension ----------------------
         * -------------------------------------------------------------*/
        private void CreateExtension()
        {
            var objExtensionInfo = new ExtensionInfo()
            {
                ExtensionID = this.Extension.ExtensionID,
                ExtensionName = this.Extension.ExtensionName,
                ExtensionType = this.Extension.ExtensionType,
                ExtensionImage = this.Extension.ExtensionImage.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"),
                FolderName = this.Extension.FolderName,
                Summary = this.Extension.Summary,
                Description = this.Extension.Description,
                ReleaseNotes = this.Extension.ReleaseNotes,
                Owner = JsonConvert.SerializeObject(this.Extension.Owner),
                Resources = JsonConvert.SerializeObject(this.Extension.Resources),
                Assemblies = JsonConvert.SerializeObject(this.Extension.Assemblies),
                SqlProviders = JsonConvert.SerializeObject(this.Extension.SqlProviders),
                IsCommercial = this.Extension.IsCommercial,
                LicenseType = this.Extension.LicenseType,
                LicenseKey = this.Extension.LicenseKey,
                SourceUrl = this.Extension.SourceUrl,
                VersionType = this.Extension.VersionType,
                Version = this.Extension.Version,
                LastModifiedByUserID = _currentUser.UserID,
                LastModifiedOnDate = DateTime.Now,
            };

            this.OldExtension = ExtensionRepository.Instance.GetExtensionByName(this.Extension.ExtensionName);
            if (this.OldExtension == null)
            {
                objExtensionInfo.CreatedByUserID = _currentUser.UserID;
                objExtensionInfo.CreatedOnDate = DateTime.Now;

                this.MonitoringInstance.Progress("Create extension record in database", 20);
                Thread.Sleep(500);

                this.Extension.ExtensionID = ExtensionRepository.Instance.AddExtension(objExtensionInfo);
            }
            else
            {
                this.Extension.ExtensionID = objExtensionInfo.ExtensionID = this.OldExtension.ExtensionID;
                objExtensionInfo.CreatedByUserID = this.OldExtension.CreatedByUserID;
                objExtensionInfo.CreatedOnDate = this.OldExtension.CreatedOnDate;

                if (new Version(this.Extension.Version) < new Version(this.OldExtension.Version)) throw new Exception("The installed extension should not be larger than the new extension");
                objExtensionInfo.Version = this.Extension.Version;

                this.MonitoringInstance.Progress("Update extension record in database", 20);
                Thread.Sleep(500);

                ExtensionRepository.Instance.UpdateExtension(objExtensionInfo);
            }
            this.MonitoringInstance.Progress("Created extension record in database has been successfully", 20);
            Thread.Sleep(500);
        }

        /*-----------------------------------------------------------------------
         * ------------------ Step 2 ==> Execute Sql Providers ----------------------
         * -------------------------------------------------------------*/
        private void ExecuteSqlProviders()
        {
            this.MonitoringInstance.Progress("Get the extension installing queries from providers", 25);
            Thread.Sleep(500);

            string sqlProviderFolder = this.ExtensionUnzipedPath + @"\sql-providers\";
            StringBuilder queries = new StringBuilder();
            foreach (var item in (this.Extension.SqlProviders ?? Enumerable.Empty<ExtensionSqlProvider>()).Where(p => p.Type == SqlProviderType.Install && IsValidVersion(p.Version)))
            {
                var query = FileUtil.GetFileContent(sqlProviderFolder + item.File);
                queries.AppendLine(query);
                queries.AppendLine(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(queries.ToString()))
            {
                queries = queries.Replace("[USERID]", this.Extension.ExtensionID.ToString());
                queries = queries.Replace("GO", Environment.NewLine);

                this.MonitoringInstance.Progress("Execute script queries in Dnn database", 30);
                Thread.Sleep(500);

                var queryResult = DbUtil.ExecuteScalarSqlTransaction(queries.ToString());

                if (queryResult.IsSuccess)
                {
                    this.MonitoringInstance.Progress("The Sql Queries has been executed successfully", 65);
                    Thread.Sleep(500);
                }
                else
                {
                    this.MonitoringInstance.Progress("The Sql Queries has been executed failed", 65);
                    this.MonitoringInstance.Progress(queryResult.ResultMessage, 65);
                    Thread.Sleep(500);
                }
            }

            FileUtil.MoveDirectory(sqlProviderFolder, HttpContext.Current.Server.MapPath(string.Format("~/DesktopModules/BusinessEngine/extensions/{0}/sql-providers/", this.Extension.FolderName)));

            this.MonitoringInstance.Progress("The sql providers directory moved to new directory", 70);
        }

        /*-----------------------------------------------------------------------
         * ------------------ Step 3 ==> Create Extension Items ----------------------
         * -------------------------------------------------------------*/
        private void CreateExtensionItems()
        {
            foreach (var package in this.Extension.Packages ?? Enumerable.Empty<ExtensionPackage>())
            {
                //1-Action
                if (package.PackageType == "Actions")
                {
                    foreach (var item in package.Actions ?? Enumerable.Empty<ActionTypeInfo>())
                    {
                        item.ExtensionID = this.Extension.ExtensionID;
                        item.GroupID = GroupRepository.Instance.CheckExistsGroupOrCreateGroup(_scenarioID, _currentUser.UserID, "ActionType", item.GroupName);

                        ActionTypeRepository.Instance.AddActionType(item);

                        this.MonitoringInstance.Progress(string.Format("{0} action type added to the ActionTypes table", item.ActionType), 80);
                        Thread.Sleep(50);
                    }
                }

                //2-Service
                if (package.PackageType == "Services")
                {
                    foreach (var item in package.Services ?? Enumerable.Empty<ServiceTypeInfo>())
                    {
                        item.ExtensionID = this.Extension.ExtensionID;
                        item.GroupID = GroupRepository.Instance.CheckExistsGroupOrCreateGroup(_scenarioID, _currentUser.UserID, "ServiceType", item.GroupName);

                        ServiceTypeRepository.Instance.AddServiceType(item);

                        this.MonitoringInstance.Progress(string.Format("{0} service type added to the ServiceTypes table", item.ServiceSubtype), 83);
                        Thread.Sleep(50);
                    }
                }

                //3-Field
                if (package.PackageType == "Fields")
                {
                    //3-Field
                    foreach (var item in package.Fields ?? Enumerable.Empty<ModuleFieldTypeInfo>())
                    {
                        item.ExtensionID = this.Extension.ExtensionID;
                        item.GroupID = GroupRepository.Instance.CheckExistsGroupOrCreateGroup(_scenarioID, _currentUser.UserID, "FieldType", item.GroupName);

                        ModuleFieldTypeRepository.Instance.AddFieldType(item);

                        this.MonitoringInstance.Progress(string.Format("{0} field type added to the FieldTypes table", item.FieldType), 85);
                        Thread.Sleep(50);
                    }

                    //4-Field Template
                    foreach (var item in package.FieldTemplates ?? Enumerable.Empty<ModuleFieldTypeTemplateInfo>())
                    {
                        ModuleFieldTypeTemplateRepository.Instance.AddTemplate(item);

                        this.MonitoringInstance.Progress(string.Format("{0} template for {1} field type added to the FieldTypeTemplates table", item.TemplateName, item.FieldType), 86);
                        Thread.Sleep(50);
                    }

                    //5-Field Theme
                    foreach (var item in package.FieldThemes ?? Enumerable.Empty<ModuleFieldTypeThemeInfo>())
                    {
                        ModuleFieldTypeThemeRepository.Instance.AddTheme(item);

                        this.MonitoringInstance.Progress(string.Format("{0} theme for {1} field type added to the FieldTypeThemes table", item.ThemeName, item.FieldType), 87);
                        Thread.Sleep(50);
                    }
                }

                //6-Skins
                if (package.PackageType == "Skins")
                {
                    foreach (var item in package.Skins ?? Enumerable.Empty<DashboardSkinInfo>())
                    {
                        item.ExtensionID = this.Extension.ExtensionID;

                        DashboardSkinRepository.Instance.AddSkin(item);

                        this.MonitoringInstance.Progress(string.Format("{0} skin added to the skins table", item.SkinName), 88);
                        Thread.Sleep(50);
                    }
                }

                //7-Providers(Payment Gateway&Sms Gateway)
                if (package.PackageType == "Providers")
                {
                    foreach (var item in package.Providers ?? Enumerable.Empty<ProviderInfo>())
                    {
                        item.ExtensionID = this.Extension.ExtensionID;

                        ProviderRepository.Instance.AddProvider(item);

                        this.MonitoringInstance.Progress(string.Format("{0} provider added to the provider", item.ProviderName), 90);
                        Thread.Sleep(50);
                    }
                }
            }
        }

        /*-----------------------------------------------------------------------
         * ------------------ Step 4 ==> Copy Resources And Assemblies ----------------------
         * -------------------------------------------------------------*/
        private void UnzipAndCopyResources()
        {
            //1-Unzip Resources And Copy To Path Them
            foreach (var item in this.Extension.Resources ?? Enumerable.Empty<ExtensionResource>())
            {
                string filename = this.ExtensionUnzipedPath + @"\" + item.ZipFile;
                string targetDir = this.ModuleMapPath + item.BasePath;

                ExtractFiles(filename, targetDir);

                this.MonitoringInstance.Progress(string.Format("{0} resource files unziped", item.ZipFile), 95);
                Thread.Sleep(50);
            }

            //2-Unzip Assemblies And Copy To bin folder Them
            foreach (var item in this.Extension.Assemblies ?? Enumerable.Empty<ExtensionAssembly>())
            {
                string targetDir = this.BaseMapPath + item.BasePath;
                foreach (var file in item.Items)
                {
                    string source = this.ExtensionUnzipedPath + @"\bin\" + file;
                    File.Copy(source, targetDir + @"\" + file, true);

                    this.MonitoringInstance.Progress(string.Format("{0} files copy to the destitions path", file), 95);
                    Thread.Sleep(50);
                }
            }
        }

        #endregion

        #region Uninstall Extension 

        /*-----------------------------------------------------------------------
         * ------------------ Uninstall Extension ----------------------
         * -------------------------------------------------------------*/
        public void UninstallExtension(Guid extensionID)
        {
            var extension = ExtensionRepository.Instance.GetExtension(extensionID);

            var sqlProviders = JsonConvert.DeserializeObject<IEnumerable<ExtensionSqlProvider>>(extension.SqlProviders) ?? Enumerable.Empty<ExtensionSqlProvider>();
            var uninstallProvider = sqlProviders.FirstOrDefault(p => p.Type == SqlProviderType.Uninstall);
            if (uninstallProvider != null) UninstallQueries(extension, uninstallProvider);

            ExtensionRepository.Instance.DeleteExtension(extensionID);
        }

        private void UninstallQueries(ExtensionInfo extension, ExtensionSqlProvider uninstallProvider)
        {
            var uninstallQueryFilePath = HttpContext.Current.Server.MapPath(string.Format("~/DesktopModules/BusinessEngine/extensions/{0}/sql-providers/{1}", extension.FolderName, uninstallProvider.File));
            var uninstallQueries = FileUtil.GetFileContent(uninstallQueryFilePath);
            uninstallQueries = uninstallQueries.Replace("GO", Environment.NewLine);

            if (!string.IsNullOrWhiteSpace(uninstallQueries)) DbUtil.ExecuteScalarSqlTransaction(uninstallQueries);
        }

        #endregion

        #region Private Methods

        private void ExtractFiles(string zipFile, string targetDir, string fileFilter = null)
        {
            FastZip fastZip = new FastZip();
            fastZip.ExtractZip(zipFile, targetDir, fileFilter); //Will always overwrite if target filenames already exist
        }

        private bool IsValidVersion(string version)
        {
            if (this.IsNewExtension || string.IsNullOrEmpty(version)) return true;

            var extVersion = ExtensionRepository.Instance.GetExtensionVersion(this.Extension.ExtensionName);

            return new Version(extVersion) < new Version(version);
        }

        #endregion
    }
}
