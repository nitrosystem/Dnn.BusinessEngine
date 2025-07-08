using NitroSystem.Dnn.BusinessEngine.Core.Appearance;
using NitroSystem.Dnn.BusinessEngine.Core.Common;
using NitroSystem.Dnn.BusinessEngine.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.ModuleBuilder
{
    internal static class ModuleResourcesContentService
    {
        #region Fields Styles Methods

        internal static async Task<string> GetModuleFieldsStyles(ModuleView module, HttpContext context)
        {
            StringBuilder styles = new StringBuilder();

            await Task.Run(() =>
            {
                //Get field type template paths and get base css from them
                var templates = ModuleFieldTypeTemplateRepository.Instance.GetFieldsTemplates(module.ModuleID);
                foreach (var item in templates)
                {
                    string baseFile = Path.GetDirectoryName(item.TemplatePath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions")) + @"\base.css";
                    var cssContent = FileUtil.GetFileContent(context.Server.MapPath(baseFile));
                    if (!string.IsNullOrEmpty(cssContent))
                    {
                        cssContent = cssContent.Replace("--T-", "--" + module.Template.ToLower());
                        cssContent = cssContent.Replace("--B1", "--" + item.FieldType.ToLower());
                        cssContent = cssContent.Replace("--B2", "--" + item.FieldType.ToLower() + "--" + item.Template.ToLower());

                        styles.AppendLine("/* --------------------------------------------------------");
                        styles.AppendLine(string.Format(" -----   {0}   -----", baseFile));
                        styles.AppendLine("-----------------------------------------------------------*/");
                        styles.AppendLine(Environment.NewLine);
                        styles.AppendLine(cssContent);
                    }
                }

                //Get field type theme css files
                var themes = ModuleFieldTypeThemeRepository.Instance.GetFieldsThemeCss(module.ModuleID);
                if (themes != null && themes.Any())
                {
                    foreach (var item in themes)
                    {
                        string cssFilePath = item.ThemeCssPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                        var fieldStyles = FileUtil.GetFileContent(context.Server.MapPath(cssFilePath));

                        if (!string.IsNullOrEmpty(fieldStyles))
                        {
                            fieldStyles = fieldStyles.Replace("--T-", "--" + module.Template.ToLower());
                            fieldStyles = fieldStyles.Replace("--B1", "--" + item.FieldType.ToLower());
                            fieldStyles = fieldStyles.Replace("--B2", "--" + item.FieldType.ToLower() + "--" + item.Template.ToLower());

                            styles.AppendLine("/* --------------------------------------------------------");
                            styles.AppendLine(string.Format(" -----   {0}   -----", cssFilePath));
                            styles.AppendLine("-----------------------------------------------------------*/");
                            styles.AppendLine(Environment.NewLine);
                            styles.AppendLine(fieldStyles);
                        }
                    }

                    styles.AppendLine(Environment.NewLine);
                }
            });

            return styles.ToString();
        }

        internal static async Task<string> GetModulesFieldsStyles(Guid moduleID, HttpContext context)
        {
            StringBuilder styles = new StringBuilder();

            await Task.Run(() =>
            {
                List<Guid> moduleIds = new List<Guid>() { moduleID };
                moduleIds.AddRange(ModuleRepository.Instance.GetModuleChildsID(moduleID));

                //Get field type template paths and get base css from them
                var templates = ModuleFieldTypeTemplateRepository.Instance.GetFieldsTemplates(string.Join(",", moduleIds));
                foreach (var template in templates)
                {
                    string baseFile = Path.GetDirectoryName(template.TemplatePath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions")) + @"\base.css";
                    var cssContent = FileUtil.GetFileContent(context.Server.MapPath(baseFile));
                    if (!string.IsNullOrEmpty(cssContent))
                    {
                        cssContent = cssContent.Replace("--B1", "--" + template.FieldType.ToLower());
                        cssContent = cssContent.Replace("--B2", "--" + template.FieldType.ToLower() + "--" + template.Template.ToLower());

                        styles.AppendLine("/* --------------------------------------------------------");
                        styles.AppendLine(string.Format(" -----   {0}   -----", baseFile));
                        styles.AppendLine("-----------------------------------------------------------*/");
                        styles.AppendLine(Environment.NewLine);
                        styles.AppendLine(cssContent);
                        styles.AppendLine(Environment.NewLine);
                        styles.AppendLine("/* --------------------------------------------------------*/");
                    }
                }


                //Get field type theme css files
                var themes = ModuleFieldTypeThemeRepository.Instance.GetFieldsThemeCss(string.Join(",", moduleIds));
                if (themes != null && themes.Any())
                {
                    foreach (var theme in themes)
                    {
                        string cssFilePath = theme.ThemeCssPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                        var fieldStyles = FileUtil.GetFileContent(context.Server.MapPath(cssFilePath));
                        if (!string.IsNullOrEmpty(fieldStyles))
                        {
                            fieldStyles = fieldStyles.Replace("--B1", "--" + theme.FieldType.ToLower());
                            fieldStyles = fieldStyles.Replace("--B2", "--" + theme.FieldType.ToLower() + "--" + theme.Template.ToLower());

                            styles.AppendLine("/* --------------------------------------------------------");
                            styles.AppendLine(string.Format(" -----   {0}   -----", cssFilePath));
                            styles.AppendLine("-----------------------------------------------------------*/");
                            styles.AppendLine(Environment.NewLine);
                            styles.AppendLine(fieldStyles);
                            styles.AppendLine(Environment.NewLine);
                            styles.AppendLine("/*-----------------------------------------------------------*/");
                        }
                    }

                    styles.AppendLine(Environment.NewLine);
                }
            });

            return styles.ToString();
        }

        internal static async Task<string> GetDashboardModulesFieldsStylesThatUsedSkinTheme(Guid moduleID, DashboardSkin skin, HttpContext context)
        {
            StringBuilder styles = new StringBuilder();

            await Task.Run(() =>
            {
                List<Guid> moduleIds = new List<Guid>() { moduleID };
                moduleIds.AddRange(ModuleRepository.Instance.GetModuleChildsID(moduleID));

                var fields = ModuleFieldTypeThemeRepository.Instance.GetFieldsUseSkinTheme(string.Join(",", moduleIds));
                foreach (var field in fields)
                {
                    var cssFilePath = ((((skin.FieldTypes ?? Enumerable.Empty<FieldTypeInfo>()).FirstOrDefault(ft => ft.FieldType == field.FieldType) ?? new FieldTypeInfo()).Themes ?? Enumerable.Empty<FieldTypeThemeInfo>()).FirstOrDefault(t => t.ThemeName == field.Theme) ?? new FieldTypeThemeInfo()).ThemeCssPath;
                    var fieldStyles = FileUtil.GetFileContent(context.Server.MapPath(cssFilePath));
                    if (!string.IsNullOrEmpty(fieldStyles))
                    {
                        styles.AppendLine("//Start Skin Theme Css : " + cssFilePath);
                        styles.AppendLine(Environment.NewLine);
                        styles.AppendLine("/* --------------------------------------------------------");
                        styles.AppendLine(string.Format(" -----   {0}   -----", cssFilePath));
                        styles.AppendLine("-----------------------------------------------------------*/");
                        styles.AppendLine(Environment.NewLine);
                        styles.AppendLine(fieldStyles);
                        styles.AppendLine(Environment.NewLine);
                        styles.AppendLine("//End Skin Theme Css : " + cssFilePath);
                    }

                    styles.AppendLine(Environment.NewLine);
                }
            });

            return styles.ToString();
        }

        #endregion

        #region Fields Directives & Actions Scripts Methods

        internal static async Task<string> GetModuleFieldsScripts(Guid moduleID, HttpContext context)
        {
            StringBuilder scripts = new StringBuilder();

            await Task.Run(() =>
            {
                var fieldTypes = ModuleFieldTypeRepository.Instance.GetFieldTypes();
                var moduleFieldTypes = ModuleFieldRepository.Instance.GetFieldTypes(moduleID);
                if (moduleFieldTypes != null && moduleFieldTypes.Any())
                {
                    foreach (var fieldTypeName in moduleFieldTypes)
                    {
                        var fieldType = fieldTypes.FirstOrDefault(ft => ft.FieldType == fieldTypeName);
                        if (fieldType != null)
                        {
                            if (!string.IsNullOrEmpty(fieldType.DirectiveJsPath))
                            {
                                scripts.AppendLine("//Start Directive of Field Type : " + fieldTypeName);

                                string jsFilePath = fieldType.DirectiveJsPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                                var fieldScript = FileUtil.GetFileContent(context.Server.MapPath(jsFilePath));

                                scripts.AppendLine(fieldScript);
                                scripts.AppendLine("//End Directive of Field Type : " + fieldTypeName);
                                scripts.AppendLine(Environment.NewLine);
                            }

                            if (!string.IsNullOrEmpty(fieldType.FieldJsPath))
                            {
                                scripts.AppendLine("//Start Field Type : " + fieldTypeName);

                                string jsFilePath = fieldType.FieldJsPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                                var fieldScript = FileUtil.GetFileContent(context.Server.MapPath(jsFilePath));

                                scripts.AppendLine(fieldScript);
                                scripts.AppendLine("//End Field Type : " + fieldTypeName);
                                scripts.AppendLine(Environment.NewLine);
                            }
                        }
                    }

                    scripts.AppendLine(Environment.NewLine);
                }
            });

            return scripts.ToString();
        }

        internal static async Task<string> GetModulesFieldsScripts(Guid moduleID, HttpContext context)
        {
            StringBuilder scripts = new StringBuilder();

            await Task.Run(() =>
            {
                List<Guid> moduleIds = new List<Guid>() { moduleID };
                moduleIds.AddRange(ModuleRepository.Instance.GetModuleChildsID(moduleID));

                var fieldTypes = ModuleFieldTypeRepository.Instance.GetFieldTypes();

                var moduleFieldTypes = ModuleFieldRepository.Instance.GetFieldTypes(string.Join(",", moduleIds));
                foreach (var fieldTypeName in moduleFieldTypes)
                {
                    var fieldType = fieldTypes.FirstOrDefault(ft => ft.FieldType == fieldTypeName);
                    if (fieldType != null)
                    {
                        if (!string.IsNullOrEmpty(fieldType.FieldJsPath))
                        {
                            scripts.AppendLine("//Start Field Type : " + fieldTypeName);

                            string jsFilePath = fieldType.FieldJsPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                            var fieldScript = FileUtil.GetFileContent(context.Server.MapPath(jsFilePath));

                            scripts.AppendLine(fieldScript);

                            scripts.AppendLine("//End Field Type : " + fieldTypeName);
                            scripts.AppendLine(Environment.NewLine);
                        }

                        if (!string.IsNullOrEmpty(fieldType.DirectiveJsPath))
                        {
                            scripts.AppendLine("//Start Directive of Field Type : " + fieldTypeName);

                            string jsFilePath = fieldType.DirectiveJsPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                            var fieldScript = FileUtil.GetFileContent(context.Server.MapPath(jsFilePath));

                            scripts.AppendLine(fieldScript);

                            scripts.AppendLine("//End Directive of Field Type : " + fieldTypeName);
                            scripts.AppendLine(Environment.NewLine);
                        }
                    }
                }

                scripts.AppendLine(Environment.NewLine);
            });

            return scripts.ToString();
        }

        internal static async Task<string> GetModuleActionsScripts(Guid moduleID, HttpContext context)
        {
            StringBuilder scripts = new StringBuilder();

            await Task.Run(() =>
            {
                var actionTypes = ActionTypeRepository.Instance.GetActionTypes();
                var moduleActionTypes = ActionRepository.Instance.GetActionTypes(moduleID);
                if (moduleActionTypes != null && moduleActionTypes.Any())
                {
                    foreach (var actionTypeName in moduleActionTypes)
                    {
                        var actionType = actionTypes.FirstOrDefault(ft => ft.ActionType == actionTypeName);
                        if (actionType != null && !string.IsNullOrEmpty(actionType.ActionJsPath))
                        {
                            scripts.AppendLine("//Start Action Type : " + actionTypeName);

                            string jsFilePath = actionType.ActionJsPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                            var actionScript = FileUtil.GetFileContent(context.Server.MapPath(jsFilePath));

                            scripts.AppendLine(actionScript);

                            scripts.AppendLine("//End Action Type : " + actionTypeName);
                            scripts.AppendLine(Environment.NewLine);
                        }
                    }

                    scripts.AppendLine(Environment.NewLine);
                }
            });

            return scripts.ToString();
        }

        internal static async Task<string> GetModulesActionsScripts(Guid moduleID, HttpContext context)
        {
            StringBuilder scripts = new StringBuilder();

            await Task.Run(() =>
            {
                List<Guid> moduleIds = new List<Guid>() { moduleID };
                moduleIds.AddRange(ModuleRepository.Instance.GetModuleChildsID(moduleID));

                var actionTypes = ActionTypeRepository.Instance.GetActionTypes();

                var moduleActionTypes = ActionRepository.Instance.GetActionTypes(string.Join(",", moduleIds));
                foreach (var actionTypeName in moduleActionTypes)
                {
                    var actionType = actionTypes.FirstOrDefault(ft => ft.ActionType == actionTypeName);
                    if (actionType != null && !string.IsNullOrEmpty(actionType.ActionJsPath))
                    {
                        scripts.AppendLine("//Start Action Type : " + actionTypeName);

                        string jsFilePath = actionType.ActionJsPath.Replace("[EXTPATH]", "~/DesktopModules/BusinessEngine/extensions");
                        var actionScript = FileUtil.GetFileContent(context.Server.MapPath(jsFilePath));

                        scripts.AppendLine(actionScript);

                        scripts.AppendLine("//End Action Type : " + actionTypeName);
                        scripts.AppendLine(Environment.NewLine);
                    }
                }

                scripts.AppendLine(Environment.NewLine);
            });

            return scripts.ToString();
        }

        #endregion
    }
}
