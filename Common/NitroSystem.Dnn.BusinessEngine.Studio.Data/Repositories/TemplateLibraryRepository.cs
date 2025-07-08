using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class TemplateLibraryRepository
    {
        public static TemplateLibraryRepository Instance
        {
            get
            {
                return new TemplateLibraryRepository();
            }
        }

        private const string CachePrefix = "BE_TemplateLibraries_";

        public Guid AddTemplateLibrary(TemplateLibraryInfo objTemplateLibraryInfo)
        {
            objTemplateLibraryInfo.Id = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateLibraryInfo>();
                rep.Insert(objTemplateLibraryInfo);

                DataCache.ClearCache(CachePrefix);

                return objTemplateLibraryInfo.Id;
            }
        }

        public void UpdateTemplateLibrary(TemplateLibraryInfo objTemplateLibraryInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateLibraryInfo>();
                rep.Update(objTemplateLibraryInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteTemplateLibrary(Guid templateLibraryID)
        {
            TemplateLibraryInfo objTemplateLibraryInfo = GetTemplateLibrary(templateLibraryID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateLibraryInfo>();
                rep.Delete(objTemplateLibraryInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public TemplateLibraryInfo GetTemplateLibrary(Guid templateLibraryID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateLibraryInfo>();
                return rep.GetById(templateLibraryID);
            }
        }

        public IEnumerable<TemplateLibraryInfo> GetTemplateLibraries(Guid templateID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<TemplateLibraryInfo>();

                return rep.Get(templateID);
            }
        }
    }
}