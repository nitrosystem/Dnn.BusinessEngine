using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Dto
{
    public class ServiceDto
    {
        public string ServiceName { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasResult { get; set; }
        public ServiceResultType? ResultType { get; set; }
        public IEnumerable<string> AuthorizationRunService { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
