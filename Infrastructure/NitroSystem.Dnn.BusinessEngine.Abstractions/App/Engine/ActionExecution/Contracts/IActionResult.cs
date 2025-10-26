using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Enums;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts
{
    public interface IActionResult
    {
        object Data { get; set; }
        ActionResultStatus Status { get; set; }
        Exception ErrorException { get; set; }
    }
}
