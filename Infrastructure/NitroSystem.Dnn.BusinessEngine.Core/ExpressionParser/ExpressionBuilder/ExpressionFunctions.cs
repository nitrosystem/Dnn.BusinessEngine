using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder
{
    public static class ExpressionFunctions
    {
        public static readonly Dictionary<string, Delegate> BuiltIn =
            new Dictionary<string, Delegate>()
            {
                ["Length"] = new Func<object, object>(arg =>
                    (arg as IEnumerable)?.Cast<object>()?.Count()),

                ["ToUpper"] = new Func<object, object>(arg =>
                    arg?.ToString()?.ToUpper()),

                ["ToLower"] = new Func<object, object>(arg =>
                    arg?.ToString()?.ToLower()),

                ["Trim"] = new Func<object, object>(arg =>
                    arg?.ToString()?.Trim()),

                ["IsNullOrEmpty"] = new Func<object, object>(arg =>
                    string.IsNullOrEmpty(arg?.ToString())),

                // فانکشن‌های چند آرگومانی
                ["Substring"] = new Func<object, object, object, object>((str, start, len) =>
                    str?.ToString()?.Substring(Convert.ToInt32(start), Convert.ToInt32(len))),

                ["Concat"] = new Func<object, object, object>((a, b) =>
                    a?.ToString() + b?.ToString()),

                ["Replace"] = new Func<object, object, object, object>((str, oldVal, newVal) =>
                    str?.ToString()?.Replace(oldVal?.ToString(), newVal?.ToString()))
            };
    }
}
