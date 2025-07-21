using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Enums
{
    public enum OperationType
    {
        // Merge files in a category and generate a single output file to be included in the page.
        MergeResourceFiles,
        // Add the resource reference to the module without copying or modifying it.
        AddResourcePathToModuleResources
    }
}
