using System;
using System.IO;
using System.Collections.Generic;
using DotNetNuke.Entities.Users;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Globals
{
    public static class Constants
    {
        public static readonly Dictionary<string, string> VariableTypes = new Dictionary<string, string>()
        {
            // App Model 
            {"AppModel","AppModel" },
            {"AppModelList","AppModelList" },

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

            { "Object", "object" },
        };

        public static readonly Dictionary<string, string> ModulePopularPaths = new Dictionary<string, string>()
        {
            { "[EXTENSION_PATH]", "/DesktopModules/BusinessEngine/Extensions" },
            { "[MODULE_PATH]", "/DesktopModules/BusinessEngine" }
        };

        public static readonly string[] SqlServerTypes = { "bigint", "binary", "bit", "char", "date", "datetime", "datetime2", "datetimeoffset", "decimal", "filestream", "float", "geography", "geometry", "hierarchyid", "image", "int", "money", "nchar", "ntext", "numeric", "nvarchar", "real", "rowversion", "smalldatetime", "smallint", "smallmoney", "sql_variant", "text", "time", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "varchar", "xml" };

        public static readonly string[] CSharpTypes = { "long", "byte[]", "bool", "char", "DateTime", "DateTime", "DateTime", "DateTimeOffset", "decimal", "byte[]", "double", "Microsoft.SqlServer.Types.SqlGeography", "Microsoft.SqlServer.Types.SqlGeometry", "Microsoft.SqlServer.Types.SqlHierarchyId", "byte[]", "int", "decimal", "string", "string", "decimal", "string", "Single", "byte[]", "DateTime", "short", "decimal", "object", "string", "TimeSpan", "byte[]", "byte", "Guid", "byte[]", "string", "string" };

        /// <summary>
        /// Get DNN Current UserId => -1: if user is not login
        /// </summary>
        public static int CurrentUserId
        {
            get
            {
                return UserController.Instance.GetCurrentUserInfo().UserID;
            }
        }
    }
}
