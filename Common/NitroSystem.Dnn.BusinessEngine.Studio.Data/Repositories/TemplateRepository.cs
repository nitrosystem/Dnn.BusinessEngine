using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class TemplateRepository
    {
        public static TemplateRepository Instance
        {
            get
            {
                return new TemplateRepository();
            }
        }

        private const string CachePrefix = "BE_Templates_";

        public Guid AddTemplate(TemplateInfo objTemplateInfo)
        {
            objTemplateInfo.Id = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateInfo>();
                rep.Insert(objTemplateInfo);

                DataCache.ClearCache(CachePrefix);

                return objTemplateInfo.Id;
            }
        }

        public void UpdateTemplate(TemplateInfo objTemplateInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateInfo>();
                rep.Update(objTemplateInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteTemplate(Guid templateID)
        {
            TemplateInfo objTemplateInfo = GetTemplate(templateID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateInfo>();
                rep.Delete(objTemplateInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public TemplateInfo GetTemplate(Guid templateID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateInfo>();
                return rep.GetById(templateID);
            }
        }

        public TemplateInfo GetTemplate(string templateName)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateInfo>();
                var result = rep.Find("Where TemplateName = @0", templateName);
                return result.Any() ? result.First() : null;
            }
        }

        public IPagedList<TemplateInfo> GetTemplates(int pageIndex, int pageSize, string searchText)
        {
            if (searchText == null) searchText = "";

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateInfo>();

                return rep.Find(pageIndex, pageSize, "WHERE TemplateName like N'%' + @0 + '%' ORDER BY ViewOrder", searchText);
            }
        }

        public IEnumerable<TemplateInfo> GetTemplates(string moduleType,string searchText)
        {
            if (searchText == null) searchText = "";

            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteQuery<TemplateInfo>(System.Data.CommandType.Text, "SELECT * FROM dbo.BusinessEngine_Templates t1 WHERE EXISTS(SELECT ItemType FROM dbo.BusinessEngine_TemplateItems WHERE TemplateId = t1.Id and ItemType = 'Module' and ItemSubtype = @0) and TemplateName like N'%' + @1 + '%' ORDER BY ViewOrder", moduleType, searchText);
            }
        }


        public IEnumerable<TemplateInfo> GetTemplates()
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateInfo>();

                return rep.Get();
            }
        }
    }
}