using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngine_ModuleVariables")]
    [Cacheable("BE_ModuleVariables_View_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleVariableView
    {
        public Guid Id { get; set; }
        public Guid? AppModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public string DefaultValue { get; set; }
        public int Scope { get; set; }
        public string ModelName { get; set; }
        public string ModelTypeRelativePath { get; set; }
        public string ModelTypeFullName { get; set; }
        public string ScenarioName { get; set; }
    }
}
