using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.PublicServices.Webservice
{
    public class WebServiceOptions
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public IEnumerable<ParamInfo> Params { get; set; }
        public Authorization Authorization { get; set; }
        public IEnumerable<ParamInfo> Headers { get; set; }
        public string BodyType { get; set; }
        public IEnumerable<ParamInfo> BodyFormDataItems { get; set; }
        public string BodyRaw { get; set; }
    }
}
