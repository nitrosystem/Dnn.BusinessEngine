using DotNetNuke.Data;
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
    public class SqlQueryExecutor : ISqlQueryExecutor
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, List<object>> _cache = new(); // Simple in-memory cache

        public SqlQueryExecutor()
        {
            _connectionString = DataProvider.Instance().ConnectionString;
        }

        public SqlQueryExecutor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<T> ExecuteQuery<T>(string queryOrSp, CommandType commandType, Dictionary<string, object> parameters = null) where T : new()
        {
            string cacheKey = GenerateCacheKey<T>(queryOrSp, parameters);
            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey].Cast<T>().ToList();
            }

            var result = new List<T>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(queryOrSp, connection)
            {
                CommandType = commandType
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            connection.Open();
            using var reader = command.ExecuteReader();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            while (reader.Read())
            {
                var item = new T();

                foreach (var prop in props)
                {
                    if (!reader.HasColumn(prop.Name) || reader[prop.Name] is DBNull)
                        continue;

                    var value = reader[prop.Name];
                    prop.SetValue(item, Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
                }

                result.Add(item);
            }

            _cache[cacheKey] = result.Cast<object>().ToList(); // Cache it
            return result;
        }

        private string GenerateCacheKey<T>(string queryOrSp, Dictionary<string, object> parameters)
        {
            string paramStr = parameters != null ? string.Join(",", parameters.Select(p => $"{p.Key}={p.Value}")) : "";
            return $"{typeof(T).FullName}_{queryOrSp}_{paramStr}";
        }
    }

}
