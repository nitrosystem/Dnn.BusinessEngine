using DotNetNuke.Collections;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Appearance;
using NitroSystem.Dnn.BusinessEngine.Core.Common;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace NitroSystem.Dnn.BusinessEngine.Core.ModuleBuilder
{
    public static class ModuleResourceService
    {
        #region Dashboard Module And Child Modules Methods

        public static async Task GenerateDashboardModulesResources(ModuleView module, int dnnTabID, bool isChild, string moduleTemplate, string customHtml, string customJs, string customCss, bool isStandaloneDashboard, PortalSettings portalSettings, ProgressMonitoring monitoringInstance)
        {
            var dashboardModule = !isChild ? module : ModuleRepository.Instance.GetModuleView(module.ParentID.Value);
            var skin = DashboardSkinManager.GetSkin(dashboardModule.ModuleID, HttpContext.Current);

            if (module.ModuleBuilderType == "HtmlEditor")
            {
                await SaveHtmlEditorModuleFiles(module, customHtml, customJs, customCss, portalSettings, HttpContext.Current, monitoringInstance);
            }
            else
            {
                await SaveModuleHtmlTemplate(module, moduleTemplate, portalSettings, HttpContext.Current, monitoringInstance);

                await SaveModuleLayoutCss(module, portalSettings, HttpContext.Current, monitoringInstance);
            }

            await SaveDashboardModulesFieldsStyles(dashboardModule, portalSettings, HttpContext.Current, monitoringInstance);

            await SaveDashboardModulesFieldsScripts(dashboardModule, portalSettings, HttpContext.Current, monitoringInstance);

            await SaveDashboardModulesActionsScripts(dashboardModule, portalSettings, HttpContext.Current, monitoringInstance);

            var hasSkinThemeFilecontent = await SaveDashboardModulesFieldsStylesThatUsedSkinTheme(dashboardModule, skin, portalSettings, HttpContext.Current, monitoringInstance);

            var resources = GetDashboardResources(dashboardModule, isChild, (isChild ? module.ModuleID : dashboardModule.ModuleID), (isChild ? module.ModuleName : null), skin, hasSkinThemeFilecontent, portalSettings, HttpContext.Current, monitoringInstance);

            Guid? childModuleID = module.ModuleID;
            await UpdateDashboardModulePageResources(dashboardModule, resources, isChild, (isChild ? childModuleID : null), dnnTabID, isStandaloneDashboard, portalSettings, HttpContext.Current, monitoringInstance);

            if (monitoringInstance != null)
            {
                monitoringInstance.End();
            }
        }

        private static async Task SaveDashboardModulesFieldsStyles(ModuleView dashboardModule, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module fields styles...", "fields-styles,0", context);
            }

            string fieldsStyles = await ModuleResourcesContentService.GetModulesFieldsStyles(dashboardModule.ModuleID, context);
            FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.css", portalSettings.HomeSystemDirectoryMapPath, dashboardModule.ScenarioName, dashboardModule.ModuleName), fieldsStyles, true);
        }

        private static async Task SaveDashboardModulesFieldsScripts(ModuleView dashboardModule, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            string fieldsScriptsFile = string.Empty;

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module fields scripts...", "fields-scripts,0", context);
            }

            string fieldScripts = await ModuleResourcesContentService.GetModulesFieldsScripts(dashboardModule.ModuleID, context);
            FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.js", portalSettings.HomeSystemDirectoryMapPath, dashboardModule.ScenarioName, dashboardModule.ModuleName), fieldScripts, true);
        }

        private static async Task SaveDashboardModulesActionsScripts(ModuleView dashboardModule, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module actions scripts...", "actions-scripts,0", context);
            }

            string actionsScripts = await ModuleResourcesContentService.GetModulesActionsScripts(dashboardModule.ModuleID, context);
            FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--actions-{2}.js", portalSettings.HomeSystemDirectoryMapPath, dashboardModule.ScenarioName, dashboardModule.ModuleName), actionsScripts, true);
        }

        private static async Task<bool> SaveDashboardModulesFieldsStylesThatUsedSkinTheme(ModuleView dashboardModule, DashboardSkin skin, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module fields styles...", "fields-styles,0", context);
            }

            string skinThemeFieldsStyles = await ModuleResourcesContentService.GetDashboardModulesFieldsStylesThatUsedSkinTheme(dashboardModule.ModuleID, skin, context);

            bool hasFilecontent = !string.IsNullOrWhiteSpace(skinThemeFieldsStyles);
            if (hasFilecontent) FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module-skin-theme-{2}.css", portalSettings.HomeSystemDirectoryMapPath, dashboardModule.ScenarioName, dashboardModule.ModuleName), skinThemeFieldsStyles, true);

            return hasFilecontent;
        }

        private static IEnumerable<PageResourceInfo> GetDashboardModuleFieldsLibraryResources(Guid moduleID)
        {
            var result = new List<PageResourceInfo>();

            List<Guid> moduleIds = new List<Guid>() { moduleID };
            moduleIds.AddRange(ModuleRepository.Instance.GetModuleChildsID(moduleID));

            var moduleFieldTypes = ModuleFieldRepository.Instance.GetModulesFieldTypes(string.Join(",", moduleIds));
            foreach (var item in moduleFieldTypes)
            {
                var libraries = ModuleFieldTypeLibraryRepository.Instance.GetLibraries(item.FieldType);
                foreach (var library in libraries)
                {
                    var logo = LibraryRepository.Instance.GetLibraryLogo(library.LibraryName, library.Version);

                    var resources = LibraryRepository.Instance.GetLibraryResources(library.LibraryName, library.Version);
                    foreach (var resource in resources)
                    {
                        result.Add(new PageResourceInfo()
                        {
                            ModuleID = item.ModuleID,
                            FieldType = item.FieldType,
                            LibraryName = library.LibraryName,
                            LibraryVersion = library.Version,
                            LibraryLogo = logo,
                            ResourceType = resource.ResourceType,
                            ResourcePath = resource.ResourcePath,
                        });
                    }
                }
            }

            return result;
        }

        private static List<PageResourceInfo> GetDashboardSkinLibraryResources(ModuleView dashboardModule, DashboardSkin skin)
        {
            var result = new List<PageResourceInfo>();

            foreach (var library in skin.Libraries)
            {
                var resources = LibraryRepository.Instance.GetLibraryResources(library.LibraryName, library.Version);
                foreach (var item in resources ?? Enumerable.Empty<LibraryView>())
                {
                    result.Add(new PageResourceInfo()
                    {
                        ModuleID = dashboardModule.ModuleID,
                        IsSkinResource = true,
                        LibraryName = item.LibraryName,
                        LibraryVersion = item.Version,
                        LibraryLogo = item.LibraryLogo,
                        ResourceType = item.ResourceType,
                        ResourcePath = item.ResourcePath
                    });
                }
            }

            return result;
        }

        private static List<PageResourceInfo> GetDashboardSkinBaseResources(ModuleView dashboardModule, DashboardSkin skin, HttpContext context)
        {
            var result = new List<PageResourceInfo>();

            foreach (var item in skin.BaseCssFiles ?? Enumerable.Empty<string>())
            {
                result.Add(new PageResourceInfo()
                {
                    ResourceType = "css",
                    ResourcePath = skin.SkinPath + "/" + item,
                    LoadOrder = result.Count,
                    IsBaseResource = true,
                    IsActive = true
                });
            }

            foreach (var item in skin.BaseJsFiles ?? Enumerable.Empty<string>())
            {
                result.Add(new PageResourceInfo()
                {
                    ResourceType = "js",
                    ResourcePath = skin.SkinPath + "/" + item,
                    LoadOrder = result.Count,
                    IsBaseResource = true,
                    IsActive = true
                });
            }

            return result;
        }

        private static List<PageResourceInfo> GetDashboardSkinTemplateResources(ModuleView dashboardModule, DashboardSkin skin, HttpContext context)
        {
            var result = new List<PageResourceInfo>();

            var template = skin.DashboardTemplates.FirstOrDefault(t => t.TemplateName == dashboardModule.Template);

            foreach (var item in template.CssFiles ?? Enumerable.Empty<string>())
            {
                result.Add(new PageResourceInfo()
                {
                    IsSystemResource = true,
                    ResourceType = "css",
                    ResourcePath = skin.SkinPath + "/" + item,
                });
            }

            foreach (var item in template.JsFiles ?? Enumerable.Empty<string>())
            {
                result.Add(new PageResourceInfo()
                {
                    IsSystemResource = true,
                    ResourceType = "js",
                    ResourcePath = skin.SkinPath + "/" + item,
                });
            }

            return result;
        }

        private static List<PageResourceInfo> GetDashboardResources(ModuleView dashboardModule, bool isChild, Guid? childModuleID, string childModuleName, DashboardSkin skin, bool hasSkinThemeFilecontent, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            List<PageResourceInfo> result = new List<PageResourceInfo>();

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Start generating {dashboardModule.ModuleName} module page resources, that are include base frameworks & client app & libraries & custom resource & system files...: ", "resource-libraries,0", context);
            }

            // 1- Get skin libraries resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate base framework including angularjs,lodash,...", "resource-libraries,10", context);
            }

            result.AddRange(GetDashboardSkinLibraryResources(dashboardModule, skin));
            //----------------------------------------------------------------------------------------------

            // 2- Get base framework resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate base framework including angularjs,lodash,...", "resource-libraries,10", context);
            }

            result.AddRange(GetBaseFrameworksAndLibrariesResources(dashboardModule.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 3- Get client app resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate client app resources(main files)...", "resource-libraries,70", context);
            }

            result.AddRange(GetClientAppBaseResources(dashboardModule.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 4- Get dashboard skin resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate base framework including angularjs,lodash,...", "resource-libraries,10", context);
            }

            result.AddRange(GetDashboardSkinBaseResources(dashboardModule, skin, context));
            //----------------------------------------------------------------------------------------------

            // 5- Get dashboard skin resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate base framework including angularjs,lodash,...", "resource-libraries,10", context);
            }

            result.AddRange(GetDashboardSkinTemplateResources(dashboardModule, skin, context));
            //----------------------------------------------------------------------------------------------

            // 6- Get custom libraries resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom libraries...", "resource-libraries,40", context);
            }

            result.AddRange(GetModuleCustomLibraryResources(dashboardModule.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 7- Get custom resources 
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom resources...", "resource-libraries,50", context);
            }

            result.AddRange(GetModuleCustomResources(dashboardModule.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 8- Get fields depended libraries resources 
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom resources...", "resource-libraries,50", context);
            }

            result.AddRange(GetDashboardModuleFieldsLibraryResources(dashboardModule.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 9- module-{2}.css : include module layout css styles
            string layoutCssFile = string.Format("{0}/business-engine/{1}/{2}/module--{2}.css", portalSettings.HomeSystemDirectory, dashboardModule.ScenarioName, isChild ? childModuleName : dashboardModule.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {layoutCssFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ModuleID = isChild ? childModuleID.Value : dashboardModule.ModuleID,
                ResourceType = "css",
                ResourcePath = layoutCssFile
            });
            //---------------------------------------------------------------------

            if (hasSkinThemeFilecontent)
            {
                // 10- module-skin-theme-{2}.css : include fields styles that used skin theme
                string skinThemeFieldsStyles = string.Format("{0}/business-engine/{1}/{2}/module-skin-theme-{2}.css", portalSettings.HomeSystemDirectory, dashboardModule.ScenarioName, dashboardModule.ModuleName);

                if (monitoringInstance != null)
                {
                    monitoringInstance.Progress($"writed the module and fields styles into file : {layoutCssFile}...", "module-styles,100", context);
                }

                result.Add(new PageResourceInfo()
                {
                    IsSystemResource = true,
                    ResourceType = "css",
                    ResourcePath = skinThemeFieldsStyles
                });
                //---------------------------------------------------------------------
            }

            // 11- module-fields--{2}.css : include module fields & template & theme css styles
            string fieldsStylesFile = string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.css", portalSettings.HomeSystemDirectory, dashboardModule.ScenarioName, dashboardModule.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {fieldsStylesFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "css",
                ResourcePath = fieldsStylesFile
            });
            //---------------------------------------------------------------------

            // 12- module-fields--{2}.js : include module fields & directive js scripts
            string fieldsScriptsFile = string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.js", portalSettings.HomeSystemDirectory, dashboardModule.ScenarioName, dashboardModule.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {fieldsScriptsFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "js",
                ResourcePath = fieldsScriptsFile
            });
            //---------------------------------------------------------------------

            // 13- module--actions-{2}.js : include module actions js scripts
            string actionsScriptsFile = string.Format("{0}/business-engine/{1}/{2}/module--actions-{2}.js", portalSettings.HomeSystemDirectory, dashboardModule.ScenarioName, dashboardModule.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {actionsScriptsFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "js",
                ResourcePath = actionsScriptsFile
            });
            //---------------------------------------------------------------------

            return result;
        }

        private static async Task UpdateDashboardModulePageResources(ModuleView dashboardModule, List<PageResourceInfo> resources, bool isChild, Guid? childModuleID, int? tabID, bool isStandaloneDashboard, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            var moduleID = dashboardModule.ModuleID;

            await Task.Run(() =>
            {
                if (monitoringInstance != null)
                {
                    monitoringInstance.Progress($"Remove old {dashboardModule.ModuleName} module page resources...", "resource-libraries,80", context);
                }

                PageResourceRepository.Instance.DeletePageResources(isChild ? childModuleID.Value : dashboardModule.ModuleID);

                if (isChild)
                {
                    var dashboardResources = PageResourceRepository.Instance.GetModuleResources(dashboardModule.ModuleID);

                    var mustBeDelete = dashboardResources.Where(r => resources.Select(rr => rr.ResourcePath).Contains(r.ResourcePath)).Select(r => r.ResourceID);
                    PageResourceRepository.Instance.DeletePageResources(string.Join(",", mustBeDelete));
                }

                var pageResources = isStandaloneDashboard ? null : PageResourceRepository.Instance.GetPageResources(tabID.ToString());

                var progressStep = 20 / resources.Count();
                var index = 80 + progressStep;
                var loadOrder = 0;

                foreach (var resource in resources)
                {
                    resource.ModuleID = resource.ModuleID != null ? resource.ModuleID : moduleID;
                    resource.IsActive = isStandaloneDashboard ? true : (!pageResources.Any(r => (r.ResourcePath ?? string.Empty).ToLower() == resource.ResourcePath.ToLower() && r.IsActive));
                    resource.LoadOrder = loadOrder++;
                    resource.CmsPageID = isStandaloneDashboard ? null : tabID;

                    if (monitoringInstance != null)
                    {
                        monitoringInstance.Progress($"Insert new resource at db : {resource.ResourcePath}...", $"resource-libraries,{index}", context);
                    }

                    PageResourceRepository.Instance.AddPageResource(resource);

                    index = index + progressStep;
                }

                if (monitoringInstance != null)
                {
                    monitoringInstance.Progress($"End inserting resources at the database", "resource-libraries,100", context);
                }
            });
        }

        #endregion

        #region Form Designer Module Methods

        public static async Task GenerateFormDesignerModuleResources(ModuleView module, int dnnTabID, string moduleTemplate, PortalSettings portalSettings, ProgressMonitoring monitoringInstance)
        {
            //1.Call SaveModuleHtmlTemplate method for write template in the module file
            await SaveModuleHtmlTemplate(module, moduleTemplate, portalSettings, HttpContext.Current, monitoringInstance);

            //2.Call SaveModuleLayoutCss method for write fields scripts in the module js file
            await SaveModuleLayoutCss(module, portalSettings, HttpContext.Current, monitoringInstance);

            //3.Call SaveModuleFieldsStyles method for write fields scripts in the module js file
            await SaveModuleFieldsStyles(module, portalSettings, HttpContext.Current, monitoringInstance);

            //4.Call SaveModuleFieldsScripts method for write module and fields style in the module css file
            await SaveModuleFieldsScripts(module, portalSettings, HttpContext.Current, monitoringInstance);

            //5.Call SaveModuleActionsScripts method for write module and fields style in the module css file
            await SaveModuleActionsScripts(module, portalSettings, HttpContext.Current, monitoringInstance);

            //6.Call GetFormDesignerModuleResources method for get module resources 
            var resources = GetFormDesignerModuleResources(module, portalSettings, HttpContext.Current, monitoringInstance);

            //7.Call UpdateModulePageResources method for insert resource into database
            await UpdateModulePageResources(module, resources, dnnTabID, portalSettings, HttpContext.Current, monitoringInstance);

            if (monitoringInstance != null)
            {
                monitoringInstance.End();
            }
        }

        private static async Task SaveModuleHtmlTemplate(ModuleView module, string moduleTemplate, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            await Task.Run(() =>
            {
                if (monitoringInstance != null)
                {
                    string moduleTemplateFile = string.Format("{0}/business-engine/{1}/{2}/module--{2}.html", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName);
                    monitoringInstance.Progress($"Generate module html template and write into file : {moduleTemplateFile}...", "module-template,100", context);
                }

                FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--{2}.html", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), moduleTemplate, true);
            });
        }

        private static async Task SaveModuleLayoutCss(ModuleView module, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            await Task.Run(() =>
            {
                string moduleStylesFile = string.Empty;

                if (monitoringInstance != null)
                {
                    monitoringInstance.Progress($"Generate module & fields styles...", "module-styles,0", context);
                }

                FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--{2}.css", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), module.LayoutCss, true);
            });
        }

        private static async Task SaveModuleFieldsStyles(ModuleView module, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module fields styles...", "fields-styles,0", context);
            }

            string fieldsStyles = await ModuleResourcesContentService.GetModuleFieldsStyles(module, context);
            FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.css", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), fieldsStyles, true);
        }

        private static async Task SaveModuleFieldsScripts(ModuleView module, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module fields scripts...", "fields-scripts,0", context);
            }

            string fieldScripts = await ModuleResourcesContentService.GetModuleFieldsScripts(module.ModuleID, context);
            FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.js", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), fieldScripts, true);
        }

        private static async Task SaveModuleActionsScripts(ModuleView module, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module actions scripts...", "actions-scripts,0", context);
            }

            string actionsScripts = await ModuleResourcesContentService.GetModuleActionsScripts(module.ModuleID, context);
            FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module--action-{2}.js", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), actionsScripts, true);
        }

        private static IEnumerable<PageResourceInfo> GetFormDesignerModuleFieldsLibraryResources(Guid moduleID)
        {
            var result = new List<PageResourceInfo>();

            var moduleFieldTypes = ModuleFieldRepository.Instance.GetModulesFieldTypes(moduleID.ToString());
            foreach (var item in moduleFieldTypes)
            {
                var libraries = ModuleFieldTypeLibraryRepository.Instance.GetLibraries(item.FieldType);
                foreach (var library in libraries)
                {
                    var logo = LibraryRepository.Instance.GetLibraryLogo(library.LibraryName, library.Version);

                    var resources = LibraryRepository.Instance.GetLibraryResources(library.LibraryName, library.Version);
                    foreach (var resource in resources)
                    {
                        result.Add(new PageResourceInfo()
                        {
                            ModuleID = item.ModuleID,
                            FieldType = item.FieldType,
                            LibraryName = library.LibraryName,
                            LibraryVersion = library.Version,
                            LibraryLogo = logo,
                            ResourceType = resource.ResourceType,
                            ResourcePath = resource.ResourcePath,
                        });
                    }
                }
            }

            return result;
        }

        private static IEnumerable<PageResourceInfo> GetFormDesignerModuleResources(ModuleView module, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            List<PageResourceInfo> result = new List<PageResourceInfo>();

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Start generating {module.ModuleName} module page resources, that are include base frameworks & client app & libraries & custom resource & system files...: ", "resource-libraries,0", context);
            }

            // 1- Get base framework resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate base framework including angularjs,lodash,...", "resource-libraries,10", context);
            }

            result.AddRange(GetBaseFrameworksAndLibrariesResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 2- Get client app resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate client app resources(main files)...", "resource-libraries,70", context);
            }

            result.AddRange(GetClientAppBaseResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 3- Get custom libraries resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom libraries...", "resource-libraries,40", context);
            }

            result.AddRange(GetModuleCustomLibraryResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 4- Get custom resources 
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom resources...", "resource-libraries,50", context);
            }

            result.AddRange(GetModuleCustomResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 5- Get fields depended libraries resources 
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom resources...", "resource-libraries,50", context);
            }

            result.AddRange(GetFormDesignerModuleFieldsLibraryResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 5- module-{2}.css : include module layout css styles
            string layoutCssFile = string.Format("{0}/business-engine/{1}/{2}/module--{2}.css", portalSettings.HomeSystemDirectory, module.ScenarioName, module.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {layoutCssFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "css",
                ResourcePath = layoutCssFile
            });
            //---------------------------------------------------------------------

            // 6- module-fields--{2}.css : include module fields & template & theme css styles
            string fieldsStylesFile = string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.css", portalSettings.HomeSystemDirectory, module.ScenarioName, module.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {fieldsStylesFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "css",
                ResourcePath = fieldsStylesFile
            });
            //---------------------------------------------------------------------

            // 7- module-fields--{2}.js : include module fields & directive js scripts
            string fieldsScriptsFile = string.Format("{0}/business-engine/{1}/{2}/module--fields-{2}.js", portalSettings.HomeSystemDirectory, module.ScenarioName, module.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {fieldsScriptsFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "js",
                ResourcePath = fieldsScriptsFile
            });
            //---------------------------------------------------------------------

            // 8- module-actions--{2}.js : include module actions js scripts
            string actionsScriptsFile = string.Format("{0}/business-engine/{1}/{2}/module--actions-{2}.js", portalSettings.HomeSystemDirectory, module.ScenarioName, module.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {actionsScriptsFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "js",
                ResourcePath = actionsScriptsFile
            });
            //---------------------------------------------------------------------

            return result;
        }

        #endregion

        #region Html Editor Module Methods

        public static async Task GenerateHtmlEditorModuleResources(ModuleView module, int dnnTabID, string customHtml, string customJs, string customCss, PortalSettings portalSettings, ProgressMonitoring monitoringInstance)
        {
            //1.Call SaveHtmlEditorModuleFiles method for write custom files 
            await SaveHtmlEditorModuleFiles(module, customHtml, customJs, customCss, portalSettings, HttpContext.Current, monitoringInstance);

            //2.Call SaveHtmlEditorModuleActionsScripts method for write module and fields style in the module css file
            await SaveHtmlEditorModuleActionsScripts(module, portalSettings, HttpContext.Current, monitoringInstance);

            //3.Call GetHtmlEditorModuleResources method for get module resources 
            var resources = GetHtmlEditorModuleResources(module, portalSettings, HttpContext.Current, monitoringInstance);

            //4.Call UpdateModulePageResources method for insert resource into database
            await UpdateModulePageResources(module, resources, dnnTabID, portalSettings, HttpContext.Current, monitoringInstance);

            if (monitoringInstance != null)
            {
                monitoringInstance.End();
            }
        }

        private static async Task SaveHtmlEditorModuleFiles(ModuleView module, string customHtml, string customJs, string customCss, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            await Task.Run(() =>
            {
                //custom html
                monitoringInstance.Progress($"Create module custom html with name module--{module.ModuleName}.html file...", "module-template,33", context);
                FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/custom.html", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), customHtml, true);

                //custom js
                monitoringInstance.Progress($"Create module custom javascripts with name module--{module.ModuleName}.js file...", "module-template,66", context);
                FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/custom.js", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), customJs, true);

                //custom css
                monitoringInstance.Progress($"Create module custom css styles with name [module--{module.ModuleName}.css] file...", "module-template,100", context);
                FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/custom.css", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), customCss, true);
            });
        }

        private static async Task SaveHtmlEditorModuleActionsScripts(ModuleView module, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate module actions scripts...", "actions-scripts,0", context);
            }

            string actionsScripts = await ModuleResourcesContentService.GetModuleActionsScripts(module.ModuleID, context);
            FileUtil.CreateTextFile(string.Format("{0}/business-engine/{1}/{2}/module-action--{2}.js", portalSettings.HomeSystemDirectoryMapPath, module.ScenarioName, module.ModuleName), actionsScripts, true);
        }

        private static IEnumerable<PageResourceInfo> GetHtmlEditorModuleResources(ModuleView module, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            List<PageResourceInfo> result = new List<PageResourceInfo>();

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Start generating {module.ModuleName} module page resources, that are include base frameworks & client app & libraries & custom resource & system files...: ", "resource-libraries,0", context);
            }

            // 1- Get base framework resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate base framework including angularjs,lodash,...", "resource-libraries,10", context);
            }

            result.AddRange(GetBaseFrameworksAndLibrariesResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 2- Get client app resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate client app resources(main files)...", "resource-libraries,70", context);
            }

            result.AddRange(GetClientAppBaseResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 3- Get custom libraries resources
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom libraries...", "resource-libraries,40", context);
            }

            result.AddRange(GetModuleCustomLibraryResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 4- Get custom resources 
            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"Generate custom resources...", "resource-libraries,50", context);
            }

            result.AddRange(GetModuleCustomResources(module.ModuleID));
            //----------------------------------------------------------------------------------------------

            // 5- custom.css : include module custom css styles
            string customCssFile = string.Format("{0}/business-engine/{1}/{2}/custom.css", portalSettings.HomeSystemDirectory, module.ScenarioName, module.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {customCssFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "css",
                ResourcePath = customCssFile
            });
            //---------------------------------------------------------------------

            // 5- custom.js : include module custom js scripts
            string customJsFile = string.Format("{0}/business-engine/{1}/{2}/custom.js", portalSettings.HomeSystemDirectory, module.ScenarioName, module.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {customJsFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "js",
                ResourcePath = customJsFile
            });
            //---------------------------------------------------------------------

            // 7- module-actions--{2}.js : include module actions js scripts
            string actionsScriptsFile = string.Format("{0}/business-engine/{1}/{2}/module--actions-{2}.js", portalSettings.HomeSystemDirectory, module.ScenarioName, module.ModuleName);

            if (monitoringInstance != null)
            {
                monitoringInstance.Progress($"writed the module and fields styles into file : {actionsScriptsFile}...", "module-styles,100", context);
            }

            result.Add(new PageResourceInfo()
            {
                IsSystemResource = true,
                ResourceType = "js",
                ResourcePath = actionsScriptsFile
            });
            //---------------------------------------------------------------------

            return result;
        }

        #endregion

        #region Common Methods

        private static List<PageResourceInfo> GetBaseFrameworksAndLibrariesResources(Guid moduleID)
        {
            var result = new List<PageResourceInfo>();

            //get angularjs framework basic resources
            var resources = LibraryRepository.Instance.GetLibraryResources("angularjs", "1.8.2");

            //get lodash library basic resources
            resources = resources.Concat<LibraryView>(LibraryRepository.Instance.GetLibraryResources("lodash", "4.17.21"));

            //get moment.js library basic resources(without locale files)
            resources = resources.Concat<LibraryView>(LibraryRepository.Instance.GetLibraryResources("moment", "2.30.1"));

            //fill resources into method result
            result.AddRange(resources.Select(lib => new PageResourceInfo()
            {
                ModuleID = moduleID,
                IsBaseResource = true,
                IsSystemResource = false,
                LibraryName = lib.LibraryName,
                LibraryVersion = lib.Version,
                LibraryLogo = lib.LibraryLogo,
                ResourceType = lib.ResourceType,
                ResourcePath = lib.ResourcePath
            }));

            return result;
        }

        private static List<PageResourceInfo> GetClientAppBaseResources(Guid moduleID)
        {
            var result = new List<PageResourceInfo>();

            var clientAppLibraryResources = LibraryRepository.Instance.GetLibraryResources("client-app", "1.0.0");
            clientAppLibraryResources.ForEach<LibraryView>(lr =>
                result.Add(new PageResourceInfo()
                {
                    ModuleID = moduleID,
                    IsBaseResource = true,
                    IsSystemResource = true,
                    LibraryName = lr.LibraryName,
                    LibraryVersion = lr.Version,
                    LibraryLogo = lr.LibraryLogo,
                    ResourceType = lr.ResourceType,
                    ResourcePath = lr.ResourcePath
                }));

            return result;
        }

        private static List<PageResourceInfo> GetModuleCustomLibraryResources(Guid moduleID)
        {
            var result = new List<PageResourceInfo>();

            var libraryies = ModuleCustomLibraryRepository.Instance.GetLibraries(moduleID);
            foreach (var library in libraryies)
            {
                var resources = LibraryRepository.Instance.GetLibraryResources(library.LibraryName, library.Version);
                foreach (var item in resources)
                {
                    result.Add(new PageResourceInfo()
                    {
                        ModuleID = moduleID,
                        IsCustomResource = true,
                        LibraryName = item.LibraryName,
                        LibraryVersion = item.Version,
                        LibraryLogo = item.LibraryLogo,
                        ResourceType = item.ResourceType,
                        ResourcePath = item.ResourcePath
                    });
                }
            }

            return result;
        }

        private static List<PageResourceInfo> GetModuleCustomResources(Guid moduleID)
        {
            var result = new List<PageResourceInfo>();

            //Get module custom resources
            var customResources = ModuleCustomResourceRepository.Instance.GetResources(moduleID);
            foreach (var item in customResources)
            {
                result.Add(new PageResourceInfo()
                {
                    ModuleID = moduleID,
                    IsCustomResource = true,
                    ResourceType = item.ResourceType,
                    ResourcePath = item.ResourcePath
                });
            }

            return result;
        }

        private static async Task UpdateModulePageResources(ModuleView module, IEnumerable<PageResourceInfo> resources, int tabID, PortalSettings portalSettings, HttpContext context, ProgressMonitoring monitoringInstance)
        {
            var moduleID = module.ModuleID;

            await Task.Run(() =>
            {
                if (monitoringInstance != null)
                {
                    monitoringInstance.Progress($"Remove old {module.ModuleName} module page resources...", "resource-libraries,80", context);
                }

                PageResourceRepository.Instance.DeletePageResources(moduleID);

                var pageResources = PageResourceRepository.Instance.GetPageResources(tabID.ToString());

                var progressStep = 20 / resources.Count();
                var index = 80 + progressStep;
                var loadOrder = 0;

                foreach (var resource in resources)
                {
                    resource.ModuleID = moduleID;
                    resource.IsActive = !pageResources.Any(r => (r.ResourcePath ?? string.Empty).ToLower() == resource.ResourcePath.ToLower() && r.IsActive);
                    resource.LoadOrder = loadOrder++;
                    resource.CmsPageID = tabID;

                    if (monitoringInstance != null)
                    {
                        monitoringInstance.Progress($"Insert new resource at db : {resource.ResourcePath}...", $"resource-libraries,{index}", context);

                    }

                    PageResourceRepository.Instance.AddPageResource(resource);

                    index = index + progressStep;
                }

                if (monitoringInstance != null)
                {
                    monitoringInstance.Progress($"End inserting resources at the database", "resource-libraries,100", context);
                }
            });
        }

        #endregion
    }
}
