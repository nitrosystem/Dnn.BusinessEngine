using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts
{
    public interface ISql
    {
        #region Get Single Entity

        Task<T> GetAsync<T>(Guid id) where T : class, IEntity, new();
        Task<T> GetByColumnAsync<T>(string column, object value) where T : class, IEntity, new();

        #endregion

        #region Get Column Value(s)

        Task<IReadOnlyList<TColumnType>> GetColumnValuesAsync<T, TColumnType>(string column) where T : class, IEntity, new();
        Task<IReadOnlyList<TColumnType>> GetColumnsValueAsync<T, TColumnType>(string column, string filterColumn, object filterValue) where T : class, IEntity, new();
        Task<TColumnType> GetColumnValueAsync<T, TColumnType>(Guid id, string column) where T : class, IEntity, new();
        Task<TColumnType> GetColumnValueAsync<T, TColumnType>(string column, string filterColumn, object filterValue) where T : class, IEntity, new();
        TColumnType GetColumnValue<T, TColumnType>(string column, string filterColumn, object filterValue) where T : class, IEntity, new();
        Task<bool> ExistsAsync<T>() where T : class, IEntity, new();

        #endregion

        #region Get Multiple Entities

        Task<IReadOnlyList<T>> GetByScopeAsync<T>(object value, params string[] orderColumns) where T : class, IEntity, new();
        Task<IReadOnlyList<T>> GetItemsByColumnAsync<T>(string column, object value, params string[] orderColumns) where T : class, IEntity, new();
        Task<IReadOnlyList<T>> GetItemsByColumnsAsync<T>(string[] columns, object values, params string[] orderColumns) where T : class, IEntity, new();
        Task<IReadOnlyList<TChild>> GetChildsByParentColumnAsync<TParent, TChild>(string parentColumn, string childColumn, object value, params string[] orderColumns)
                    where TParent : class, IEntity, new()
                    where TChild : class, IEntity, new();
        Task<IReadOnlyList<T>> GetAllAsync<T>(params string[] orderColumns) where T : class, IEntity, new();

        #endregion

        #region Insert

        Task<Guid> InsertAsync<T>(T entity) where T : class, IEntity, new();
        Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, IEntity, new();

        #endregion

        #region Update

        Task<bool> UpdateAsync<T>(T entity, params string[] updatedColumns) where T : class, IEntity, new();
        Task<bool> UpdateColumnAsync<T>(string column, object value, Guid id) where T : class, IEntity, new();
        Task<bool> UpdateColumnAsync<T>(string column, string value, string filterColumn, object filterValue) where T : class, IEntity, new();

        #endregion

        #region Delete

        Task<bool> DeleteAsync<T>(Guid id) where T : class, IEntity, new();
        Task<bool> DeleteByScopeAsync<T>(object value) where T : class, IEntity, new();

        #endregion

        #region Stored Procedures - Execution

        Task ExecuteStoredProcedureAsync(string storedProcedure, object parameters);
        Task<T> ExecuteStoredProcedureScalerAsync<T>(string storedProcedure, string cacheKey, object parameters);

        #endregion

        #region Stored Procedures - Single Result

        Task<T> ExecuteStoredProcedureAsync<T>(string storedProcedure, string cacheKey, object parameters);
        T ExecuteStoredProcedure<T>(string storedProcedure, string cacheKey, object parameters);
        Task<object> ExecuteStoredProcedureAsync(Type type, string storedProcedure, string cacheKey, object parameters = null);

        #endregion

        #region Stored Procedures - List Result

        Task<IReadOnlyList<T>> ExecuteStoredProcedureAsListAsync<T>(string storedProcedure, string cacheKey, object parameters);
        IReadOnlyList<T> ExecuteStoredProcedureAsList<T>(string storedProcedure, string cacheKey, object parameters);
        Task<IReadOnlyList<object>> ExecuteStoredProcedureAsListAsync(Type type, string storedProcedure, string cacheKey, object parameters = null);

        #endregion

        #region Stored Procedures - Paging

        Task<(IReadOnlyList<object> Items, int TotalCount)> ExecuteStoredProcedureForPagingAsync(
                    Type type,
                    string storedProcedure,
                    string cacheKey,
                    object parameters = null);
        #endregion

        #region Stored Procedures - Multiple Result Sets

        Task<(IReadOnlyList<T1>, IReadOnlyList<T2>)> ExecuteStoredProcedureMultipleAsync<T1, T2>(
                    string storedProcedure,
                    string cacheKey,
                    object parameters = null);
        Task<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3>(
                    string storedProcedure,
                    string cacheKey,
                    object parameters = null);
        Task<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3, T4>(
                    string storedProcedure,
                    string cacheKey,
                    object parameters = null);
        Task<(IReadOnlyList<T1>, IReadOnlyList<T2>, IReadOnlyList<T3>, IReadOnlyList<T4>, IReadOnlyList<T5>)> ExecuteStoredProcedureMultipleAsync<T1, T2, T3, T4, T5>(
                    string storedProcedure,
                    string cacheKey,
                    object parameters = null);

        #endregion

        #region Execute Sql Command

        Task ExecuteSqlCommandTextAsync(IUnitOfWork unitOfWork, string commandText, object param = null);
        Task<IEnumerable<T>> ExecuteSqlCommandTextAsync<T>(IUnitOfWork unitOfWork, string commandText, object param = null);
        IDataReader ExecuteSqlReader(IUnitOfWork unitOfWork, CommandType commandType, string commandText, object param = null);

        #endregion

        #region Sql Database Metadata

        Task<IEnumerable<string>> GetDatabaseObjectsAsync(int type);
        Task<List<DbTableColumnInfo>> GetDatabaseObjectColumnsAsync(string objectName);
        Task<string> GetStoredProcedureScriptAsync(string spName);

        #endregion
    }
}
