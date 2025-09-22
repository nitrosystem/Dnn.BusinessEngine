using Dapper;
using DotNetNuke.Common.Controls;
using DotNetNuke.Data;
using Microsoft.ApplicationBlocks.Data;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace NitroSystem.Dnn.BusinessEngine.Utilities
{
    public static class DbUtil
    {
        public static void CreateTable(DataTable table)
        {
            string tableName = table.TableName;

            string sqlsc;
            sqlsc = "CREATE TABLE " + tableName + "(";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }

            string query = sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";

            ExecuteScalarSqlTransaction(query);
        }

        public static SqlResultInfo ExecuteSqlTransaction(string query)
        {
            var result = new SqlResultInfo() { IsSuccess = true, Query = query };

            using (SqlConnection connection = new SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("SampleTransaction");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = query;

                    command.ExecuteNonQuery();

                    // Attempt to commit the transaction.
                    transaction.Commit();

                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ResultMessage = ex.Message;

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                    }

                    throw new Exception(ex.Message, ex);
                }
            }

            return result;
        }

        public static SqlResultInfo ExecuteScalarSqlTransaction(string query)
        {
            if (String.IsNullOrWhiteSpace(query)) throw new Exception("Sql Query is incorrect!.");

            var result = new SqlResultInfo() { IsSuccess = true, Query = query };

            using (SqlConnection connection = new SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("SampleTransaction");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = query;

                    result.Result = command.ExecuteScalar();

                    // Attempt to commit the transaction.
                    transaction.Commit();

                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ResultMessage = ex.Message;

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                    }

                    throw new Exception(ex.Message, ex);
                }
            }

            return result;
        }

        public static void ExecuteSql(string query, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = DataProvider.Instance().ConnectionString;

            var connection = new SqlConnection(connectionString);

            SqlMapper.Execute(connection, query);
        }

        public static void ExecuteSp(string spName, IDictionary<string, object> spParams)
        {
            var param = new DynamicParameters();
            foreach (var item in spParams)
            {
                param.Add(item.Key, item.Value);
            }

            string connectionString = DataProvider.Instance().ConnectionString;
            var connection = new SqlConnection(connectionString);

            SqlMapper.Execute(connection, spName, param, commandType: CommandType.StoredProcedure);
        }

        public static T ExecuteScaler<T>(string spName, IDictionary<string, object> spParams)
        {
            var param = new DynamicParameters();
            foreach (var item in spParams)
            {
                param.Add(item.Key, item.Value);
            }

            string connectionString = DataProvider.Instance().ConnectionString;
            var connection = new SqlConnection(connectionString);

            var result = SqlMapper.ExecuteScalar<T>(connection, spName, param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public static object ExecuteScaler(string query)
        {
            return SqlHelper.ExecuteScalar(DataProvider.Instance().ConnectionString, CommandType.Text, query);
        }

        public static bool IsTableExists(string tableName)
        {
            var connection = new SqlConnection(DataProvider.Instance().ConnectionString);

            string query = string.Format("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tableName);

            var result = SqlMapper.ExecuteScalar<bool?>(connection, query, commandType: CommandType.Text);

            return result == null || !result.Value ? false : true;
        }

        public static bool IsTableColumnExists(string tableName, string columnName)
        {
            var connection = new SqlConnection(DataProvider.Instance().ConnectionString);

            string query = string.Format("SELECT 1 FROM sys.columns WHERE Name = N'{0}' AND Object_ID = Object_ID(N'{1}')", columnName, tableName);

            var result = SqlMapper.ExecuteScalar<bool?>(connection, query, commandType: CommandType.Text);

            return result == null || !result.Value ? false : true;
        }

        public static IDataReader ExecuteReader(string query)
        {
            return SqlHelper.ExecuteReader(DataProvider.Instance().ConnectionString, CommandType.Text, query);
        }

        public static bool IsNumericType(string columnType)
        {
            if (string.IsNullOrEmpty(columnType)) return false;

            if (columnType.ToLower().StartsWith("bit")) return true;
            else if (columnType.ToLower().StartsWith("decimal")) return true;
            else if (columnType.ToLower().StartsWith("numeric")) return true;
            else if (columnType.ToLower().StartsWith("float")) return true;
            else if (columnType.ToLower().StartsWith("real")) return true;
            else if (columnType.ToLower().StartsWith("int")) return true;
            else if (columnType.ToLower().StartsWith("bigint")) return true;
            else if (columnType.ToLower().StartsWith("smallint")) return true;
            else if (columnType.ToLower().StartsWith("tinyint")) return true;
            else if (columnType.ToLower().StartsWith("money")) return true;
            else if (columnType.ToLower().StartsWith("smallmoney")) return true;

            return false;
        }

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

        public static IEnumerable<SpParamInfo> GetSpParams(string schema, string spName, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = DataProvider.Instance().ConnectionString;

            var connection = new SqlConnection(connectionString);

            var spParams = new DynamicParameters();
            spParams.Add("@Schema", "dbo");
            spParams.Add("@SpName", spName);

            var result = SqlMapper.Query<SpParamInfo>(connection, "dbo.BusinessEngine_GetStoredProcedureParams", commandType: CommandType.StoredProcedure, param: spParams);

            return result;
        }

        public static void ExecuteTransaction(string query, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = DataProvider.Instance().ConnectionString;

            var connection = new SqlConnection(connectionString);

            using (var transactionScope = new TransactionScope())
            {
                var queries = query.Split(new[] { "\nGO\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string cmd in queries)
                {
                    SqlMapper.Execute(connection, cmd);
                }

                transactionScope.Complete();
            }
        }

        public static IEnumerable<T> ExecuteListTransaction<T>(string query, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = DataProvider.Instance().ConnectionString;

            var connection = new SqlConnection(connectionString);

            IEnumerable<T> result;

            using (var transactionScope = new TransactionScope())
            {
                result = SqlMapper.Query<T>(connection, query);
                transactionScope.Complete();
            }

            return result;
        }

        public static SqlResultInfo RemoveTableColumn(string owner, string tableName, string columnName)
        {
            string query = string.Format(@"
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID(N'[{0}].[{1}]') and [name] = '{2}') 
                        ALTER TABLE [{0}].[{1}] DROP COLUMN [{2}]", owner, tableName, columnName);

            var result = new SqlResultInfo() { IsSuccess = true, Query = query };

            using (SqlConnection connection = new SqlConnection(DataProvider.Instance().ConnectionString))
            {
                try
                {
                    SqlMapper.Execute(connection, query);
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ResultMessage = ex.Message;
                }
            }

            return result;
        }

        public static SqlResultInfo RemoveRelationship(string tableName, string relationshipName)
        {
            string query = @"
                IF (OBJECT_ID('dbo.{0}', 'F') IS NOT NULL)
                BEGIN
                    ALTER TABLE dbo.{1} DROP CONSTRAINT {0}
                END
            ";

            query = string.Format(query, relationshipName, tableName);
            var result = ExecuteScalarSqlTransaction(query);

            return result;
        }

        public static bool AddColumnDefaultValue(string tableName, string columnName, object defaultValue, string prefix = "dbo")
        {
            string queryTemplate = @"ALTER TABLE {0}{1} ADD CONSTRAINT DF_{1}_{2} DEFAULT {3} FOR {2}";

            prefix = string.IsNullOrWhiteSpace(prefix) ? "" : prefix + ".";

            var query = string.Format(queryTemplate, prefix, tableName, columnName, defaultValue);

            var result = ExecuteSqlTransaction(query);

            return result.IsSuccess;
        }

        public static bool RemoveColumnDefaultValue(string tableName, string columnName, string prefix = "dbo")
        {
            string queryTemplate = @"ALTER TABLE {0}{1} DROP CONSTRAINT DF_{1}_{2}";

            prefix = string.IsNullOrWhiteSpace(prefix) ? "" : prefix + ".";

            var query = string.Format(queryTemplate, prefix, tableName, columnName);

            var result = ExecuteSqlTransaction(query);

            return result.IsSuccess;
        }

        public static string GetTableScript(string tableName, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = DataProvider.Instance().ConnectionString;
            
            string Script = "";

            string Sql = "declare @table varchar(100)" + Environment.NewLine +
            "set @table = '" + tableName + "' " + Environment.NewLine +
            //"-- set table name here" +
            "declare @sql table(s varchar(1000), id int identity)" + Environment.NewLine +
            " " + Environment.NewLine +
            //"-- create statement" +
            "insert into  @sql(s) values ('create table [' + @table + '] (')" + Environment.NewLine +
            " " + Environment.NewLine +
            //"-- column list" +
            "insert into @sql(s)" + Environment.NewLine +
            "select " + Environment.NewLine +
            "    '  ['+column_name+'] ' + " + Environment.NewLine +
            "    data_type + coalesce('('+cast(character_maximum_length as varchar)+')','') + ' ' +" + Environment.NewLine +
            "    case when exists ( " + Environment.NewLine +
            "        select id from syscolumns" + Environment.NewLine +
            "        where object_name(id)=@table" + Environment.NewLine +
            "        and name=column_name" + Environment.NewLine +
            "        and columnproperty(id,name,'IsIdentity') = 1 " + Environment.NewLine +
            "    ) then" + Environment.NewLine +
            "        'IDENTITY(' + " + Environment.NewLine +
            "        cast(ident_seed(@table) as varchar) + ',' + " + Environment.NewLine +
            "        cast(ident_incr(@table) as varchar) + ')'" + Environment.NewLine +
            "    else ''" + Environment.NewLine +
            "   end + ' ' +" + Environment.NewLine +
            "    ( case when IS_NULLABLE = 'No' then 'NOT ' else '' end ) + 'NULL ' + " + Environment.NewLine +
            "    coalesce('DEFAULT '+COLUMN_DEFAULT,'') + ','" + Environment.NewLine +
            " " + Environment.NewLine +
            " from information_schema.columns where table_name = @table" + Environment.NewLine +
            " order by ordinal_position" + Environment.NewLine +
            " " + Environment.NewLine +
            //"-- primary key" +
            "declare @pkname varchar(100)" + Environment.NewLine +
            "select @pkname = constraint_name from information_schema.table_constraints" + Environment.NewLine +
            "where table_name = @table and constraint_type='PRIMARY KEY'" + Environment.NewLine +
            " " + Environment.NewLine +
            "if ( @pkname is not null ) begin" + Environment.NewLine +
            "    insert into @sql(s) values('  PRIMARY KEY (')" + Environment.NewLine +
            "    insert into @sql(s)" + Environment.NewLine +
            "        select '   ['+COLUMN_NAME+'],' from information_schema.key_column_usage" + Environment.NewLine +
            "        where constraint_name = @pkname" + Environment.NewLine +
            "        order by ordinal_position" + Environment.NewLine +
            //"    -- remove trailing comma" +
            "    update @sql set s=left(s,len(s)-1) where id=@@identity" + Environment.NewLine +
            "    insert into @sql(s) values ('  )')" + Environment.NewLine +
            "end" + Environment.NewLine +
            "else begin" + Environment.NewLine +
            //"    -- remove trailing comma" +
            "    update @sql set s=left(s,len(s)-1) where id=@@identity" + Environment.NewLine +
            "end" + Environment.NewLine +
            " " + Environment.NewLine +
            "-- closing bracket" + Environment.NewLine +
            "insert into @sql(s) values( ')' )" + Environment.NewLine +
            " " + Environment.NewLine +
            //"-- result!" +
            "select s from @sql order by id";

            //DataTable dt = GetTableData(Sql, ConnectionString);
            //foreach (DataRow row in dt.Rows)
            //{
            //    Script += row[0].ToString() + Environment.NewLine;
            //}

            return Script;
        }

        public static string GetSpScript(string spName)
        {
            var connection = new SqlConnection(DataProvider.Instance().ConnectionString);

            var result = SqlMapper.QuerySingle<string>(connection, string.Format("SELECT [Definition] FROM sys.sql_modules WHERE objectproperty(OBJECT_ID, 'IsProcedure') = 1 AND OBJECT_NAME(OBJECT_ID) = '{0}'", spName));

            return result;
        }

        public static string NormalizeProcedureName(string input)
        {
            // حذف براکت‌ها و فاصله‌های اضافی
            var cleaned = input.Replace("[", "").Replace("]", "").Trim();

            // اسپلیت بر اساس نقطه
            var parts = cleaned.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string schema;
            string proc;

            if (parts.Length == 2)
            {
                schema = parts[0];
                proc = parts[1];
            }
            else if (parts.Length == 1)
            {
                schema = "dbo"; // پیش‌فرض
                proc = parts[0];
            }
            else
            {
                // ورودی خیلی بدفرمت بود
                schema = "dbo";
                proc = "Unknown";
            }

            return $"{schema}.{proc}";
        }
    }
}
