using System;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace NitroSystem.Dnn.BusinessEngine.Core.ConditionParser
{
    class Program
    {
        static void Main()
        {
            // 1. دیکشنری داده‌ها
            IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "Name", "John" },
            { "Age", 30 },
            { "IsActive", true }
        };

            // 2. مشخص کردن پارامتر و مقدار ثابت برای مقایسه
            var parameter = Expression.Parameter(typeof(IDictionary<string, object>), "data");

            // 3. دسترسی به کلید "Age" و بررسی مقدار اون
            var property = Expression.Call(parameter, "get_Item", null, Expression.Constant("Age"));
            var constant = Expression.Constant(25);

            // 4. ساخت شرط بزرگتر از
            var greaterThan = Expression.GreaterThan(property, constant);

            // 5. کامپایل کردن Expression به Func
            var lambda = Expression.Lambda<Func<IDictionary<string, object>, bool>>(greaterThan, parameter);
            var func = lambda.Compile();

            // 6. اعمال شرط
            bool result = func(data);
            Console.WriteLine($"Is Age greater than 25? {result}"); // خروجی: True
        }
    }

}
