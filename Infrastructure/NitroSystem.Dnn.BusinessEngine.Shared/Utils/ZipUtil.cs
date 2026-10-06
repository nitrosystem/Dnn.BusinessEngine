using System.IO;
using System.IO.Compression;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    public static class ZipUtil
    {
        public static string Zip(string filename, string sourceDirectory, bool recurse = true)
        {
            // Delete the file if it already exists (CreateFromDirectory throws if file exists)
            if (File.Exists(filename))
                File.Delete(filename);
            if (recurse)
            {
                // Default behavior: includes all files and subdirectories
                ZipFile.CreateFromDirectory(sourceDirectory, filename, CompressionLevel.Optimal, false);
            }
            else
            {
                // Manual approach: only top-level files, no subdirectories
                using (var fileStream = new FileStream(filename, FileMode.Create))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    foreach (var file in Directory.GetFiles(sourceDirectory))
                    {
                        archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    }
                }
            }
            return filename;
        }

        public static void Unzip(string zipFilePath, string extractPath, bool overwrite = false)
        {
            if (overwrite && Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
        }
    }
}