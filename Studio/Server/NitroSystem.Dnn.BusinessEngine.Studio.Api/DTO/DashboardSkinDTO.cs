using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class DashboardSkinDTO
    {
        public Guid DashboardId { get; set; }
        public Guid ModuleId { get; set; }
        public DashboardType DashboardType { get; set; }
        public string Skin { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
    }
}
