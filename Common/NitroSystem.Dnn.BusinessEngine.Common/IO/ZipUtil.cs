using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.IO
{
    public static class ZipUtil
    {
        public static string Zip(string filename, string sourceDirectory, bool recurse = true)
        {
            FastZip fastZip = new FastZip();
            fastZip.CreateZip(filename, sourceDirectory, recurse, null);

            return filename;
        }

        public static void UnZip(string zipFilePath, string extractPath)
        {
            FastZip fastZip = new FastZip();
            fastZip.ExtractZip(zipFilePath, extractPath, null);
        }
    }
}
