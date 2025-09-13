using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System.Globalization;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class EntityService : IEntityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        public EntityService(IUnitOfWork unitOfWork, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<EntityViewModel> GetEntityViewModelAsync(Guid entityId)
        {
            var entityTask = _repository.GetAsync<EntityInfo>(entityId);
            var entityColumnsTask = _repository.ExecuteStoredProcedureAsListAsync<EntityColumnInfo>("BusinessEngine_GetEntityColumns",
                new
                {
                    EntityId = entityId
                });

            await Task.WhenAll(entityTask, entityColumnsTask);

            var columns = BaseMapping<EntityColumnInfo, EntityColumnViewModel>.MapViewModels(entityColumnsTask.Result);

            return EntityMapping.MapEntityViewModel(entityTask.Result, columns);
        }

        public async Task<(IEnumerable<EntityViewModel> Items, int? TotalCount)> GetEntitiesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, byte? entityType, bool? isReadonly, string sortBy)
        {
            var results = await _repository.ExecuteStoredProcedureMultiGridResultAsync(
                        "BusinessEngine_GetEntitiesWithColumns", "Studio_Entities_",
                        new
                        {
                            ScenarioId = scenarioId,
                            SearchText = searchText,
                            EntityType = entityType,
                            IsReadonly = isReadonly,
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            SortBy = sortBy
                        },
                        grid => grid.ReadSingle<int?>(),
                        grid => grid.Read<EntityInfo>(),
                        grid => grid.Read<EntityColumnInfo>()
                    );

            var totalCount = results[0] as int?;
            var entities = results[1] as IEnumerable<EntityInfo>;
            var entityColumns = results[2] as IEnumerable<EntityColumnInfo>;

            return (EntityMapping.MapEntitiesViewModel(entities, entityColumns), totalCount);
        }

        public async Task<Guid> SaveEntity(EntityViewModel entity, bool isNew, HttpContext context)
        {
            var objEntityInfo = EntityMapping.MapEntityInfo(entity);

            _unitOfWork.BeginTransaction();

            try
            {
                var queries = new StringBuilder();

                string oldTableName = string.Empty;

                if (isNew)
                {
                    objEntityInfo.Id = await _repository.AddAsync<EntityInfo>(objEntityInfo);
                }
                else
                {
                    oldTableName = await _repository.GetColumnValueAsync<EntityInfo, string>(objEntityInfo.Id, "TableName");

                    var isUpdated = await _repository.UpdateAsync<EntityInfo>(objEntityInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objEntityInfo);
                }

                string query = string.Empty;

                if (!entity.IsReadonly)
                {
                    if (!string.IsNullOrEmpty(oldTableName) && oldTableName != entity.TableName) queries.AppendLine($"exec sp_rename '{oldTableName}', '{entity.TableName}'");

                    if (isNew)
                    {
                        var primaryColumn = entity.Columns.First();
                        query = FileUtil.GetFileContent(context.Server.MapPath("~/DesktopModules/BusinessEngine/data/sql-templates/create-entity.sql"));
                        query = query.Replace("{TableName}", entity.TableName);
                        query = query.Replace("{PrimaryColumnName}", primaryColumn.ColumnName);
                        query = query.Replace("{PrimaryColumnType}", primaryColumn.ColumnType);
                        query = query.Replace("{PrimaryIsIdentity}", primaryColumn.IsIdentity ? "IDENTITY (1, 1)" : "");
                        queries.AppendLine(query);
                    }

                    IEnumerable<EntityColumnInfo> oldColumns = Enumerable.Empty<EntityColumnInfo>();

                    if (!isNew)
                    {
                        oldColumns = await _repository.GetByScopeAsync<EntityColumnInfo>(objEntityInfo.Id);

                        // the Columns that must be delete
                        foreach (EntityColumnInfo column in oldColumns.Where(c => !entity.Columns.Select(cc => cc.Id).Contains(c.Id)))
                        {
                            query = $@"IF EXISTS (SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[{entity.TableName}]') and [name] = '{column.ColumnName}') ALTER TABLE [dbo].[{entity.TableName}] DROP COLUMN [{column.ColumnName}]";
                            queries.AppendLine(query);

                            await _repository.DeleteAsync<EntityColumnInfo>(column.Id);
                        }
                    }

                    var viewOrder = 0;
                    foreach (EntityColumnViewModel column in entity.Columns.Where(c => !c.IsPrimary || isNew).OrderBy(c => c.ViewOrder))
                    {
                        column.EntityId = objEntityInfo.Id;

                        var objEntityColumnInfo = BaseMapping<EntityColumnInfo, EntityColumnViewModel>.MapEntity(column);

                        if (isNew && column.IsPrimary)
                        {
                            await _repository.AddAsync<EntityColumnInfo>(objEntityColumnInfo);
                            continue;
                        }

                        objEntityColumnInfo.IsComputedColumn = column.ColumnType.Trim().StartsWith("as ");
                        objEntityColumnInfo.ViewOrder = viewOrder++;

                        var isNewColumn = isNew ? true : column.Id == Guid.Empty;

                        if (isNewColumn) // add new column
                        {
                            string allowNull = column.AllowNulls ? "NULL" : "NOT NULL";
                            //column must be create
                            queries.AppendLine($@"ALTER TABLE {entity.TableName} ADD {column.ColumnName} {column.ColumnType} {allowNull}");
                        }
                        else if (!isNewColumn) // modify column
                        {
                            var oldColumn = oldColumns.FirstOrDefault(c => c.Id == column.Id);

                            //column name is renamed
                            if (oldColumn != null && column.ColumnName != oldColumn.ColumnName)
                            {
                                queries.AppendLine($@"EXEC sp_RENAME '{entity.TableName}.{oldColumn.ColumnName}' , '{column.ColumnName}', 'COLUMN'");
                            }

                            //column type is changed and new type is formula or old type was formula
                            if (oldColumn != null && column.ColumnType != oldColumn.ColumnType && (column.ColumnType.ToLower().StartsWith("as ") || oldColumn.ColumnType.ToLower().StartsWith("as ")))
                            {
                                string allowNull = column.ColumnType.ToLower().StartsWith("as ") ? "" : column.AllowNulls ? "NULL" : "NOT NULL";
                                queries.AppendLine($@"ALTER TABLE {entity.TableName} DROP COLUMN {column.ColumnName} ALTER TABLE {entity.TableName} ADD {column.ColumnName} {column.ColumnType} {allowNull}");
                            }

                            //column type is changes
                            else if (oldColumn != null && (column.ColumnType != oldColumn.ColumnType || column.AllowNulls != oldColumn.AllowNulls))
                            {
                                string allowNull = column.AllowNulls ? "NULL" : "NOT NULL";
                                queries.AppendLine($@"ALTER TABLE {entity.TableName} ALTER COLUMN {column.ColumnName} {column.ColumnType} {allowNull}");
                            }

                            //remove the default value if deleted by the user
                            if (!string.IsNullOrWhiteSpace(oldColumn.DefaultValue))
                            {
                                queries.AppendLine($@"ALTER TABLE dbo.{entity.TableName} DROP CONSTRAINT DF_{entity.TableName}_{oldColumn.ColumnName}");
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                        {
                            queries.AppendLine($@"ALTER TABLE dbo.{entity.TableName} ADD CONSTRAINT DF_{entity.TableName}_{column.ColumnName} DEFAULT {column.DefaultValue} FOR {column.ColumnName}");
                        }

                        if (isNewColumn)
                        {
                            await _repository.AddAsync<EntityColumnInfo>(objEntityColumnInfo);
                        }
                        else
                        {
                            await _repository.UpdateAsync(objEntityColumnInfo);
                        }
                    }

                    if (queries.Length > 0)
                    {
                        var sqlCommand = new ExecuteSqlCommand(_unitOfWork);
                        await sqlCommand.ExecuteSqlCommandTextAsync(queries.ToString());
                    }
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return objEntityInfo.Id;
        }

        public async Task<bool> UpdateGroupColumn(Guid entityId, Guid? groupId)
        {
            return await _repository.UpdateColumnAsync<EntityInfo>("GroupId", groupId, entityId);
        }

        public async Task<bool> DeleteEntityAsync(Guid entityId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                string tableName = await _repository.GetColumnValueAsync<EntityInfo, string>(entityId, "TableName");
                string token = TokenGenerator.GenerateToken(tableName);

                string query = $"DROP TABLE {tableName};";

                var task1 = _repository.DeleteAsync<EntityInfo>(entityId);
                var task2 = _repository.ExecuteQueryByToken(token, tableName, query);

                await Task.WhenAll(task1, task2);

                _unitOfWork.Commit();

                return await task1;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }
        }
    }
}
