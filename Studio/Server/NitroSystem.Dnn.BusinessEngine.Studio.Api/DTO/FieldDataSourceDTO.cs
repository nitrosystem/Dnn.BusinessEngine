using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class FieldDataSourceDTO
    {
        public Guid ModuleId { get; set; }
        public Guid FieldID { get; set; }
        public string ConnectionID { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IDictionary<string, object> Form { get; set; }
        public string PageUrl { get; set; }
    }
}
