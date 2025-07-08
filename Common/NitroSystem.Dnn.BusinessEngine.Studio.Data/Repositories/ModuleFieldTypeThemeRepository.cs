using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ModuleFieldTypeThemeRepository
    {
        public static ModuleFieldTypeThemeRepository Instance
        {
            get
            {
                return new ModuleFieldTypeThemeRepository();
            }
        }

        private const string CachePrefix = "BE_ModuleFieldTypeThemes_";

        public Guid AddTheme(ModuleFieldTypeThemeInfo objModuleFieldTypeTheme)
        {
            objModuleFieldTypeTheme.ThemeID = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeThemeInfo>();
                rep.Insert(objModuleFieldTypeTheme);

                DataCache.ClearCache(CachePrefix);

                return objModuleFieldTypeTheme.ThemeID;
            }
        }

        public void UpdateTheme(ModuleFieldTypeThemeInfo objModuleFieldTypeTheme)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeThemeInfo>();
                rep.Update(objModuleFieldTypeTheme);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteTheme(int themeID)
        {
            ModuleFieldTypeThemeInfo objModuleFieldTypeTheme = GetTheme(themeID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeThemeInfo>();
                rep.Delete(objModuleFieldTypeTheme);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public ModuleFieldTypeThemeInfo GetTheme(int themeID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeThemeInfo>();
                return rep.GetById(themeID);
            }
        }

        public IEnumerable<ModuleFieldTypeThemeInfo> GetThemes(string fieldType)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeThemeInfo>();
                var result = rep.Get(fieldType).OrderBy(t => t.ViewOrder);
                return result;
            }
        }

        public IEnumerable<ModuleFieldTypeThemeInfo> GetTemplateThemes(string fieldType, string templateName)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeThemeInfo>();
                var result = rep.Find("Where FieldType = @0 and TemplateName = @1", fieldType, templateName).OrderBy(t => t.ViewOrder);
                return result;
            }
        }

        public IEnumerable<CssModel> GetFieldsThemeCss(Guid moduleID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteQuery<CssModel>(System.Data.CommandType.Text, "Select Distinct f.FieldType,f.Template,t.ThemeCssPath From dbo.BusinessEngine_ModuleFields f INNER JOIN dbo.BusinessEngine_ModuleFieldTypeThemes t on f.FieldType = t.FieldType and f.Theme = t.ThemeName WHERE ISNULL(t.ThemeCssPath,'') != '' and f.ModuleID=@0", moduleID);
            }
        }

        public IEnumerable<CssModel> GetFieldsThemeCss(string modules)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteQuery<CssModel>(System.Data.CommandType.Text, "Select Distinct f.FieldType,f.Template,t.ThemeCssPath From dbo.BusinessEngine_ModuleFields f INNER JOIN dbo.BusinessEngine_ModuleFieldTypeThemes t on f.FieldType = t.FieldType and f.Theme = t.ThemeName WHERE ISNULL(t.ThemeCssPath,'') != '' and f.ModuleID in (Select [RowValue] From dbo.ConvertListToTable(',',@0))", modules);
            }
        }

        public IEnumerable<ModuleFieldInfo> GetFieldsUseSkinTheme(string modules)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldInfo>();
                var result = rep.Find("Select ModuleID,FieldID,FieldType,FieldName,Theme From dbo.BusinessEngine_ModuleFields f Where f.IsSkinTheme = 1 and f.ModuleID in (Select [RowValue] From dbo.ConvertListToTable(',',@0))", modules);
                return result;
            }
        }
    }
}