using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class ServiceParamViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string ParamName { get; set; }
        public string ParamType { get; set; }
        public int ViewOrder { get; set; }
    }
}
