using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class ServiceTypeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string ServiceDomain { get; set; }
        public string ServiceType { get; set; }
        public string ServiceComponent { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string JsFile { get; set; }
        public string JsServiceFn { get; set; }
        public string BusinessControllerClass { get; set; }
        public string StudioControllerClass { get; set; }
        public bool HasResult { get; set; }
        public ServiceResultType? ResultType { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public int GroupViewOrder { get; set; }
        public int ViewOrder { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
