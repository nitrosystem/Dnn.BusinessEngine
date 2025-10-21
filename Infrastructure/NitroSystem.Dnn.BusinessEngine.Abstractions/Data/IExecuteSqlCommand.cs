using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts
{
    public interface IExecuteSqlCommand
    {
        Task ExecuteSqlCommandTextAsync(IUnitOfWork unitOfWork, string commandText, object param = null);
    }
}
