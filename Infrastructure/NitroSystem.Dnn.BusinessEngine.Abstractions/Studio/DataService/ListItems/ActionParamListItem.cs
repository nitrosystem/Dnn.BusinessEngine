using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class ActionParamListItem
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
