using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models
{
    public class ResourceMergeStrategy
    {
        // Callback to generate the final merged content using all provided resources.
        public Func<object[], string> MergedCallback { get; set; }

        // Optional: content to be placed at the beginning of the merged output file.
        public string BeforeContent { get; set; }

        // Required: output path for the merged files resource path.
        public string MergedResourcePath { get; set; }

        // Required: output path for the merged file. If empty, merging won't occur.
        public string MergedOutputFilePath { get; set; }

        // Optional: content to be added at the end of the merged output file.
        public string AfterContent { get; set; }
    }
}
