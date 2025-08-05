using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database
{
    public class LoginUserService : IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public string StoredProcedureName { get; set; }
        public string Query { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
