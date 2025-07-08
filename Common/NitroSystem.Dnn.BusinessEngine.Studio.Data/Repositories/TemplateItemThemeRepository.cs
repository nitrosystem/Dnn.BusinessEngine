using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class TemplateItemThemeRepository
    {
        public static TemplateItemThemeRepository Instance
        {
            get
            {
                return new TemplateItemThemeRepository();
            }
        }

        private const string CachePrefix = "BE_TemplateItemThemes_";

        public Guid AddTemplateItemTheme(TemplateThemeInfo objTemplateItemThemeInfo)
        {
            objTemplateItemThemeInfo.Id = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateThemeInfo>();
                rep.Insert(objTemplateItemThemeInfo);

                DataCache.ClearCache(CachePrefix);

                return objTemplateItemThemeInfo.Id;
            }
        }

        public void UpdateTemplateItemTheme(TemplateThemeInfo objTemplateItemThemeInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateThemeInfo>();
                rep.Update(objTemplateItemThemeInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteTemplateItemTheme(Guid themeId)
        {
            TemplateThemeInfo objTemplateItemThemeInfo = GetTemplateItemTheme(themeId);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateThemeInfo>();
                rep.Delete(objTemplateItemThemeInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public TemplateThemeInfo GetTemplateItemTheme(Guid themeId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateThemeInfo>();
                return rep.GetById(themeId);
            }
        }

        public IEnumerable<TemplateThemeInfo> GetTemplateItemThemes(Guid itemId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateThemeInfo>();

                return rep.Get(itemId);
            }
        }

        public IEnumerable<TemplateThemeInfo> GetTemplateThemes(Guid templateId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateThemeInfo>();

                return rep.Get(templateId);
            }
        }
    }
}