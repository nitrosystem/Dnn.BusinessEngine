using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.PublicServices.Webservice
{
    public class Authorization
    {
        public string Type { get; set; }
        public string Bearer { get; set; }
        public BasicAuth BasicAuth { get; set; }
    }
}
