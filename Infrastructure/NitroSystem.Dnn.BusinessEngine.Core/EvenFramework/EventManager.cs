using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class EventManager
    {
        //private readonly IList<IEventObserver> _observers = new List<IEventObserver>();
        //private readonly ILogger _logger; // فرض: هر پروژه ممکنه ILogger داشته باشه
        //private readonly ConcurrentDictionary<Guid, EventContext> _running = new ConcurrentDictionary<Guid, EventContext>();
        
        [ThreadStatic]
        private static EventContext _current;
        public EventContext CurrentContext => _current;

        //public EventManager(/*ILogger logger = null*/)
        //{
        //    //_logger = logger;
        //}

        //public void Subscribe(IEventObserver observer)
        //{
        //    if (observer == null) throw new ArgumentNullException(nameof(observer));
        //    _observers.Add(observer);
        //}

        public async Task<T> ExecuteAsync<T>(string eventName, Func<EventContext, Task<T>> action)
        {
            var context = new EventContext(eventName);
            _current = context;

            context.Start();

            try
            {
                var result = await action(context);
                context.Complete();
                return result;
            }
            catch (Exception ex)
            {
                context.Fail(ex);
                throw;
            }
            finally
            {
                _current = null;
            }
        }

        // Safety wrapper so a failing observer doesn't break execution
        //private async Task SafeInvoke(Func<Task> call)
        //{
        //    try
        //    {
        //        await call();
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger?.LogError(ex, "Observer threw exception");
        //    }
        //}
       
        public IDisposable CreateNodeScope(string name)
        {
            var node = _current?.AddNode(name) ?? new EventNode(name);
            return node;
        }   
    }
}
