using System.IO;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Extensions
{
    public static class DirectoryExtensions
    {
        public static void Empty(this DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }
}
