using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Dapper;
using static Dapper.SqlMapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.ORM.Dapper.Sql
{
    public class Sql : ISql
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private static readonly ConcurrentDictionary<Type, HashSet<string>> _types = new();

        public Sql(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        #region Get Single Entity

        public async Task<T> GetAsync<T>(Guid id) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_{id}"
                : string.Empty;

            var query = $"SELECT * FROM {table} WHERE Id = @Value";

            return await _cacheService.GetOrCreateAsync<T>(cacheKey, () =>
              _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(
                query,
                new { Value = id },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<T> GetByColumnAsync<T>(string column, object value) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_{column}_{value}"
                : string.Empty;

            var query = $"SELECT * FROM {table} WHERE {column} = @Value";

            return await _cacheService.GetOrCreateAsync<T>(cacheKey, () =>
             _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(
                query,
                new { Value = value },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut) ?? default(T);
        }

        #endregion

        #region Get Column Value(s)

        public async Task<IReadOnlyList<TColumnType>> GetColumnValuesAsync<T, TColumnType>(string column) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_{column}"
                : string.Empty;

            var query = $"SELECT {column} FROM {table}";

            return await _cacheService.GetOrCreateReadOnlyListAsync<TColumnType>(cacheKey, () =>
             _unitOfWork.Connection.QueryAsync<TColumnType>(
                query,
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<IReadOnlyList<TColumnType>> GetColumnsValueAsync<T, TColumnType>(string column, string filterColumn, object filterValue) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));
            if (!props.Contains(filterColumn))
                throw new ArgumentException($"Invalid column name {filterColumn}.", nameof(filterColumn));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_{column}_{filterColumn}_{filterValue}"
                : string.Empty;

            var query = $"SELECT {column} FROM {table} WHERE {filterColumn} = @Value";

            return await _cacheService.GetOrCreateReadOnlyListAsync<TColumnType>(cacheKey, () =>
             _unitOfWork.Connection.QueryAsync<TColumnType>(
                query,
                new { Value = filterValue },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<TColumnType> GetColumnValueAsync<T, TColumnType>(Guid id, string column) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_{column}_{id}"
                : string.Empty;

            var query = $"SELECT TOP 1 {column} FROM {table} WHERE Id = @Value";

            return await _cacheService.GetOrCreateAsync<TColumnType>(cacheKey, () =>
             _unitOfWork.Connection.ExecuteScalarAsync<TColumnType>(
                query,
                new { Value = id },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<TColumnType> GetColumnValueAsync<T, TColumnType>(string column, string filterColumn, object filterValue) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));
            if (!props.Contains(filterColumn))
                throw new ArgumentException($"Invalid column name {filterColumn}.", nameof(filterColumn));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_{column}_{filterColumn}_{filterValue}"
                : string.Empty;

            var query = $"SELECT TOP 1 {column} FROM {table} WHERE {filterColumn} = @Value";

            return await _cacheService.GetOrCreateAsync<TColumnType>(cacheKey, () =>
             _unitOfWork.Connection.ExecuteScalarAsync<TColumnType>(
                query,
                new { Value = filterValue },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public TColumnType GetColumnValue<T, TColumnType>(string column, string filterColumn, object filterValue) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));
            if (!props.Contains(filterColumn))
                throw new ArgumentException($"Invalid column name {filterColumn}.", nameof(filterColumn));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_{column}_{filterColumn}_{filterValue}"
                : string.Empty;

            var query = $"SELECT TOP 1 {column} FROM {table} WHERE {filterColumn} = @Value";

            return _cacheService.GetOrCreate<TColumnType>(cacheKey, () =>
             _unitOfWork.Connection.ExecuteScalar<TColumnType>(
                query,
                new { Value = filterValue },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<bool> ExistsAsync<T>() where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + "__Exists"
                : string.Empty;

            var query = $"SELECT TOP 1 1 FROM {table}";

            return await _cacheService.GetOrCreateAsync<bool>(cacheKey, () =>
             _unitOfWork.Connection.ExecuteScalarAsync<bool>(
                query,
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        #endregion

        #region Get Multiple Entities

        public async Task<IReadOnlyList<T>> GetAllAsync<T>(params string[] orderColumns) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var column = orderColumn.Split(' ')[0];
                if (!props.Contains(column))
                    throw new ArgumentException($"Invalid column name {column}.", nameof(column));

                sorts.Add(orderColumn);
            }

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? CacheKeyBuilder.BuildCacheKey(cacheAttr.key, null, orderColumns)
                : string.Empty;

            var query = $"SELECT * FROM {table}" +
                (sorts.Any()
                    ? $" ORDER BY {string.Join(",", sorts)}"
                    : string.Empty
                );

            return await _cacheService.GetOrCreateReadOnlyListAsync<T>(cacheKey, () =>
             _unitOfWork.Connection.QueryAsync<T>(
                query,
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<IReadOnlyList<T>> GetByScopeAsync<T>(object value, params string[] orderColumns) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var column = orderColumn.Split(' ')[0];
                if (!props.Contains(column))
                    throw new ArgumentException($"Invalid column name {column}.", nameof(column));

                sorts.Add(orderColumn);
            }

            var table = AttributeCache.Instance.GetTableName<T>();
            var scopeColumn = AttributeCache.Instance.GetScope<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? CacheKeyBuilder.BuildCacheKey($"{cacheAttr.key}_{value}_", null, orderColumns)
                : string.Empty;

            var query = $"SELECT * FROM {table} WHERE {scopeColumn} = @Value" +
                (sorts.Any()
                    ? $" ORDER BY {string.Join(",", sorts)}"
                    : string.Empty
                );

            return await _cacheService.GetOrCreateReadOnlyListAsync<T>(cacheKey, () =>
             _unitOfWork.Connection.QueryAsync<T>(
                query,
                new { Value = value },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<IReadOnlyList<T>> GetItemsByColumnAsync<T>(string column, object value, params string[] orderColumns) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));

            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var col = orderColumn.Split(' ')[0];
                if (!props.Contains(col))
                    throw new ArgumentException($"Invalid column name {col}.", nameof(col));

                sorts.Add(orderColumn);
            }

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? CacheKeyBuilder.BuildCacheKey($"{cacheAttr.key}_{column}_{value}_", null, orderColumns)
                : string.Empty;

            var query = $"SELECT * FROM {table} WHERE {column} = @Value" +
                (sorts.Any()
                    ? $" ORDER BY {string.Join(",", sorts)}"
                    : string.Empty
                );

            return await _cacheService.GetOrCreateReadOnlyListAsync<T>(cacheKey, () =>
            _unitOfWork.Connection.QueryAsync<T>(
                query,
                new { Value = value },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<IReadOnlyList<T>> GetItemsByColumnsAsync<T>(string[] columns, object values, params string[] orderColumns)
            where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            foreach (var column in columns)
            {
                if (!props.Contains(column))
                    throw new ArgumentException($"Invalid column name {column}.", nameof(column));
            }

            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var col = orderColumn.Split(' ')[0];
                if (!props.Contains(col))
                    throw new ArgumentException($"Invalid column name {col}.", nameof(col));

                sorts.Add(orderColumn);
            }

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
               ? CacheKeyBuilder.BuildCacheKey(cacheAttr.key, values, columns)
               : string.Empty;

            var condition = string.Join(" and ", columns.Select(column =>
            {
                var prop = values.GetType().GetProperty(column);
                var value = prop?.GetValue(values, null);
                return value == null ? $"{column} IS NULL" : $"{column} = @{column}";
            }));

            var query = $"SELECT * FROM {table} WHERE {condition}" +
                (sorts.Any()
                    ? $" ORDER BY {string.Join(",", sorts)}"
                    : string.Empty
                );

            return await _cacheService.GetOrCreateReadOnlyListAsync<T>(cacheKey, () =>
                _unitOfWork.Connection.QueryAsync<T>(
                    query, values,
                    _unitOfWork.Transaction
                ), cacheAttr.timeOut);
        }

        public async Task<IReadOnlyList<TChild>> GetChildsByParentColumnAsync<TParent, TChild>(string parentColumn, string childColumn, object value, params string[] orderColumns)
            where TParent : class, IEntity, new()
            where TChild : class, IEntity, new()
        {
            Type parentType = typeof(TParent);
            var parentProps = _types.GetOrAdd(parentType, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!parentProps.Contains(parentColumn))
                throw new ArgumentException($"Invalid column name {parentColumn}.", nameof(parentColumn));

            Type childType = typeof(TChild);
            var childProps = _types.GetOrAdd(childType, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!childProps.Contains(childColumn))
                throw new ArgumentException($"Invalid column name {childColumn}.", nameof(childColumn));

            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var col = orderColumn.Split(' ')[0];
                if (!childProps.Contains(col))
                    throw new ArgumentException($"Invalid column name {col}.", nameof(col));

                sorts.Add(orderColumn);
            }

            var childTable = AttributeCache.Instance.GetTableName<TChild>();
            var parentTable = AttributeCache.Instance.GetTableName<TParent>();
            var cacheAttr = AttributeCache.Instance.GetCache<TChild>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
               ? CacheKeyBuilder.BuildCacheKey(cacheAttr.key + $"_{parentColumn}_{childColumn}_${value}")
               : string.Empty;

            var query = $"SELECT * FROM {childTable} WHERE {childColumn} in (SELECT Id FROM {parentTable} WHERE {parentColumn} = @Value)" +
                (sorts.Any()
                    ? $" ORDER BY {string.Join(",", sorts)}"
                    : string.Empty
                );

            return await _cacheService.GetOrCreateReadOnlyListAsync<TChild>(cacheKey, () =>
                _unitOfWork.Connection.QueryAsync<TChild>(
                    query,
                    new { Value = value },
                    _unitOfWork.Transaction
                ), cacheAttr.timeOut);
        }

        #endregion

        #region Insert

        public async Task<Guid> InsertAsync<T>(T entity) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite).ToArray();

            var createdOnDateProp = properties.FirstOrDefault(p => p.Name == "CreatedOnDate");
            if (createdOnDateProp != null)
                createdOnDateProp.SetValue(entity, DateTime.Now);

            var createdByUserProp = properties.FirstOrDefault(p => p.Name == "CreatedByUserId");
            if (createdByUserProp != null)
                createdByUserProp.SetValue(entity, Constants.CurrentUserId);

            var lastModifiedDateProp = properties.FirstOrDefault(p => p.Name == "LastModifiedOnDate");
            if (lastModifiedDateProp != null)
                lastModifiedDateProp.SetValue(entity, DateTime.Now);

            var lastModifiedByUserProp = properties.FirstOrDefault(p => p.Name == "LastModifiedByUserId");
            if (lastModifiedByUserProp != null)
                lastModifiedByUserProp.SetValue(entity, Constants.CurrentUserId);

            var columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
            var values = string.Join(", ", properties.Select(p => "@" + p.Name));

            var sql = $"INSERT INTO {table} ({columns}) VALUES ({values});";

            try
            {
                await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(
                    sql,
                    entity,
                    _unitOfWork.Transaction);

                _cacheService.RemoveByPrefix(cacheAttr.key);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting into {table}: {ex.Message}", ex);
            }

            return entity.Id;
        }

        public async Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, IEntity, new()
        {
            if (entities == null || !entities.Any()) return 0;

            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite).ToArray();

            var entityList = entities.ToList();
            foreach (var entity in entityList)
            {
                if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

                var lastModifiedDateProp = properties.FirstOrDefault(p => p.Name == "LastModifiedOnDate");
                if (lastModifiedDateProp != null)
                    lastModifiedDateProp.SetValue(entity, DateTime.Now);

                var lastModifiedByUserProp = properties.FirstOrDefault(p => p.Name == "LastModifiedByUserId");
                if (lastModifiedByUserProp != null)
                    lastModifiedByUserProp.SetValue(entity, Constants.CurrentUserId);

                var createdOnDateProp = properties.FirstOrDefault(p => p.Name == "CreatedOnDate");
                if (createdOnDateProp != null)
                    createdOnDateProp.SetValue(entity, DateTime.Now);

                var createdByUserProp = properties.FirstOrDefault(p => p.Name == "CreatedByUserId");
                if (createdByUserProp != null)
                    createdByUserProp.SetValue(entity, Constants.CurrentUserId);
            }

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            // Generate column names and parameter names for the SQL query
            var columnNames = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
            var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var query = $"INSERT INTO [{table}] ({columnNames}) VALUES ({parameterNames})";
            var result = await _unitOfWork.Connection.ExecuteAsync(query, entityList, _unitOfWork.Transaction);

            _cacheService.ClearByPrefix(cacheAttr.key);

            return result;
        }

        #endregion

        #region Update

        public async Task<bool> UpdateAsync<T>(T entity, params string[] updatedColumns) where T : class, IEntity, new()
        {
            if (entity.Id == Guid.Empty)
                throw new ArgumentException("Entity must have a valid Id.");

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var properties =
                typeof(T)
                    .GetProperties()
                    .Where(p => p.CanRead && p.CanWrite && p.Name != "Id" && p.Name != "CreatedOnDate" && p.Name != "CreatedByUserId" &&
                            (updatedColumns == null || updatedColumns.Length == 0) || updatedColumns.Contains(p.Name))
                    .ToArray();

            var lastModifiedOnDateProp = properties.FirstOrDefault(p => p.Name == "LastModifiedOnDate");
            if (lastModifiedOnDateProp != null)
                lastModifiedOnDateProp.SetValue(entity, DateTime.Now);

            var lastModifiedByUserProp = properties.FirstOrDefault(p => p.Name == "LastModifiedByUserId");
            if (lastModifiedByUserProp != null)
                lastModifiedByUserProp.SetValue(entity, Constants.CurrentUserId);

            if (!properties.Any())
                throw new InvalidOperationException("No valid properties to update.");

            var setClause = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));
            var sql = $"UPDATE {table} SET {setClause} WHERE Id = @Id;";

            try
            {
                var result = await _unitOfWork.Connection.ExecuteAsync(sql, entity, _unitOfWork.Transaction);
                _cacheService.RemoveByPrefix(cacheAttr.key);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating {table}: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateColumnAsync<T>(string column, object value, Guid id) where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var query = $"UPDATE dbo.{table} SET {column} = @NewValue WHERE Id = @Value";

            int rowAffected = await _unitOfWork.Connection.ExecuteAsync(
                query,
                new
                {
                    NewValue = value,
                    Value = id
                }, _unitOfWork.Transaction);

            _cacheService.RemoveByPrefix(cacheAttr.key);

            return rowAffected > 0;
        }

        public async Task<bool> UpdateColumnAsync<T>(string column, string value, string filterColumn, object filterValue)
                                                                                                where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var props = _types.GetOrAdd(type, t => t.GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal));
            if (!props.Contains(column))
                throw new ArgumentException($"Invalid column name {column}.", nameof(column));

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var query = $"UPDATE dbo.{table} SET {column} = @NewValue WHERE {filterColumn} = @FilterValue";

            int rowAffected = await _unitOfWork.Connection.ExecuteAsync(
                query,
                new
                {
                    NewValue = value,
                    FilterValue = filterValue
                }, _unitOfWork.Transaction);

            _cacheService.RemoveByPrefix(cacheAttr.key);

            return rowAffected > 0;
        }


        #endregion

        #region Delete

        public async Task<bool> DeleteAsync<T>(Guid id) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var query = $"DELETE FROM {table} WHERE id = @Value";
            int rowAffected = await _unitOfWork.Connection.ExecuteAsync(
                query,
                new { Value = id },
                _unitOfWork.Transaction);

            _cacheService.RemoveByPrefix(cacheAttr.key);

            return rowAffected >= 1;
        }

        public async Task<bool> DeleteByScopeAsync<T>(object value) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var scopeColumn = AttributeCache.Instance.GetScope<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var query = $"DELETE FROM {table} WHERE {scopeColumn} = @Value";
            int rowAffected = await _unitOfWork.Connection.ExecuteAsync(query, new { Value = value }, _unitOfWork.Transaction);

            _cacheService.RemoveByPrefix(cacheAttr.key);

            return rowAffected >= 1;
        }

        #endregion

        #region Stored Procedures - Execution

        public async Task ExecuteStoredProcedureAsync(string storedProcedure, object parameters)
        {
            await _unitOfWork.Connection.ExecuteAsync(
               storedProcedure,
               param: parameters,
               commandType: CommandType.StoredProcedure,
               transaction: _unitOfWork.Transaction
           );
        }

        public async Task<T> ExecuteStoredProcedureScalerAsync<T>(string storedProcedure, string cacheKey, object parameters)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            return await _cacheService.GetOrCreateAsync<T>(cacheKey, () =>
                _unitOfWork.Connection.ExecuteScalarAsync<T>(
                    storedProcedure,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: _unitOfWork.Transaction
                ), 20);
        }

        #endregion

        #region Stored Procedures - Single Result

        public async Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedure, string cacheKey, object parameters)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            return await _cacheService.GetOrCreateAsync<T>(cacheKey, () =>
                _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(
                    storedProcedure,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: _unitOfWork.Transaction
                ), 20);
        }

        public T ExecuteStoredProcedure<T>(string storedProcedure, string cacheKey, object parameters)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            return _cacheService.GetOrCreate<T>(cacheKey, () =>
                _unitOfWork.Connection.QuerySingleOrDefault<T>(
                    storedProcedure,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: _unitOfWork.Transaction
                ), 20);
        }

        public async Task<object> ExecuteStoredProcedureAsync(Type type, string storedProcedure, string cacheKey, object parameters = null)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            return await _cacheService.GetOrCreateAsync<object>(cacheKey, () =>
                _unitOfWork.Connection.QuerySingleOrDefaultAsync(
                    type,
                    storedProcedure,
                    param: parameters,
                    transaction: _unitOfWork.Transaction,
                    commandType: CommandType.StoredProcedure
                ), 20);
        }

        #endregion

        #region Stored Procedures - List Result

        public async Task<IReadOnlyList<T>> ExecuteStoredProcedureAsListAsync<T>(string storedProcedure, string cacheKey, object parameters)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            return await _cacheService.GetOrCreateReadOnlyListAsync<T>(cacheKey, () =>
                _unitOfWork.Connection.QueryAsync<T>(
                    storedProcedure,
                    parameters,
                    _unitOfWork.Transaction,
                    commandType: CommandType.StoredProcedure
                ), 20);
        }

        public IReadOnlyList<T> ExecuteStoredProcedureAsList<T>(string storedProcedure, string cacheKey, object parameters)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            return _cacheService.GetOrCreateReadOnlyList<T>(cacheKey, () =>
                _unitOfWork.Connection.Query<T>(
                    storedProcedure,
                    parameters,
                    _unitOfWork.Transaction,
                    commandType: CommandType.StoredProcedure
                ), 20);
        }

        public async Task<IReadOnlyList<object>> ExecuteStoredProcedureAsListAsync(Type type, string storedProcedure, string cacheKey, object parameters = null)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            return await _cacheService.GetOrCreateReadOnlyListAsync<object>(cacheKey, () =>
                _unitOfWork.Connection.QueryAsync(
                    type,
                    storedProcedure,
                    param: parameters,
                    transaction: _unitOfWork.Transaction,
                    commandType: CommandType.StoredProcedure
                ), 20);
        }

        #endregion

        #region Stored Procedures - Paging

        public async Task<(IReadOnlyList<object> Items, int TotalCount)> ExecuteStoredProcedureForPagingAsync(
            Type type,
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            // --- بررسی کش ---
            var cached = _cacheService.Get<(IReadOnlyList<object>, int)?>(cacheKey);
            if (cached != null)
                return cached.Value;

            (IReadOnlyList<object> Items, int TotalCount) result;

            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                var items = (await grid.ReadAsync(type)).ToList();
                var totalCount = await grid.ReadSingleAsync<int>();

                result = (items, totalCount);
            }

            // --- ذخیره کش ---
            _cacheService.Set(cacheKey, result);

            return result;
        }

        #endregion

        #region Stored Procedures - Multiple Result Sets

        public async Task<(IReadOnlyList<T1>, IReadOnlyList<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            // Check cache first
            var cachedResult = !string.IsNullOrEmpty(cacheKey)
                ? _cacheService.Get<(IReadOnlyList<T1>, IReadOnlyList<T2>)?>(cacheKey)
                : null;
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IReadOnlyList<T1>, IReadOnlyList<T2>) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                result.Item1 = (await grid.ReadAsync<T1>()).ToList();
                result.Item2 = (await grid.ReadAsync<T2>()).ToList();
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }

        public async Task<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            // Check cache first
            var cachedResult = !string.IsNullOrEmpty(cacheKey)
                ? _cacheService.Get<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>)?>(cacheKey)
                : null;
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                result.Item1 = (await grid.ReadAsync<T1>()).ToList();
                result.Item2 = (await grid.ReadAsync<T2>()).ToList();
                result.Item3 = (await grid.ReadAsync<T3>()).ToList();
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }

        public async Task<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3, T4>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            // Check cache first
            var cachedResult = !string.IsNullOrEmpty(cacheKey)
                ? _cacheService.Get<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>)?>(cacheKey)
                : null;
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                result.Item1 = (await grid.ReadAsync<T1>()).ToList();
                result.Item2 = (await grid.ReadAsync<T2>()).ToList();
                result.Item3 = (await grid.ReadAsync<T3>()).ToList();
                result.Item4 = (await grid.ReadAsync<T4>()).ToList();
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }

        public async Task<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>, IReadOnlyList<T5>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3, T4, T5>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            if (!string.IsNullOrEmpty(cacheKey)) cacheKey = CacheKeyBuilder.BuildCacheKey(cacheKey, parameters);

            // Check cache first
            var cachedResult = !string.IsNullOrEmpty(cacheKey)
                ? _cacheService.Get<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>, IReadOnlyList<T5>)?>(cacheKey)
                : null;
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>, IReadOnlyList<T5>) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                result.Item1 = (await grid.ReadAsync<T1>()).ToList();
                result.Item2 = (await grid.ReadAsync<T2>()).ToList();
                result.Item3 = (await grid.ReadAsync<T3>()).ToList();
                result.Item4 = (await grid.ReadAsync<T4>()).ToList();
                result.Item5 = (await grid.ReadAsync<T5>()).ToList();
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }

        #endregion

        #region Execute Sql Command

        public async Task ExecuteSqlCommandTextAsync(IUnitOfWork unitOfWork, string commandText, object param = null)
        {
            try
            {
                var batches = Regex.Split(commandText, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    var trimmed = batch.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        await unitOfWork.Connection.ExecuteAsync(trimmed, param, unitOfWork.Transaction);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error execute query {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<T>> ExecuteSqlCommandTextAsync<T>(IUnitOfWork unitOfWork, string commandText, object param = null)
        {
            try
            {
                return await unitOfWork.Connection.QueryAsync<T>(commandText, param, _unitOfWork.Transaction);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error execute query {ex.Message}", ex);
            }
        }

        public IDataReader ExecuteSqlReader(IUnitOfWork unitOfWork, CommandType commandType, string commandText, object param = null)
        {
            try
            {
                return unitOfWork.Connection.ExecuteReader(
                    sql: commandText,
                    param: param,
                    commandType: commandType,
                    transaction: unitOfWork.Transaction
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing query: {ex.Message}", ex);
            }
        }


        #endregion

        #region Sql Database Metadata

        public async Task<IEnumerable<string>> GetDatabaseObjectsAsync(int type)
        {
            string query = null;
            if (type == 0)
                query = "SELECT name FROM sys.objects WHERE type = N'U' ORDER BY name"; // Tables
            else if (type == 1)
                query = "SELECT name FROM sys.objects WHERE type = N'V' ORDER BY name"; // Views

            if (string.IsNullOrEmpty(query))
                return new string[0];

            var result = await _unitOfWork.Connection.QueryAsync<string>(query, _unitOfWork.Transaction);
            return result.ToList();
        }

        public async Task<List<DbTableColumnInfo>> GetDatabaseObjectColumnsAsync(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                throw new ArgumentNullException(nameof(objectName));

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

            var result = await _unitOfWork.Connection.QueryAsync<DbTableColumnInfo>(query, new { ObjectName = objectName }, _unitOfWork.Transaction);
            return result.ToList();
        }

        public async Task<string> GetStoredProcedureScriptAsync(string spName)
        {
            if (string.IsNullOrWhiteSpace(spName))
                throw new ArgumentNullException(nameof(spName));

            const string query = @"
                    SELECT [Definition] 
                    FROM sys.sql_modules 
                    WHERE OBJECTPROPERTY(OBJECT_ID, 'IsProcedure') = 1
                          AND OBJECT_NAME(OBJECT_ID) = @SpName"
            ;

            var result = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<string>(query, new { SpName = spName }, _unitOfWork.Transaction);
            return result;
        }

        #endregion
    }
}
