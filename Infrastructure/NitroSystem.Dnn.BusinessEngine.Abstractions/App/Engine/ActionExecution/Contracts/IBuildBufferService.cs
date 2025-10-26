using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts
{
    public interface IBuildBufferService
    {
        Queue<ActionTree> BuildBufferByEvent(List<ActionDto> actions);
        Queue<ActionTree> BuildBuffer(IEnumerable<ActionDto> actions);
    }
}
