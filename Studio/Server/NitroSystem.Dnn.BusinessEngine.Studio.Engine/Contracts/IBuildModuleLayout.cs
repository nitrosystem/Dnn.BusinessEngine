using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IBuildModuleLayout
    {
        string BuildLayout(string layoutTemplate, IDictionary<(string fieldType, string template), string> fieldTypes, IEnumerable<BuildModuleFieldDto> fields);
    }
}
