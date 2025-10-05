using System;
using System.Data;
using System.Data.SqlClient;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Extensions
{
    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            var schemaTable = reader.GetSchemaTable();

            if (schemaTable == null) return false;

            foreach (DataRow row in schemaTable.Rows)
            {
                if (row["ColumnName"] is string col && string.Equals(col, columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
