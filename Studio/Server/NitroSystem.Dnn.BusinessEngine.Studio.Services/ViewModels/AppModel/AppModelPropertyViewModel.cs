using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.AppModel
{
    public class AppModelPropertyViewModel
    {
        public Guid Id { get; set; }
        public Guid AppModelId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int ViewOrder { get; set; }
    }
}
