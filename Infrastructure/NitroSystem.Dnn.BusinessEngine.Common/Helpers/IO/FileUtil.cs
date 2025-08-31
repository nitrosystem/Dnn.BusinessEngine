using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.IO
{
    public static class FileUtil
    {
        public static string GetFileContent(string filename)
        {
            try
            {
                if (!File.Exists(filename)) return string.Empty;

                StreamReader objStreamReader = default(StreamReader);
                objStreamReader = File.OpenText(filename);
                string template = objStreamReader.ReadToEnd();
                objStreamReader.Close();
                return template;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static async Task<string> GetFileContentAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return string.Empty;

                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FileUtil Error: " + ex.ToString());
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

        public static void WriteFileContent(string filePath, string content, bool isCreateDirectory = true, bool isDeleteOldFile = true)
        {
            if (isDeleteOldFile && File.Exists(filePath))
                File.Delete(filePath);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var writer = new StreamWriter(filePath, append: false))
            {
                writer.Write(content);
            }
        }

        public static bool CreateTextFile(string fileName, string content, bool deleteIsExists)
        {
            try
            {
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(fileName) && deleteIsExists)
                {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(Path.GetDirectoryName(fileName))) Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                // Create a new file     
                using (FileStream fs = File.Create(fileName))
                {
                    // Add some text to file    
                    Byte[] data = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(data, 0, data.Length);

                    fs.Close();
                }

                return true;
            }
            catch (Exception Ex)
            {
                return false;
            }
        }

        public static void AppendTextFile(string monitoringFile, string message)
        {
            File.AppendAllText(monitoringFile, message);
        }

        public static bool DeleteFile(string fileName, bool raiseException = false)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                else
                    throw new Exception("File not found!.");

                return true;
            }
            catch (Exception Ex)
            {
                if (raiseException) throw Ex;

                return false;
            }
        }

        public static (bool isDeleted, Exception errorException) DeleteDirectory(string path, bool recursive = true)
        {
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

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
        }

        public static async Task MoveFileAsync(string source, string destination)
        {
            await Task.Run(() =>
            {
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
                File.Move(source, destination);
            });
        }
    }
}