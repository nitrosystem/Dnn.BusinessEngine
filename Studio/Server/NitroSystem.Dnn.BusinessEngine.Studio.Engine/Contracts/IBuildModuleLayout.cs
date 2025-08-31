using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IBuildModuleLayout
    {
        string BuildLayout(string layoutTemplate, IDictionary<(string fieldType, string template), string> fieldTypes, IEnumerable<BuildModuleFieldDto> fields);
    }
}
