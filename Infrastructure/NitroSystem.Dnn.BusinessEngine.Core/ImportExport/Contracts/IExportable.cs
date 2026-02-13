using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface IExportable
    {
        Task<ExportResponse> ExportAsync(ExportContext context);
    }
}
