using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ModuleFieldTypeTemplateRepository
    {
        public static ModuleFieldTypeTemplateRepository Instance
        {
            get
            {
                return new ModuleFieldTypeTemplateRepository();
            }
        }

        private const string CachePrefix = "BE_ModuleFieldTypeTemplates_";

        public Guid AddTemplate(ModuleFieldTypeTemplateInfo objModuleFieldTypeTemplate)
        {
            objModuleFieldTypeTemplate.TemplateID = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeTemplateInfo>();
                rep.Insert(objModuleFieldTypeTemplate);

                DataCache.ClearCache(CachePrefix);

                return objModuleFieldTypeTemplate.TemplateID;
            }
        }

        public void UpdateTemplate(ModuleFieldTypeTemplateInfo objModuleFieldTypeTemplate)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeTemplateInfo>();
                rep.Update(objModuleFieldTypeTemplate);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteTemplate(int templateID)
        {
            ModuleFieldTypeTemplateInfo objModuleFieldTypeTemplate = GetTemplate(templateID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeTemplateInfo>();
                rep.Delete(objModuleFieldTypeTemplate);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public ModuleFieldTypeTemplateInfo GetTemplate(int templateID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeTemplateInfo>();
                return rep.GetById(templateID);
            }
        }

        public IEnumerable<ModuleFieldTypeTemplateInfo> GetTemplates(string fieldType)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeTemplateInfo>();
                var result = rep.Get(fieldType).OrderBy(t => t.ViewOrder);
                return result;
            }
        }

        public ModuleFieldTypeTemplateInfo GetFieldTemplate(string FieldType, string Template)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ModuleFieldTypeTemplateInfo>();
                var result = rep.Find("WHERE FieldType=@0 and TemplateName=@1", FieldType, Template);
                return result.Any() ? result.First() : null;

            }
        }

        public IEnumerable<CssModel> GetFieldsTemplates(Guid moduleID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteQuery<CssModel>(System.Data.CommandType.Text, "Select Distinct f.FieldType,f.Template,t1.TemplatePath From dbo.BusinessEngine_ModuleFieldTypeTemplates t1 INNER JOIN dbo.BusinessEngine_ModuleFields f on t1.FieldType = f.FieldType and t1.TemplateName = f.Template WHERE f.ModuleID =@0", moduleID);
            }
        }

        public IEnumerable<CssModel> GetFieldsTemplates(string modules)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteQuery<CssModel>(System.Data.CommandType.Text, "Select Distinct f.FieldType,f.Template,t1.TemplatePath From dbo.BusinessEngine_ModuleFieldTypeTemplates t1 INNER JOIN dbo.BusinessEngine_ModuleFields f on t1.FieldType = f.FieldType and t1.TemplateName = f.Template WHERE f.ModuleID in (Select [RowValue] From dbo.ConvertListToTable(',',@0))", modules);
            }
        }
    }
}