using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class DashboardPageLiteDto : IDto
    {
        public DtoType Type => DtoType.EntityPart;

        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid DashboardId { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
    }
}
