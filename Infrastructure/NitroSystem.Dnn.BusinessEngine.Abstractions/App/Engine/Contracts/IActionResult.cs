using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Contracts
{
    public interface IActionResult
    {
        object Data { get; set; }
        ActionResultStatus Status { get; set; }
        Exception ErrorException { get; set; }
    }
}
