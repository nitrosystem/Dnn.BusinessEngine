using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Contracts
{
    public interface IJob
    {
        Task RunAsync(JobContext context, CancellationToken token);
    }
}
