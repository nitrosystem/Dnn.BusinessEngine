using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ModuleRepository
    {
        public static ModuleRepository Instance
        {
            get
            {
                return new ModuleRepository();
            }
        }

        private const string CachePrefix = "BE_Modules_";
        private const string FieldCachePrefix = "BE_ModuleFields_";
        private const string ActionsCachePrefix = "BE_Actions_";


        public Guid AddModule(ModuleInfo objModuleInfo)
        {
            objModuleInfo.ModuleId = Guid.NewGuid();
            //objModuleInfo.ModuleName = Regex.Replace(objModuleInfo.ModuleName, @"[^a-zA-Z0-9]", "");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();
                rep.Insert(objModuleInfo);

                DataCache.ClearCache(CachePrefix);
                DataCache.ClearCache(FieldCachePrefix);
                DataCache.ClearCache(ActionsCachePrefix);

                return objModuleInfo.ModuleId;
            }
        }

        public void AddModuleVersion(Guid moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.Text, "UPDATE dbo.BusinessEngine_Modules SET Version = ISNULL(Version,0) + 1 WHERE ModuleId = @0", moduleId);
            }

            DataCache.ClearCache(CachePrefix);
            DataCache.ClearCache(FieldCachePrefix);
            DataCache.ClearCache(ActionsCachePrefix);
        }

        public void UpdateModule(ModuleInfo objModuleInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();
                rep.Update(objModuleInfo);
            }

            DataCache.ClearCache(CachePrefix);
            DataCache.ClearCache(FieldCachePrefix);
            DataCache.ClearCache(ActionsCachePrefix);
        }

        public int UpdateModuleVersion(Guid moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                int version = ctx.ExecuteScalar<int>(System.Data.CommandType.Text, "UPDATE dbo.BusinessEngine_Modules SET Version = Version + 1 WHERE ModuleId = @0; SELECT Version FROM dbo.BusinessEngine_Modules WHERE ModuleId = @0", moduleId);

                DataCache.ClearCache(CachePrefix);
                DataCache.ClearCache(FieldCachePrefix);
                DataCache.ClearCache(ActionsCachePrefix);

                return version;
            }
        }

        public void UpdateModuleTemplate(Guid moduleId, string template, string layoutTemplate, string layoutCss)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.Text, "UPDATE dbo.BusinessEngine_Modules SET Template = @0, LayoutTemplate = @1, LayoutCss = @2 WHERE ModuleId = @3", template, layoutTemplate, layoutCss, moduleId);

                DataCache.ClearCache(CachePrefix);
                DataCache.ClearCache(FieldCachePrefix);
                DataCache.ClearCache(ActionsCachePrefix);
            }
        }

        public void ChangeChildModulesSkin(Guid moduleId, string skin)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.Text, "UPDATE dbo.BusinessEngine_Modules SET Skin = @1 WHERE ParentID = @0", moduleId, skin);
            }

            DataCache.ClearCache(CachePrefix);
            DataCache.ClearCache(FieldCachePrefix);
            DataCache.ClearCache(ActionsCachePrefix);
        }

        public void DeleteModule(Guid moduleId)
        {
            ModuleInfo objModuleInfo = GetModule(moduleId);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();
                rep.Delete(objModuleInfo);
            }

            DataCache.ClearCache(CachePrefix);
            DataCache.ClearCache(FieldCachePrefix);
            DataCache.ClearCache(ActionsCachePrefix);
        }

        public ModuleInfo GetModule(Guid moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();
                return rep.GetById(moduleId);
            }
        }

        public string GetModuleBuilderType(Guid moduleId)
        {
            string cacheKey = CachePrefix + "ModuleBuilderType_" + moduleId;

            var result = DataCache.GetCache<string>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteScalar<string>(System.Data.CommandType.Text, "SELECT ModuleBuilderType FROM dbo.BusinessEngine_Modules WHERE ModuleId = @0", moduleId);

                    DataCache.SetCache(cacheKey, result);
                }
            }

            return result;
        }

        public ModuleView GetModuleView(Guid moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleView>();
                return rep.GetById(moduleId);
            }
        }

        public string GetModuleName(Guid moduleId)
        {
            string cacheKey = CachePrefix + "ModuleName_" + moduleId;

            var result = DataCache.GetCache<string>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteScalar<string>(System.Data.CommandType.Text, "SELECT ModuleName FROM dbo.BusinessEngine_Modules WHERE ModuleId = @0", moduleId);

                    DataCache.SetCache(cacheKey, result);
                }
            }

            return result;
        }

        public bool? IsSSR(Guid moduleId)
        {
            string cacheKey = CachePrefix + "IsSSR_" + moduleId;

            var result = DataCache.GetCache<bool?>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteScalar<bool?>(System.Data.CommandType.Text, "SELECT IsSSR FROM dbo.BusinessEngine_Modules WHERE ModuleId = @0", moduleId);

                    DataCache.SetCache(cacheKey, result);
                }
            }

            return result;
        }

        public string GetModuleScenarioName(Guid moduleId)
        {
            string cacheKey = CachePrefix + "ScenarioName_" + moduleId;

            var result = DataCache.GetCache<string>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteScalar<string>(System.Data.CommandType.Text, "SELECT ScenarioName FROM dbo.BusinessEngineView_Modules WHERE ModuleId = @0", moduleId);

                    DataCache.SetCache(cacheKey, result);
                }
            }

            return result;
        }

        public Guid GetModuleScenarioId(Guid moduleId)
        {
            string cacheKey = CachePrefix + "ScenarioId_" + moduleId;

            var result = DataCache.GetCache<Guid>(cacheKey);
            if (result == Guid.Empty)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    var rep = ctx.GetRepository<ModuleInfo>();

                    var modules = rep.Find("WHERE ModuleId = @0", moduleId);

                    result = modules.Any() ? modules.First().ScenarioId : Guid.Empty;
                }

                DataCache.SetCache(cacheKey, result);
            }

            return result;
        }

        public IEnumerable<ModuleInfo> GetModules(Guid scenarioId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();

                return rep.Get(scenarioId);
            }
        }

        public Guid? GetModuleGuidByDnnModuleId(int dnnModuleId)
        {
            string cacheKey = CachePrefix + "ModuleGuid_" + dnnModuleId;

            var result = DataCache.GetCache<Guid?>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    var rep = ctx.GetRepository<ModuleInfo>();

                    var modules = rep.Find("WHERE DnnModuleId = @0", dnnModuleId);

                    result = modules.Any() ? modules.First().ModuleId : (Guid?)null;
                }

                DataCache.SetCache(cacheKey, result);
            }

            return result;
        }

        public bool IsValidModuleName(Guid scenarioId, Guid? moduleId, string moduleName)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var count = ctx.ExecuteScalar<int?>(System.Data.CommandType.Text, "SELECT ISNULL(COUNT(ModuleName),0) FROM dbo.BusinessEngine_Modules WHERE ScenarioId=@0 and (@1 is null or ModuleId != @1) and ModuleName=@2", scenarioId, moduleId, moduleName);
                return count == null || count == 0 ? true : false;
            }
        }

        public IEnumerable<Guid> GetModuleChildsID(Guid moduleId)
        {
            string cacheKey = CachePrefix + "ModuleIds_" + moduleId;

            var result = DataCache.GetCache<IEnumerable<Guid>>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteQuery<Guid>(System.Data.CommandType.Text, "SELECT ModuleId FROM dbo.BusinessEngine_Modules WHERE ParentID = @0", moduleId);

                    DataCache.SetCache(cacheKey, result);
                }
            }

            return result;
        }

        public bool IsModuleParent(Guid moduleId)
        {
            string cacheKey = CachePrefix + "IsParent_" + moduleId;

            var result = DataCache.GetCache<bool?>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteScalar<bool?>(System.Data.CommandType.Text, "SELECT Top 1 1 FROM dbo.BusinessEngine_Modules WHERE ParentID = @0", moduleId);

                    DataCache.SetCache(cacheKey, result);
                }
            }

            return result != null ? result.Value : false;
        }

        public int GetModuleTabID(int dnnModuleId)
        {
            return DataContext.Instance().ExecuteScalar<int>(System.Data.CommandType.Text, "Select Top 1 TabID From dbo.TabModules Where ModuleId = @0", dnnModuleId);
        }

        public string GetModuleSkinName(Guid moduleId)
        {
            string cacheKey = CachePrefix + "SkinName_" + moduleId;

            var result = DataCache.GetCache<string>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = DataContext.Instance().ExecuteScalar<string>(System.Data.CommandType.Text, "Select Top 1 Skin From dbo.BusinessEngine_Modules Where ModuleId = @0", moduleId);

                    DataCache.SetCache(cacheKey, result);
                }
            }

            return result;
        }

        public IEnumerable<ModuleInfo> GetModulesByDnnTabID(int tabID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteQuery<ModuleInfo>(System.Data.CommandType.StoredProcedure, "dbo.BusinessEngine_GetModulesByTabID", tabID);
            }
        }

        public IEnumerable<ModuleInfo> GetAllModules()
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();

                return rep.Get();
            }
        }

        public IEnumerable<ModuleInfo> GetScenarioModules(Guid scenarioId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();

                return rep.Get(scenarioId);
            }
        }

        public IEnumerable<ModuleInfo> GetModulesByParentID(Guid moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleInfo>();

                return rep.Find("WHERE ParentID = @0", moduleId);
            }
        }
    }
}