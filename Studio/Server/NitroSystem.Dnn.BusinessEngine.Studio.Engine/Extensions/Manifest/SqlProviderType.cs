using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Extensions.Manifest
{
    public enum SqlProviderType
    {
        [Description("Install")]
        Install = 1,
        [Description("Uninstall")]
        Uninstall = 2
    }
}
