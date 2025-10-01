using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using static Dapper.SqlMapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repository
{
    public class RepositoryBase : IRepositoryBase
    {
        protected readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public RepositoryBase(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var query = $"SELECT * FROM {table} WHERE Id = @Value";
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            return await _cacheService.GetOrCreateAsync<T>(cacheAttr.key + $"_{id}", () =>
              _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(
                query,
                new { Value = id }
            ), cacheAttr.timeOut);
        }

        public async Task<T> GetByColumnAsync<T>(string column, object value) where T : class, IEntity, new()
        {
            if (!typeof(T).GetProperties().Any(p => p.Name == column))
                throw new ArgumentException($"Invalid column name {column}.");

            var table = AttributeCache.Instance.GetTableName<T>();
            var query = $"SELECT * FROM {table} WHERE {column} = @Value";
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var result = await _cacheService.GetOrCreateAsync<T>(cacheAttr.key + $"_{column}_{value}", () =>
             _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(
                query,
                new { Value = value }
            ), cacheAttr.timeOut);

            return result ?? default(T);
        }

        public async Task<IEnumerable<TColumnType>> GetColumnValuesAsync<T, TColumnType>(string column) where T : class, IEntity, new()
        {
            if (!typeof(T).GetProperties().Any(p => p.Name == column))
                throw new ArgumentException($"Invalid column name {column}.");

            var table = AttributeCache.Instance.GetTableName<T>();
            var query = $"SELECT {column} FROM {table}";
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            return await _cacheService.GetOrCreateAsync<IEnumerable<TColumnType>>(cacheAttr.key + $"_{column}", () =>
             _unitOfWork.Connection.QueryAsync<TColumnType>(
                query,
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<TColumnType> GetColumnValueAsync<T, TColumnType>(Guid id, string column) where T : class, IEntity, new()
        {
            if (!typeof(T).GetProperties().Any(p => p.Name == column))
                throw new ArgumentException($"Invalid column name {column}.");

            var table = AttributeCache.Instance.GetTableName<T>();
            var query = $"SELECT {column} FROM {table} WHERE Id = @Value";
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            return await _cacheService.GetOrCreateAsync<TColumnType>(cacheAttr.key + $"_{column}_{id}", () =>
             _unitOfWork.Connection.ExecuteScalarAsync<TColumnType>(
                query,
                new { Value = id },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<TColumnType> GetColumnValueAsync<T, TColumnType>(string column, string filerColumn, object filterValue) where T : class, IEntity, new()
        {
            if (!typeof(T).GetProperties().Any(p => p.Name == column))
                throw new ArgumentException($"Invalid column name {column}.");

            if (!typeof(T).GetProperties().Any(p => p.Name == filerColumn))
                throw new ArgumentException($"Invalid column name {column}.");

            var table = AttributeCache.Instance.GetTableName<T>();
            var query = $"SELECT {column} FROM {table} WHERE {filerColumn} = @Value";
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            return await _cacheService.GetOrCreateAsync<TColumnType>(cacheAttr.key + $"_{column}_{filerColumn}_{filterValue}", () =>
             _unitOfWork.Connection.ExecuteScalarAsync<TColumnType>(
                query,
                new { Value = filterValue },
                _unitOfWork.Transaction
            ), cacheAttr.timeOut);
        }

        public async Task<IEnumerable<T>> GetByScopeAsync<T>(object value, params string[] orderColumns) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var scopeColumn = AttributeCache.Instance.GetScope<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();
            var cacheKey = !string.IsNullOrEmpty(cacheAttr.key)
                ? cacheAttr.key + $"_Scope_{value}"
                : string.Empty;

            var query = $"SELECT * FROM {table} WHERE {scopeColumn} = @Value";

            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var column = orderColumn.Split(' ')[0];
                if (!typeof(T).GetProperties().Any(p => p.Name == column))
                    throw new ArgumentException($"Invalid column name {column}.");

                sorts.Add(orderColumn);
            }
            if (sorts.Any()) query += $" ORDER BY {string.Join(",", sorts)}";

            return await _cacheService.GetOrCreateAsync<IEnumerable<T>>(cacheKey, () =>
             _unitOfWork.Connection.QueryAsync<T>(
                query,
                new { Value = value }
            ), cacheAttr.timeOut);
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetByScopeAsync<T>(Guid value, int pageIndex, int pageSize) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var scopeColumn = AttributeCache.Instance.GetScope<T>();

            var sql = $@"SELECT * FROM dbo.{table} WHERE {scopeColumn} = @Value
                ORDER BY CreatedOnDate DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

                SELECT COUNT(*) FROM dbo.{table} WHERE {scopeColumn} = @Value;";

            using (var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new
            {
                Skip = (pageIndex - 1) * pageSize,
                Take = pageSize,
                Value = value
            }))
            {
                var all = await multi.ReadAsync<T>();
                var totalCount = await multi.ReadFirstAsync<int>();
                return (all, totalCount);
            }
        }

        public async Task<IEnumerable<T>> GetItemsByColumnAsync<T>(string column, object value, params string[] orderColumns) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            if (!typeof(T).GetProperties().Any(p => p.Name == column))
                throw new ArgumentException($"Invalid column name {column}.");

            var query = $"SELECT * FROM {table} WHERE {column} = @Value";

            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var col = orderColumn.Split(' ')[0];
                if (!typeof(T).GetProperties().Any(p => p.Name == col))
                    throw new ArgumentException($"Invalid column name {col}.");

                sorts.Add(orderColumn);
            }
            if (sorts.Any()) query += $" ORDER BY {string.Join(",", sorts)}";

            return await _cacheService.GetOrCreateAsync<IEnumerable<T>>(cacheAttr.key + $"_Items_{column}_{value}", () =>
            _unitOfWork.Connection.QueryAsync<T>(
                query,
                new { Value = value }
            ), cacheAttr.timeOut);
        }

        public async Task<IEnumerable<T>> GetItemsByColumnsAsync<T>(string[] columns, object values) where T : class, IEntity, new()
        {
            if (!typeof(T).GetProperties().Any(p => columns.Contains(p.Name)))
                throw new ArgumentException($"Invalid column name.");

            string condition = string.Join(" or ", columns.Select(column => $"{column} = @{column}"));

            var table = AttributeCache.Instance.GetTableName<T>();
            var query = $"SELECT * FROM {table} WHERE {condition}";

            return await
            _unitOfWork.Connection.QueryAsync<T>(
                query,
                values
            );
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(params string[] orderColumns) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var query = $"SELECT * FROM {table}";
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var sorts = new List<string>();
            foreach (var orderColumn in orderColumns)
            {
                var column = orderColumn.Split(' ')[0];
                if (!typeof(T).GetProperties().Any(p => p.Name == column))
                    throw new ArgumentException($"Invalid column name {column}.");

                sorts.Add(orderColumn);
            }
            if (sorts.Any()) query += $" ORDER BY {string.Join(",", sorts)}";

            return await _cacheService.GetOrCreateAsync<IEnumerable<T>>(cacheAttr.key, () =>
             _unitOfWork.Connection.QueryAsync<T>(
                query
            ), cacheAttr.timeOut);
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetByPage<T>(int pageIndex, int pageSize) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();

            var sql = $@"SELECT * FROM dbo.{table}
                ORDER BY CreatedOnDate DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

                SELECT COUNT(*) FROM dbo.{table};";

            using (var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql, new
            {
                Skip = (pageIndex - 1) * pageSize,
                Take = pageSize
            }))
            {
                var all = await multi.ReadAsync<T>();
                var totalCount = await multi.ReadFirstAsync<int>();
                return (all, totalCount);
            }
        }

        public async Task<Guid> AddAsync<T>(T entity, bool isNew = false) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite);

            if (isNew || entity.Id == Guid.Empty)
            {
                if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

                var createdOnDateProp = properties.FirstOrDefault(p => p.Name == "CreatedOnDate");
                if (createdOnDateProp != null)
                    createdOnDateProp.SetValue(entity, DateTime.Now);

                var createdByUserProp = properties.FirstOrDefault(p => p.Name == "CreatedByUserId");
                if (createdByUserProp != null)
                    createdByUserProp.SetValue(entity, Constants.CurrentUser.UserID);
            }

            var lastModifiedDateProp = properties.FirstOrDefault(p => p.Name == "LastModifiedOnDate");
            if (lastModifiedDateProp != null)
                lastModifiedDateProp.SetValue(entity, DateTime.Now);

            var lastModifiedByUserProp = properties.FirstOrDefault(p => p.Name == "LastModifiedByUserId");
            if (lastModifiedByUserProp != null)
                lastModifiedByUserProp.SetValue(entity, Constants.CurrentUser.UserID);

            var columns = string.Join(", ", properties.Select(p => p.Name));
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

        /// <summary>
        /// Performs a bulk insert of a list of entities into the specified table.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entities">The list of entities to insert</param>
        /// <param name="tableName">The name of the target database table</param>
        /// <param name="transaction">Optional transaction</param>
        /// <returns>The total number of rows inserted</returns>
        public async Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, IEntity, new()
        {
            if (entities == null || !entities.Any())
                return 0;

            // Get all properties of the entity that are not marked with [IgnoreInsert]
            var properties = typeof(T).GetProperties()
            //    .Where(p => !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute)))
                .ToArray();

            var entityList = entities.ToList();
            foreach (var entity in entityList)
            {
                if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

                var lastModifiedDateProp = properties.FirstOrDefault(p => p.Name == "LastModifiedOnDate");
                if (lastModifiedDateProp != null)
                    lastModifiedDateProp.SetValue(entity, DateTime.Now);

                var lastModifiedByUserProp = properties.FirstOrDefault(p => p.Name == "LastModifiedByUserId");
                if (lastModifiedByUserProp != null)
                    lastModifiedByUserProp.SetValue(entity, Constants.CurrentUser.UserID);

                var createdOnDateProp = properties.FirstOrDefault(p => p.Name == "CreatedOnDate");
                if (createdOnDateProp != null)
                    createdOnDateProp.SetValue(entity, DateTime.Now);

                var createdByUserProp = properties.FirstOrDefault(p => p.Name == "CreatedByUserId");
                if (createdByUserProp != null)
                    createdByUserProp.SetValue(entity, Constants.CurrentUser.UserID);
            }

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            // Generate column names and parameter names for the SQL query
            var columnNames = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
            var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var sql = $"INSERT INTO [{table}] ({columnNames}) VALUES ({parameterNames})";

            var result = await _unitOfWork.Connection.ExecuteAsync(sql, entityList, _unitOfWork.Transaction);

            _cacheService.ClearByPrefix(cacheAttr.key);

            return result;
        }

        public async Task<bool> UpdateAsync<T>(T entity, params string[] updatedColumns) where T : class, IEntity, new()
        {
            if (entity.Id == Guid.Empty)
                throw new ArgumentException("Entity must have a valid Id.");

            var table = AttributeCache.Instance.GetTableName<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var properties = typeof(T).GetProperties()
                                      .Where(p => p.CanRead && p.CanWrite && p.Name != "Id"
                                                 && p.Name != "CreatedOnDate"
                                                 && p.Name != "CreatedByUserId")
                                      .ToList();

            if (updatedColumns.Length > 0)
            {
                properties = properties.Where(p => updatedColumns.Contains(p.Name)).ToList();
            }

            var lastModifiedOnDateProp = properties.FirstOrDefault(p => p.Name == "LastModifiedOnDate");
            if (lastModifiedOnDateProp != null)
                lastModifiedOnDateProp.SetValue(entity, DateTime.Now);

            var lastModifiedByUserProp = properties.FirstOrDefault(p => p.Name == "LastModifiedByUserId");
            if (lastModifiedByUserProp != null)
                lastModifiedByUserProp.SetValue(entity, Constants.CurrentUser.UserID);

            if (!properties.Any())
                throw new InvalidOperationException("No valid properties to update.");

            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
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
            if (!typeof(T).GetProperties().Any(p => p.Name == column))
                throw new ArgumentException($"Invalid column name {column}.");

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
            if (!typeof(T).GetProperties().Any(p => p.Name == column))
                throw new ArgumentException($"Invalid column name {column}.");

            if (!typeof(T).GetProperties().Any(p => p.Name == filterColumn))
                throw new ArgumentException($"Invalid column name {column}.");

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

        public async Task<bool> DeleteByScopeAsync<T>(Guid value) where T : class, IEntity, new()
        {
            var table = AttributeCache.Instance.GetTableName<T>();
            var scopeColumn = AttributeCache.Instance.GetScope<T>();
            var cacheAttr = AttributeCache.Instance.GetCache<T>();

            var query = $"DELETE FROM {table} WHERE {scopeColumn} = @Value";
            int rowAffected = await _unitOfWork.Connection.ExecuteAsync(query, new { Value = value }, _unitOfWork.Transaction);

            _cacheService.RemoveByPrefix(cacheAttr.key);

            return rowAffected >= 1;
        }

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
            return await _unitOfWork.Connection.ExecuteScalarAsync<T>(
                storedProcedure,
                param: parameters,
                commandType: CommandType.StoredProcedure,
                transaction: _unitOfWork.Transaction
            );
        }

        public async Task<IDataReader> ExecuteStoredProcedureAsDataReaderAsync(string storedProcedure, string cacheKey, object parameters)
        {
            return await _unitOfWork.Connection.ExecuteReaderAsync(
                    storedProcedure,
                    parameters,
                    _unitOfWork.Transaction,
                    commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsListAsync<T>(string storedProcedure, string cacheKey, object parameters)
        {
            return await _unitOfWork.Connection.QueryAsync<T>(
                    storedProcedure,
                    parameters,
                    _unitOfWork.Transaction,
                    commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Executes a stored procedure and returns two strongly-typed result sets.
        /// </summary>
        /// <summary>
        /// Executes a stored procedure for paging and caches the final result.
        /// This version safely disposes the GridReader and only caches the materialized data.
        /// </summary>
        public async Task<(IEnumerable<T> Items, int TotalCount)> ExecuteStoredProcedureForPagingAsync<T>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            // Check cache first
            var cachedResult = _cacheService.Get<(IEnumerable<T>, int)?>(cacheKey);
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IEnumerable<T> Items, int TotalCount) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                var totalCount = await grid.ReadSingleAsync<int>();
                var items = await grid.ReadAsync<T>();
                result = (items.ToList(), totalCount); // Materialize to avoid deferred reading
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }

        /// <summary>
        /// Executes a stored procedure and returns two strongly-typed result sets.
        /// </summary>
        /// <summary>
        /// Executes a stored procedure for paging and caches the final result.
        /// This version safely disposes the GridReader and only caches the materialized data.
        /// </summary>
        public async Task<(IEnumerable<T1>, IEnumerable<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            // Check cache first
            var cachedResult = _cacheService.Get<(IEnumerable<T1>, IEnumerable<T2>)?>(cacheKey);
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IEnumerable<T1>, IEnumerable<T2>) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                result.Item1 = await grid.ReadAsync<T1>();
                result.Item2 = await grid.ReadAsync<T2>();
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }


        /// <summary>
        /// Executes a stored procedure and returns two strongly-typed result sets.
        /// </summary>
        /// <summary>
        /// Executes a stored procedure for paging and caches the final result.
        /// This version safely disposes the GridReader and only caches the materialized data.
        /// </summary>
        public async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            // Check cache first
            var cachedResult = string.IsNullOrEmpty(cacheKey)
                ? _cacheService.Get<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)?>(cacheKey)
                : null;
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                result.Item1 = await grid.ReadAsync<T1>();
                result.Item2 = await grid.ReadAsync<T2>();
                result.Item3 = await grid.ReadAsync<T3>();
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }

        /// <summary>
        /// Executes a stored procedure and returns two strongly-typed result sets.
        /// </summary>
        /// <summary>
        /// Executes a stored procedure for paging and caches the final result.
        /// This version safely disposes the GridReader and only caches the materialized data.
        /// </summary>
        public async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3, T4>(
            string storedProcedure,
            string cacheKey,
            object parameters = null)
        {
            // Check cache first
            var cachedResult = _cacheService.Get<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)?>(cacheKey);
            if (cachedResult != null)
                return cachedResult.Value;

            // Query DB and dispose GridReader properly
            (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>) result;
            using (var grid = await _unitOfWork.Connection.QueryMultipleAsync(
                storedProcedure,
                parameters,
                _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure))
            {
                result.Item1 = await grid.ReadAsync<T1>();
                result.Item2 = await grid.ReadAsync<T2>();
                result.Item3 = await grid.ReadAsync<T3>();
                result.Item4 = await grid.ReadAsync<T4>();
            }

            // Cache the final materialized result
            _cacheService.Set(cacheKey, result);
            return result;
        }
    }
}