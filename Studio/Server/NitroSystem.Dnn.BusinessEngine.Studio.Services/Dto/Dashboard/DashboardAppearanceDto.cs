using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class DashboardAppearanceDto : IDto
    {
        public DtoType Type => DtoType.ServiceCalculation;

        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid SkinId { get; set; }
        public DashboardType DashboardType { get; set; }
        public string Skin { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string SkinPath { get; set; }
    }
}
