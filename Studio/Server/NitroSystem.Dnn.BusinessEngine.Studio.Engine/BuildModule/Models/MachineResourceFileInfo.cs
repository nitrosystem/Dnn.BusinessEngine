using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models
{
    public class MachineResourceFileInfo
    {
        // Represents the grouping type of the resource, e.g., FieldType.
        public string EntryType { get; set; }

        // Optional extra data that can be used in the merge strategy.
        public string Additional { get; set; }

        // Unique key for caching purposes.
        public string CacheKey { get; set; }

        // Full path to the actual resource file.
        public string FilePath { get; set; }

        // If false, any error in this file stops further processing.
        public bool ContinueOnError { get; set; }
    }

}
