using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class LibraryResourceRepository
    {
        public static LibraryResourceRepository Instance
        {
            get
            {
                return new LibraryResourceRepository();
            }
        }

        private const string CachePrefix = "BE_LibraryResources_";

        public Guid AddResource(LibraryResourceInfo objLibraryResourceInfo)
        {
            objLibraryResourceInfo.ResourceID = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LibraryResourceInfo>();
                rep.Insert(objLibraryResourceInfo);

                DataCache.ClearCache(CachePrefix);

                return objLibraryResourceInfo.ResourceID;
            }
        }

        public void UpdateResource(LibraryResourceInfo objLibraryResourceInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LibraryResourceInfo>();
                rep.Update(objLibraryResourceInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteResource(Guid resourceID)
        {
            LibraryResourceInfo objLibraryResourceInfo = GetResource(resourceID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LibraryResourceInfo>();
                rep.Delete(objLibraryResourceInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public LibraryResourceInfo GetResource(Guid resourceID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LibraryResourceInfo>();
                return rep.GetById(resourceID);
            }
        }

        public IEnumerable<LibraryResourceInfo> GetResources(Guid libraryID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LibraryResourceInfo>();
                return rep.Get(libraryID).OrderBy(r => r.LoadOrder);
            }
        }

        public IEnumerable<LibraryResourceInfo> GetResources(string libraryName, string Version)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LibraryResourceInfo>();
                return rep.Find("Select * From dbo.BusinessEngine_Libraries l INNER JOIN dbo.BusinessEngine_LibraryResources r on l.LibraryID = r.LibraryID Where l.LibraryName = @0 and l.Version = @1", libraryName, Version).OrderBy(r => r.LoadOrder);
            }
        }
    }
}