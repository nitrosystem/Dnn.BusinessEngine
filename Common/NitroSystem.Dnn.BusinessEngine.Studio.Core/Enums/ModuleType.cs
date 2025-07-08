using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Enums
{
    public enum ModuleType
    {
        [Description("Dashboard")]
        Dashboard = 0,

        [Description("Form")]
        Form = 1,

        [Description("List")]
        List = 2,

        [Description("Details")]
        Details = 3
    }

}
