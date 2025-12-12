using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts
{
    public interface IBuildBufferService
    {
        Queue<ActionTree> BuildBuffer(List<ActionDto> actions);
    }
}
