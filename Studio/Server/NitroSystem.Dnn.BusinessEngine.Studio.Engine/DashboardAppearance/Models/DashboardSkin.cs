using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models
{
    public class DashboardSkin
    {
        public Guid Id { get; set; }
        public string SkinName { get; set; }
        public string SkinImage { get; set; }
        public string Title { get; set; }
        public string SkinPath { get; set; }
        public string[] BaseCssFiles { get; set; }
        public string[] BaseJsFiles { get; set; }
        public IEnumerable<DashboardTemplate> DashboardTemplates { get; set; }
        public IEnumerable<ModuleTemplate> FormTemplates { get; set; }
        public IEnumerable<ModuleTemplate> ListTemplates { get; set; }
        public IEnumerable<ModuleTemplate> DetailsTemplates { get; set; }
        public IEnumerable<FieldTypeInfo> FieldTypes { get; set; }
        public IEnumerable<DashboardSkinLibrary> Libraries { get; set; }
    }
}