using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Enums
{
    public enum VariableType
    {
        String,
        Int,
        Long,
        Decimal,
        Float,
        Double,
        Boolean,
        DateTime,
        Guid,

        AppModel,
        Object,

        AppModelList,

        Unknown               // Optional for uninitialized states
    }
}
