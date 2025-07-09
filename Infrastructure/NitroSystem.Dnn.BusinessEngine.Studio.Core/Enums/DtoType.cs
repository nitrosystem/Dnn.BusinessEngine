using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Enums
{
    public enum DtoType
    {
        EntityPart = 1,            // بخشی از یک Entity
        DatabaseViewPart = 2,      // بخشی از یک ویو دیتابیس
        MultipleEntities = 3,      // ترکیب چند Entity
        ServiceCalculation = 4,    // حاصل محاسبات سرویس
        ClientOnly = 5,            // فقط برای کلاینت
        BusinessCore = 6,          // مناسب برای لایه‌های بیزینسی و Core
        Other = 99                 // سایر گزینه‌ها
    }
}
