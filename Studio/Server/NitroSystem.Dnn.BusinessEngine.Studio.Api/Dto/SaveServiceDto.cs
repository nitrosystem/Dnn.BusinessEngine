using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Service;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto
{
    public class SaveServiceDto
    {
        public ServiceViewModel Service { get; set; }
        public string ExtensionServiceJson { get; set; }
    }
}
