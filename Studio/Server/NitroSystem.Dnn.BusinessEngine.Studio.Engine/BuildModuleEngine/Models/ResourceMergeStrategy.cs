using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Models
{
    public class ResourceMergeStrategy
    {
        // Callback to generate the final merged content using all provided resources.
        public Func<object[], string> MergedCallback { get; set; }

        public bool IgnoreLoadingResourceFiles { get; set; }

        // Required: output path for the merged files resource path.
        public string MergedResourcePath { get; set; }

        // Required: output path for the merged file. If empty, merging won't occur.
        public string MergedOutputFilePath { get; set; }
    }
}
