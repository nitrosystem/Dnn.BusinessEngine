using NitroSystem.Dnn.BusinessEngine.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.Models.Shared
{
    public class ParamInfo
    {
        public string ParamName { get; set; }
        public string ParamType { get; set; }
        public object ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
