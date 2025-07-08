using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contract
{
    public interface IModule
    {
        Guid ModuleId { get; set; }
        string ModuleType { get; set; }
        string Wrapper { get; set; }
        string Template { get; set; }
        string Theme { get; set; }
        string LayoutTemplate { get; set; }
        string LayoutCss { get; set; }
    }
}
