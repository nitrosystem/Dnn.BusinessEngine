using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class ParamHelper
    {
        private const char ParamPrefix = '@';

        public static Dictionary<string, object> GetLookup(IReadOnlyList<ParamInfo> paramList)
        {
            return paramList?.ToDictionary(
                p => p.ParamName,
                p => p.ParamValue) ?? new Dictionary<string, object>();
        }

        public static object FillSqlParamsAsObject(IReadOnlyList<ParamInfo> paramList)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var param in paramList ?? Array.Empty<ParamInfo>())
            {
                expando[param.ParamName] = param.ParamValue;
            }

            return expando;
        }

        public static string ParseParams(string input, IEnumerable<ParamInfo> actionParams)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return Regex.Replace(input, @"(@\w+)", match =>
            {
                var paramName = match.Groups[1].Value;
                var paramValue = actionParams.FirstOrDefault(a => a.ParamName == paramName).ParamValue;
                return paramValue?.ToString();
            });
        }

        // ----------------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------------
        /// <summary>
        /// Resolves an "@paramName" placeholder against the filled params,
        /// or returns the literal value if it is not a placeholder.
        /// </summary>
        public static string ResolveValue(object rawValue, IReadOnlyDictionary<string, object> paramLookup)
        {
            if (rawValue is null) return null;

            var stringValue = rawValue.ToString();
            if (string.IsNullOrEmpty(stringValue)) return stringValue;
            if (stringValue[0] != ParamPrefix) return stringValue;

            return paramLookup.TryGetValue(stringValue, out var resolved)
                    ? resolved?.ToString()
                    : null;
        }
    }
}
