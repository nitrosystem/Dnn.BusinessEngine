using DotNetNuke.Entities.Users;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Globals
{
    public class Constants
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

        public static readonly Dictionary<string, string> ModulePopularPaths = new Dictionary<string, string>()
        {
            { "[EXTPATH]", "/DesktopModules/BusinessEngine/Extensions" },
            { "[ModulePath]", "/DesktopModules/BusinessEngine" },
            { "[MODULEPATH]", "/DesktopModules/BusinessEngine" },
            { "[BuildPath]", "/DesktopModules/BusinessEngine/Build" }
        };

        public static readonly string[] SqlServerTypes = { "bigint", "binary", "bit", "char", "date", "datetime", "datetime2", "datetimeoffset", "decimal", "filestream", "float", "geography", "geometry", "hierarchyid", "image", "int", "money", "nchar", "ntext", "numeric", "nvarchar", "real", "rowversion", "smalldatetime", "smallint", "smallmoney", "sql_variant", "text", "time", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "varchar", "xml" };

        public static readonly string[] CSharpTypes = { "long", "byte[]", "bool", "char", "DateTime", "DateTime", "DateTime", "DateTimeOffset", "decimal", "byte[]", "double", "Microsoft.SqlServer.Types.SqlGeography", "Microsoft.SqlServer.Types.SqlGeometry", "Microsoft.SqlServer.Types.SqlHierarchyId", "byte[]", "int", "decimal", "string", "string", "decimal", "string", "Single", "byte[]", "DateTime", "short", "decimal", "object", "string", "TimeSpan", "byte[]", "byte", "Guid", "byte[]", "string", "string" };

        public static UserInfo CurrentUser
        {
            get
            {
                return UserController.Instance.GetCurrentUserInfo();
            }
        }
    }
}
