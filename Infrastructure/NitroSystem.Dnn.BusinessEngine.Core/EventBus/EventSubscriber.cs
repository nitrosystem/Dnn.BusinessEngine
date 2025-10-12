using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EventBus
{
    public class EventSubscriber
    {
        public string Id { get; set; }

        // شرط به صورت Expression برای فیلتر کردن Event
        public Expression<Func<IEvent, bool>> ConditionExpression { get; set; }

        // کامپایل Expression به Func
        public Func<IEvent, bool> Condition => ConditionExpression.Compile();

        // Callback که وقتی رخداد رخ داد اجرا شود
        public Action<IEvent> Callback { get; set; }

        // کانال‌ها (می‌توان در آینده Email, SMS, SignalR اضافه کرد)
        public List<string> Channels { get; set; } = new List<string>();
    }

}
