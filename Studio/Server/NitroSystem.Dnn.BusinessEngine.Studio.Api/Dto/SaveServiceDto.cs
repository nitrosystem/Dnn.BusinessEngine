using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto
{
    public class SaveServiceDto
    {
        public ServiceViewModel Service { get; set; }
        public string ExtensionServiceJson { get; set; }

    }
}
