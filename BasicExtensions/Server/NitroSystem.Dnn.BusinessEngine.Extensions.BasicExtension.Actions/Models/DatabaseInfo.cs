using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models
{
   public class DatabaseInfo
    {
        public IEnumerable<ParamInfo> Params { get; set; }
        public string ResultName { get; set; }
        public string ResultListName { get; set; }
        public string TotalCountIn { get; set; }
    }
}
