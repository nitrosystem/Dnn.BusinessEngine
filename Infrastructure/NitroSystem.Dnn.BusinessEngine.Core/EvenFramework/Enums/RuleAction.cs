using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Enums
{
    public enum RuleAction
    {
        Warn,       // فقط هشدار
        Cancel,     // لغو اجرا
        Retry,      // تلاش مجدد (با محدودیت)
        Rollback    // فراخوانی callback ری‌ورک
    }
}
