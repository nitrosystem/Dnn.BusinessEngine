using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Contracts
{
    public interface ICustomTask
    {
        string Name { get; set; }
        bool ContinueOnError { get; set; }
        Task<TaskResult> ExecuteAsync();
    }
}
