using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.DnnService
{
    public class ServiceUserRegisterOptions
    {
        public int RegisterType { get; set; }
        public bool EmailAsUsername { get; set; }
        public bool RegisterInAllPortals { get; set; }
    }
}