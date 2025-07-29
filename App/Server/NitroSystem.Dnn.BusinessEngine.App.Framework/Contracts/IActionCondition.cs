using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Common.Globals;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionCondition
    {
        bool IsTrueConditions(IModuleData moduleData, IEnumerable<ExpressionInfo> conditions);
    }
}
