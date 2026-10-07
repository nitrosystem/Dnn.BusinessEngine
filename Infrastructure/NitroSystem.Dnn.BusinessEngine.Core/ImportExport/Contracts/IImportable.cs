using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface IImportable
    {
        Task<ImportResponse> ImportAsync(string json, ImportContext context);
    }
}