using System.Data.SqlClient;
using System.Collections.Generic;
using Dapper;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    public static class DbUtil
    {
        public static IEnumerable<string> GetDatabaseObjects(int type, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = DataProvider.Instance().ConnectionString;

            var connection = new SqlConnection(connectionString);

            if (type == 0)
                return SqlMapper.Query<string>(connection, "SELECT name FROM sys.objects Where type = N'U' order by name");
            else if (type == 1)
                return SqlMapper.Query<string>(connection, "SELECT name FROM sys.objects Where type = N'V' order by name");
            else
                return null;
        }

        public static IEnumerable<TableColumnInfo> GetDatabaseObjectColumns(string objectName, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = DataProvider.Instance().ConnectionString;

            var connection = new SqlConnection(connectionString);

            string query = @"With PrimaryColumn as
                            (
	                            SELECT Distinct
		                            c.name as ColumnName,
		                            c.column_id as Number,
		                            t.Name as ColumnType,
		                            c.max_length as MaxLength,
		                            c.is_identity as IsIdentity,
		                            c.is_nullable as AllowNulls,
		                            c.is_computed as IsComputed,
		                            ISNULL(i.is_primary_key, 0) as IsPrimary
		                            FROM
			                            sys.columns c
		                            INNER JOIN
			                            sys.types t ON c.user_type_id = t.user_type_id
		                            LEFT OUTER JOIN 
			                            sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
		                            INNER JOIN
			                            sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
		                            WHERE
			                            c.object_id = OBJECT_ID('{0}') and i.is_primary_key = 1
                            )
                            Select * From PrimaryColumn
                            union
                            SELECT Distinct
	                            c.name,
	                            c.column_id,
	                            t.Name,
	                            c.max_length,
	                            c.is_identity,
	                            c.is_nullable,
	                            c.is_computed,
	                            0
	                            FROM
		                            sys.columns c
	                            INNER JOIN
		                            sys.types t ON c.user_type_id = t.user_type_id
	                            LEFT OUTER JOIN 
		                            sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
	                            WHERE
		                            c.object_id = OBJECT_ID('{0}') and c.name not in (Select ColumnName From PrimaryColumn) order by number";

            return SqlMapper.Query<TableColumnInfo>(connection, string.Format(query, objectName));
        }

        public static string GetSpScript(string spName)
        {
            var connection = new SqlConnection(DataProvider.Instance().ConnectionString);

            var result = SqlMapper.QuerySingle<string>(connection, string.Format("SELECT [Definition] FROM sys.sql_modules WHERE objectproperty(OBJECT_ID, 'IsProcedure') = 1 AND OBJECT_NAME(OBJECT_ID) = '{0}'", spName));

            return result;
        }
    }
}
