using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.ADO_NET
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
