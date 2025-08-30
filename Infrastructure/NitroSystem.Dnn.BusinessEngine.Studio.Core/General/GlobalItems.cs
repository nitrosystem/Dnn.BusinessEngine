using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.General
{
    public static class GlobalItems
    {
        public static readonly Dictionary<string, string> VariableTypes = new Dictionary<string, string>()
        {
            // Primitive & Nullable
            { "String", "string" },
            { "Int", "int?" },
            { "Long", "long?" },
            { "Float", "float?" },
            { "Double", "double?" },
            { "Decimal", "decimal?" },
            { "Boolean", "bool?" },
            { "Byte", "byte?" },
            { "Short", "short?" },
            { "Char", "char?" },

            // Structs
            { "Guid", "Guid?" },
            { "DateTime", "DateTime?" },
            { "TimeSpan", "TimeSpan?" },

            // Generic Collections
            { "ListString", "List<string>" },
            { "ListInt", "List<int>" },
            { "ListGuid", "List<Guid>" },
            { "ListDateTime", "List<DateTime>" },

            // Dictionaries
            { "DictionaryStringString", "Dictionary<string, string>" },
            { "DictionaryStringInt", "Dictionary<string, int>" },
            { "DictionaryGuidObject", "Dictionary<Guid, object>" },

            // Other
            { "Object", "object" },
            { "Dynamic", "dynamic" }
        };
    }
}
