using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public interface IRepositoryBase
    {
        Task<T> GetAsync<T>(Guid id) where T : class, IEntity, new();

        Task<T> GetByColumnAsync<T>(string column, object value) where T : class, IEntity, new();

        Task<IEnumerable<TColumnType>> GetColumnValuesAsync<T, TColumnType>(string column) where T : class, IEntity, new();

        Task<TColumnType> GetColumnValueAsync<T, TColumnType>(Guid id, string column) where T : class, IEntity, new();

        Task<TColumnType> GetColumnValueAsync<T, TColumnType>(string column, string filerColumn, object filterValue) where T : class, IEntity, new();

        Task<IEnumerable<T>> GetByScopeAsync<T>(object value, params string[] orderColumns) where T : class, IEntity, new();

        Task<(IEnumerable<T> Items, int TotalCount)> GetByScopeAsync<T>(Guid value, int pageIndex, int pageSize) where T : class, IEntity, new();

        Task<IEnumerable<T>> GetItemsByColumnAsync<T>(string column, object value, params string[] orderColumns) where T : class, IEntity, new();

        Task<IEnumerable<T>> GetItemsByColumnsAsync<T>(string[] columns, object values) where T : class, IEntity, new();

        Task<IEnumerable<T>> GetAllAsync<T>(params string[] orderColumns) where T : class, IEntity, new();

        Task<(IEnumerable<T> Items, int TotalCount)> GetByPage<T>(int pageIndex, int pageSize) where T : class, IEntity, new();

        Task<Guid> AddAsync<T>(T entity, bool isNew = false) where T : class, IEntity, new();

        Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, IEntity, new();

        Task<bool> UpdateAsync<T>(T entity, params string[] updatedColumns) where T : class, IEntity, new();

        Task<bool> UpdateColumnAsync<T>(string column, object value, Guid id) where T : class, IEntity, new();

        Task<bool> UpdateColumnAsync<T>(string column, string value, string filterColumn, object filterValue) where T : class, IEntity, new();

        Task<bool> DeleteAsync<T>(Guid id) where T : class, IEntity, new();

        Task<bool> DeleteByScopeAsync<T>(Guid value) where T : class, IEntity, new();

        Task ExecuteStoredProcedureAsync(string storedProcedure, object parameters);

        Task<T> ExecuteStoredProcedureScalerAsync<T>(string storedProcedure, string cacheKey, object parameters);

        Task<IDataReader> ExecuteStoredProcedureAsDataReaderAsync(string storedProcedure, string cacheKey, object parameters);

        Task<IEnumerable<T>> ExecuteStoredProcedureAsListAsync<T>(string storedProcedure, string cacheKey, object parameters);

        Task<(IEnumerable<T> Items, int TotalCount)> ExecuteStoredProcedureForPagingAsync<T>(
           string storedProcedure,
           string cacheKey,
           object parameters = null);

        Task<(IEnumerable<T1>, IEnumerable<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(
           string storedProcedure,
           string cacheKey,
           object parameters = null);

        Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3>(
           string storedProcedure,
           string cacheKey,
           object parameters = null);
        Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3, T4>(
           string storedProcedure,
           string cacheKey,
           object parameters = null);
    }
}