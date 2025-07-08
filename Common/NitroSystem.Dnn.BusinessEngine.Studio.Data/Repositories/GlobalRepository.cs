using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class GlobalRepository
    {
        public static GlobalRepository Instance
        {
            get
            {
                return new GlobalRepository();
            }
        }

        public IEnumerable<DbRelationshipView> GetRelationships()
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DbRelationshipView>();
                return rep.Get();
            }
        }

        public IEnumerable<DbRelationshipView> GetTablesRelationships(string tables)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteQuery<DbRelationshipView>(CommandType.StoredProcedure, "dbo.BusinessEngine_GetTablesRelationships", tables);
            }
        }
    }
}