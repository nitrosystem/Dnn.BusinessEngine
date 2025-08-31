using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class CustomizeStylesDTO
    {
        public Guid TemplateId { get; set; }
        public string ItemName { get; set; }
        public string ToolboxHtmlTemplate { get; set; }
        public IEnumerable<string> Fields { get; set; }
    }
}
