using NNitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models
{
    public class FieldTypeInfo
    {
        public string FieldType { get; set; }
        public IEnumerable<DashboardSkinFieldTypeTemplate> Templates { get; set; }
        public IEnumerable<FieldTypeThemeInfo> Themes { get; set; }
    }
}
