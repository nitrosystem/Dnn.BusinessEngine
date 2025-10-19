using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repository
{
    public class SqlDatabaseMetadataRepository : IDatabaseMetadataRepository
    {
        private readonly IDbConnection _connection;
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _objectsCache = new ConcurrentDictionary<string, IEnumerable<string>>();
        private readonly ConcurrentDictionary<string, IEnumerable<TableColumnInfo>> _columnsCache = new ConcurrentDictionary<string, IEnumerable<TableColumnInfo>>();
        private readonly ConcurrentDictionary<string, string> _spScriptCache = new ConcurrentDictionary<string, string>();

        public SqlDatabaseMetadataRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<string>> GetDatabaseObjectsAsync(int type)
        {
            string cacheKey = $"DBObjects_{type}";
            if (_objectsCache.TryGetValue(cacheKey, out var cached))
                return cached;

            string query = null;
            if (type == 0)
                query = "SELECT name FROM sys.objects WHERE type = N'U' ORDER BY name"; // Tables
            else if (type == 1)
                query = "SELECT name FROM sys.objects WHERE type = N'V' ORDER BY name"; // Views

            if (string.IsNullOrEmpty(query))
                return new string[0];

            var result = await _connection.QueryAsync<string>(query);
            _objectsCache[cacheKey] = result;
            return result;
        }

        public async Task<IEnumerable<TableColumnInfo>> GetDatabaseObjectColumnsAsync(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                throw new ArgumentNullException(nameof(objectName));

            if (_columnsCache.TryGetValue(objectName, out var cached))
                return cached;

            string query = @"
                ;WITH PrimaryColumn AS (
                    SELECT DISTINCT 
                        c.name AS ColumnName,
                        c.column_id AS Number,
                        t.Name AS ColumnTypeWithoutSize,
                        c.max_length AS MaxLength,
                        c.is_identity AS IsIdentity,
                        c.is_nullable AS AllowNulls,
                        c.is_computed AS IsComputed,
                        ISNULL(i.is_primary_key, 0) AS IsPrimary,
                        CASE 
                            WHEN t.name IN ('nvarchar', 'varchar', 'nchar', 'char') THEN 
                                t.name + '(' + 
                                CASE 
                                    WHEN c.max_length = -1 THEN 'max' 
                                    ELSE CAST(c.max_length / CASE WHEN t.name LIKE 'n%' THEN 2 ELSE 1 END AS VARCHAR)
                                END + ')'
                            WHEN t.name IN ('decimal', 'numeric') THEN 
                                t.name + '(' + CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR) + ')'
                            ELSE t.name
                        END AS ColumnType
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                    WHERE c.object_id = OBJECT_ID(@ObjectName) AND i.is_primary_key = 1
                )
                SELECT * FROM PrimaryColumn
                UNION
                SELECT DISTINCT 
                    c.name,
                    c.column_id,
                    t.Name,
                    c.max_length,
                    c.is_identity,
                    c.is_nullable,
                    c.is_computed,
                    0,
                    CASE 
                        WHEN t.name IN ('nvarchar', 'varchar', 'nchar', 'char') THEN 
                            t.name + '(' + 
                            CASE 
                                WHEN c.max_length = -1 THEN 'max' 
                                ELSE CAST(c.max_length / CASE WHEN t.name LIKE 'n%' THEN 2 ELSE 1 END AS VARCHAR)
                            END + ')'
                        WHEN t.name IN ('decimal', 'numeric') THEN 
                            t.name + '(' + CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR) + ')'
                        ELSE t.name
                    END AS ColumnType
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE c.object_id = OBJECT_ID(@ObjectName)
                  AND c.name NOT IN (SELECT ColumnName FROM PrimaryColumn)
                ORDER BY Number
                    ";

            var result = await _connection.QueryAsync<TableColumnInfo>(query, new { ObjectName = objectName });
            _columnsCache[objectName] = result;
            return result;
        }

        public async Task<string> GetStoredProcedureScriptAsync(string spName)
        {
            if (string.IsNullOrWhiteSpace(spName))
                throw new ArgumentNullException(nameof(spName));

            if (_spScriptCache.TryGetValue(spName, out var cached))
                return cached;

            const string query = @"
                    SELECT [Definition] 
                    FROM sys.sql_modules 
                    WHERE OBJECTPROPERTY(OBJECT_ID, 'IsProcedure') = 1
                          AND OBJECT_NAME(OBJECT_ID) = @SpName";

            var result = await _connection.QuerySingleOrDefaultAsync<string>(query, new { SpName = spName });
            _spScriptCache[spName] = result;
            return result;
        }

        public async Task<string> GetSpScript(string spName)
        {
            var result = await _connection.QuerySingleAsync<string>(string.Format("SELECT [Definition] FROM sys.sql_modules WHERE objectproperty(OBJECT_ID, 'IsProcedure') = 1 AND OBJECT_NAME(OBJECT_ID) = '{0}'", spName));
            return result;
        }
    }
}
