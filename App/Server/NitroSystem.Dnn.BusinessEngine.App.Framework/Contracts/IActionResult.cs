using System;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionResult
    {
        object Data { get; set; }
        ActionResultStatus ResultStatus { get; set; }
        Exception ErrorException { get; set; }
    }
}
