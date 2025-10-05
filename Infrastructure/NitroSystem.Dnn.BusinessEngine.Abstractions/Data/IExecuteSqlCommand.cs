using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts
{
    public interface IExecuteSqlCommand
    {
        Task<int> ExecuteSqlCommandTextAsync(string commandText, object param = null);
    }
}
