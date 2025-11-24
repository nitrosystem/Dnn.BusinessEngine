using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.AppModel
{
    public class AppModelPropertyViewModel
    {
        public Guid Id { get; set; }
        public Guid AppModelId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int ViewOrder { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
