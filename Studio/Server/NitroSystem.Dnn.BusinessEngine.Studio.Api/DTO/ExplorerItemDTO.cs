using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class ExplorerItemDTO
    {
        public string ItemType { get; set; }
        public Guid ItemId { get; set; }
        public Guid? GroupId { get; set; }
    }
}
