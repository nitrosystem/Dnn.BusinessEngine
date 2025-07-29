using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Models
{
    public class ActionResult : IActionResult
    {
        public ActionResultStatus ResultStatus { get; set; }
        public object Data { get; set; }
        public Exception ErrorException { get; set; }
    }
}
