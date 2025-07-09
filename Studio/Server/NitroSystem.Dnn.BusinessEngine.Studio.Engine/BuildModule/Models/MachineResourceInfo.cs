using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models
{
    public class MachineResourceInfo
    {
        public Guid ModuleId { get; set; }
        public List<MachineResourceFileInfo> ResourceFiles { get; set; }

        // If MergeResourceFiles, creates a new output file that includes the specified resource files.
        public OperationType OperationType { get; set; }

        // Defines how the resource files will be merged.
        public ResourceMergeStrategy MergeStrategy { get; set; }

        // If true, the service should trace the operation and notify the client(s) of progress and messages.
        public bool TraceAndProgress { get; set; }

        //Check condition for process resources. if value is null, ignore condition
        public string Condition { get; set; }

        // If false, and an error occurs, the entire build process should fail.
        public bool ContinueOnError { get; set; }

        //The action type specifies whether to read the contents of the files or only get the file addresses.
        public ActionType ActionType { get; set; }
        
        //If true, the resource add to the module resources
        public bool AddToResources { get; set; }

        // Determines the order in which resource files are processed.
        public int Order { get; set; }
    }
}
