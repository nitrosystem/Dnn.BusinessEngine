using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder
{
    public static class ExpressionFunctions
    {
        public static readonly Dictionary<string, Delegate> BuiltIn =
            new Dictionary<string, Delegate>()
            {
                ["ArraySize"] = new Func<object, object>((arg) =>
                {
                    if (arg == null)
                        return 0;
                    else if (arg is string)
                    {
                        var list = JsonConvert.DeserializeObject<JArray>(arg.ToString());
                        return list.Count;
                    }
                    else if (arg.GetType() == typeof(IEnumerable))
                        return (arg as IEnumerable)?.Cast<object>()?.Count();
                    else
                        throw new Exception("Param is not IEnumerable");
                }),
                ["Length"] = new Func<object, object>((arg) =>
                {
                    if (arg is string)
                    {
                        return arg.ToString().Length;
                    }

                    throw new Exception("Param is not String");
                }),
                ["ToUpper"] = new Func<object, object>(arg =>
                    arg?.ToString()?.ToUpper()),

                ["ToLower"] = new Func<object, object>(arg =>
                    arg?.ToString()?.ToLower()),

                ["Trim"] = new Func<object, object>(arg =>
                    arg?.ToString()?.Trim()),

                ["IsNullOrEmpty"] = new Func<object, object>(arg =>
                    string.IsNullOrEmpty(arg?.ToString())),

                // فانکشن‌های چند آرگومانی
                ["Substring"] = new Func<string, object, object, string>((str, start, len) =>
                    str?.Length <= Convert.ToInt32(len)
                    ? str
                    : str?.Substring(Convert.ToInt32(start), Convert.ToInt32(len))),

                ["Concat"] = new Func<object, object, object>((a, b) =>
                    a?.ToString() + b?.ToString()),

                ["Replace"] = new Func<object, object, object, object>((str, oldVal, newVal) =>
                    str?.ToString()?.Replace(oldVal?.ToString(), newVal?.ToString())),

                ["DateFormat"] = new Func<string, string, string>((dateStr, format) =>
                {
                    var date = DateTime.Parse(dateStr);
                    return date.ToString(format);
                })
            };
    }
}
