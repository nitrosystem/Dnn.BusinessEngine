using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;
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
using NitroSystem.Dnn.BusinessEngine.Core.WebSocketServer;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using System.Globalization;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Common.IO;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class EntityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public EntityService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = new RepositoryBase(_unitOfWork, _cacheService);
        }

        public async Task<EntityViewModel> GetEntityViewModelAsync(Guid id)
        {
            var entityTask = _repository.GetAsync<EntityInfo>(id);
            var entityColumnsTask = _repository.GetByScopeAsync<EntityColumnInfo>(id);

            await Task.WhenAll(entityTask, entityColumnsTask);

            var columns = BaseMapping<EntityColumnInfo, EntityColumnViewModel>.MapViewModels(entityColumnsTask.Result);

            return EntityMapping.MapEntityViewModel(entityTask.Result, columns);
        }

        public async Task<(IEnumerable<EntityViewModel> Items, int? TotalCount)> GetEntitiesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, byte? entityType, bool? isReadonly, string sortBy)
        {
            var results =
                await _repository
                    .ExecuteStoredProcedureMultiGridResultAsync(
                        "BusinessEngine_GetEntitiesWithColumns",
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

        public async Task<Guid> SaveEntity(EntityViewModel entity, bool isNew, HttpContext context, WebSocketServer ws)
        {
            var queries = new StringBuilder();

            await ws.SendMessageToClientAsync(10, "Mapping EntityViewModel to EntityInfo...");
            var objEntityInfo = EntityMapping.MapEntityInfo(entity);

            string oldTableName = string.Empty;

            if (isNew)
            {
                await ws.SendMessageToClientAsync(20, "Adding Entity in database table...");
                objEntityInfo.Id = await _repository.AddAsync<EntityInfo>(objEntityInfo);
                await ws.SendMessageToClientAsync(30, "Entity added to database successfully. Done!");
            }
            else
            {
                oldTableName = await _repository.GetColumnValueAsync<EntityInfo, string>(objEntityInfo.Id, objEntityInfo.TableName);
                await ws.SendMessageToClientAsync(30, "Get old Entity table name from database...");

                await ws.SendMessageToClientAsync(35, "Updating Entity in database table...");
                var isUpdated = await _repository.UpdateAsync<EntityInfo>(objEntityInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objEntityInfo);
                await ws.SendMessageToClientAsync(40, "Entity updated to database successfully. Done!");
            }

            if (!entity.IsReadonly)
            {
                if (!string.IsNullOrEmpty(oldTableName) && oldTableName != entity.TableName) queries.AppendLine($"exec sp_rename '{oldTableName}', '{entity.TableName}'");

                var primaryColumn = entity.Columns.First();
                string query = FileUtil.GetFileContent(context.Server.MapPath("~/DesktopModules/BusinessEngine/data/sql-templates/create-entity.sql"));
                query = query.Replace("{TableName}", entity.TableName);
                query = query.Replace("{PrimaryColumnName}", primaryColumn.ColumnName);
                query = query.Replace("{PrimaryColumnType}", primaryColumn.ColumnType);
                query = query.Replace("{PrimaryIsIdentity}", primaryColumn.IsIdentity ? "IDENTITY (1, 1)" : "");
                queries.AppendLine(query);

                IEnumerable<EntityColumnInfo> oldColumns = Enumerable.Empty<EntityColumnInfo>();

                if (!isNew)
                {
                    oldColumns = await _repository.GetByScopeAsync<EntityColumnInfo>(objEntityInfo.Id);
                    await ws.SendMessageToClientAsync(50, "Get old EntityColumns from database.");

                    // the Columns that must be delete
                    foreach (EntityColumnInfo column in oldColumns.Where(c => !entity.Columns.Select(cc => cc.Id).Contains(c.Id)))
                    {
                        query = $@"IF EXISTS (SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[{entity.TableName}]') and [name] = '{column.ColumnName}') ALTER TABLE [dbo].[{entity.TableName}] DROP COLUMN [{column.ColumnName}]";
                        queries.AppendLine(query);

                        await _repository.DeleteAsync<EntityColumnInfo>(column.Id);
                        await ws.SendMessageToClientAsync(55, $"{column.ColumnName} Column deleted from database.");
                    }
                }

                var viewOrder = 0;
                foreach (EntityColumnViewModel column in entity.Columns.Where(c => !c.IsPrimary || isNew).OrderBy(c => c.ViewOrder))
                {
                    column.EntityId = objEntityInfo.Id;

                    await ws.SendMessageToClientAsync(60, $"Mapping {column.ColumnName} Column to EntityColumnViewModel...");
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

                    if (isNew)
                    {
                        await _repository.AddAsync<EntityColumnInfo>(objEntityColumnInfo);
                        await ws.SendMessageToClientAsync(70, $"{column.ColumnName} column added to database successfully. Done!");
                    }
                    else
                    {
                        await _repository.UpdateAsync(objEntityColumnInfo);
                        await ws.SendMessageToClientAsync(70, $"{column.ColumnName} column updated to database successfully. Done!");
                    }
                }

                foreach (var relationship in entity.Relationships ?? Enumerable.Empty<TableRelationshipInfo>())
                {
                    query = @"
                        IF (OBJECT_ID('dbo.{0}', 'F') IS NOT NULL)
                        BEGIN
                            ALTER TABLE dbo.{1} DROP CONSTRAINT {0}
                        END

                        ALTER TABLE dbo.{1} ADD CONSTRAINT
	                        {0} FOREIGN KEY
	                        (
	                        {2}
	                        ) REFERENCES dbo.{3}
	                        (
	                        {4}
	                        ) {5}
	                         {6}";

                    relationship.ChildEntityTableName = entity.TableName;


                    query += relationship.EnforceForReplication ? "" : "\n NOT FOR REPLICATION";
                    query += relationship.EnforceForeignKeyConstraint ? "" : "\n ALTER TABLE dbo.{1} NOCHECK CONSTRAINT {0}";
                    queries.AppendLine(query);

                    //Add Relationsheeps

                    relationship.DELETESpecification = string.IsNullOrEmpty(relationship.DELETESpecification) ? "" : "ON DELETE " + (relationship.DELETESpecification ?? "").Replace("NO_ACTION", "NO ACTION");
                    relationship.UPDATESpecification = string.IsNullOrEmpty(relationship.UPDATESpecification) ? "" : "ON UPDATE " + (relationship.UPDATESpecification ?? "").Replace("NO_ACTION", "NO ACTION");

                    relationship.DELETESpecification = (relationship.DELETESpecification ?? "").Replace("SET_NULL", "SET NULL");
                    relationship.UPDATESpecification = (relationship.UPDATESpecification ?? "").Replace("SET_NULL", "SET NULL");

                    relationship.DELETESpecification = (relationship.DELETESpecification ?? "").Replace("SET_DEFAULT", "SET DEFAULT");
                    relationship.UPDATESpecification = (relationship.UPDATESpecification ?? "").Replace("SET_DEFAULT", "SET DEFAULT");

                    query = string.Format(query, relationship.RelationshipName, relationship.ChildEntityTableName, string.Join(",", relationship.Columns.Select(c => c.PrimaryKey)), relationship.ParentEntityTableName, string.Join(",", relationship.Columns.Select(c => c.ForeignKey)), relationship.UPDATESpecification, relationship.DELETESpecification);
                    queries.AppendLine(query);
                }

                await ws.SendMessageToClientAsync(90, $"Executing entity queries on database...");
                string token = TokenGenerator.GenerateToken(entity.TableName);
                await _repository.ExecuteQueryByToken(token, entity.TableName, queries.ToString());
                await ws.SendMessageToClientAsync(100, $"Executed entity queries on database. Done!");
            }

            return objEntityInfo.Id;
        }

        public async Task<bool> DeleteEntityAsync(Guid id)
        {
            string tableName = await _repository.GetColumnValueAsync<EntityInfo, string>(id, "TableName");
            string token = TokenGenerator.GenerateToken(tableName);

            string query = $"DROP TABLE {tableName};";

            var task1 = _repository.DeleteAsync<EntityInfo>(id);
            var task2 = _repository.ExecuteQueryByToken(token, tableName, query);
            await Task.WhenAll(task1, task2);

            return task1.Result;
        }
    }
}
