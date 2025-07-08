using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Dto
{
    public class DashboardPageModuleDto : IDto
    {
        public DtoType Type => DtoType.MultipleEntities;

        public Guid Id { get; set; }
        public Guid DashboardModuleId { get; set; }
        public Guid PageId { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string PageName { get; set; }
        public string ModuleType { get; set; }
        public string ModuleBuilderType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
    }
}
