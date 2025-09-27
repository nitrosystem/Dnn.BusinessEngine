using System;
using System.IO;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    public static class FileUtil
    {
        public static string GetFileContent(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                return File.ReadAllText(filePath);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<string> GetFileContentAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task WriteFileContentAsync(string filePath, string content, bool isCreateDirectory = true, bool isDeleteOldFile = true)
        {
            if (isDeleteOldFile && File.Exists(filePath))
                File.Delete(filePath);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var writer = new StreamWriter(filePath, append: false))
            {
                await writer.WriteAsync(content);
            }
        }

        public static (bool isDeleted, Exception error) DeleteDirectory(string path, bool recursive = true)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (false, new ArgumentException("Path cannot be null or empty."));

            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }
    }
}