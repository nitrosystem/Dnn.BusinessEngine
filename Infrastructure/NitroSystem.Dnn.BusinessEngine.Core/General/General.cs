using ICSharpCode.SharpZipLib.Zip;

namespace NitroSystem.Dnn.BusinessEngine.Core.General
{
    public static class General

    {
        public static string Zip(string filename, string sourceDirectory, bool recurse = true)
        {
            FastZip fastZip = new FastZip();
            fastZip.CreateZip(filename, sourceDirectory, recurse, null);

            return filename;
        }

        public static void Unzip(string zipFilePath, string extractPath)
        {
            FastZip fastZip = new FastZip();
            fastZip.ExtractZip(zipFilePath, extractPath, null);
        }
    }
}
