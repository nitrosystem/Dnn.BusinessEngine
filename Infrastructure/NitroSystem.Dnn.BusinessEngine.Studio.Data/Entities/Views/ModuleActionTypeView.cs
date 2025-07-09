using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_ModuleCustomLibraries")]
    [Cacheable("BE_ModuleActionTypes_Views_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleActionTypeView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string ActionType { get; set; }
        public string ActionJsPath { get; set; }
        public string BusinessControllerClass { get; set; }
    }
}
