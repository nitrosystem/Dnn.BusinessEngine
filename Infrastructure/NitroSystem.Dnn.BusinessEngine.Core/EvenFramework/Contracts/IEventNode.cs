using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts
{
    // هر Node (مثلاً یک Middleware، Service، یا Step) باید این رفتار را داشته باشد
    public interface IEventNode
    {
        string Name { get; }
        Task ExecuteAsync(EventContext context);
    }
}
