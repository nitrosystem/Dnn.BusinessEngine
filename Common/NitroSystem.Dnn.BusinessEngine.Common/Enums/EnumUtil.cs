using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.Enums
{
   public static class EnumUtil
    {
        public static string GetDescription<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var field = typeof(TEnum).GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
