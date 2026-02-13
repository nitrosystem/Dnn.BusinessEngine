using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Events
{
    public delegate Task BackgroundJobProgressHandler(object payload);
    public delegate Task BackgroundJobErrorHandler(object payload);
}
