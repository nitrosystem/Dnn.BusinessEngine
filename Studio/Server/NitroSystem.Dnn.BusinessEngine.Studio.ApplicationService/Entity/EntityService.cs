using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Entity
{
	public class EntityService : IEntityService, IExportable, IImportable
    {
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEntityRepository _entityRepository;
		private readonly IEntityColumnRepository _entityColumnRepository;
		private readonly ISql _sql;

		public EntityService(IUnitOfWork unitOfWork, ISql sql, IEntityRepository entityRepository, IEntityColumnRepository entityColumnRepository)
		{
			_unitOfWork = unitOfWork;
			_sql = sql;
			_entityRepository = entityRepository;
			_entityColumnRepository = entityColumnRepository;
		}

		public async Task<(IEnumerable<EntityViewModel> Items, int? TotalCount)> GetEntitiesViewModelAsync(
			Guid scenarioId,
			int pageIndex,
			int pageSize,
			string searchText,
			int? entityType,
			bool? isReadonly,
			string sortBy)
		{
			var results = await _entityRepository.GetsAsync(scenarioId, pageIndex, pageSize, searchText, entityType, isReadonly, sortBy);
			var result = HybridMapper.MapWithChildren<EntityInfo, EntityViewModel, EntityColumnInfo, EntityColumnViewModel>(
				 results.Entities,
				 results.Columns,
				 parentKeySelector: p => p.Id,
				 childKeySelector: c => c.EntityId,
				 assignChildren: (parent, childs) => parent.Columns = childs
			 );

			return (result, results.TotalCount);
		}

		public async Task<IEnumerable<EntityListItem>> GetEntitiesListItemAsync(Guid scenarioId, string sortBy)
		{
			var results = await _entityRepository.GetsAsync(scenarioId, sortBy);
			return HybridMapper.MapWithChildren<EntityInfo, EntityListItem, EntityColumnInfo, EntityColumnListItem>(
			   parents: results.Entities,
			   children: results.Columns,
			   parentKeySelector: p => p.Id,
			   childKeySelector: c => c.EntityId,
			   assignChildren: (parent, childs) => parent.Columns = childs
		   );
		}

		public async Task<EntityViewModel> GetEntityViewModelAsync(Guid entityId)
		{
			var entity = await _entityRepository.GetAsync(entityId);
			var columns = await _entityColumnRepository.GetsAsync(entityId);
			return HybridMapper.MapWithChildren<EntityInfo, EntityViewModel, EntityColumnInfo, EntityColumnViewModel>(
				entity,
				columns,
				(parent, childs) => parent.Columns = childs
			);
		}

		public async Task<Guid> CreateEntityAsync(EntityViewModel entity)
		{
			var objEntityInfo = HybridMapper.Map<EntityViewModel, EntityInfo>(entity);

			_unitOfWork.BeginTransaction();

			try
			{
				var queries = new StringBuilder();
				var oldTableName = string.Empty;
				var isNew = objEntityInfo.Id == Guid.Empty;

				if (isNew)
				{
					objEntityInfo.Id = await _entityRepository.AddAsync(objEntityInfo);
				}
				else
				{
					oldTableName = await _entityRepository.GetTableNameAsync(objEntityInfo.Id);

					await _entityRepository.UpdateAsync(objEntityInfo);
				}

				string query = string.Empty;

				if (!entity.IsReadonly)
				{
					if (!string.IsNullOrEmpty(oldTableName) && oldTableName != entity.TableName)
						queries.AppendLine($"exec sp_rename '{oldTableName}', '{entity.TableName}'");

					if (isNew)
					{
						var primaryColumn = entity.Columns.First();
						query = FileUtil.GetFileContent(GlobalHelper.MapPath("~/DesktopModules/BusinessEngine/data/sql-templates/create-entity.sql"));
						query = query.Replace("{TableName}", entity.TableName);
						query = query.Replace("{PrimaryColumnName}", primaryColumn.ColumnName);
						query = query.Replace("{PrimaryColumnType}", primaryColumn.ColumnType);
						query = query.Replace("{PrimaryIsIdentity}", primaryColumn.IsIdentity ? "IDENTITY (1, 1)" : "");
						queries.AppendLine(query);
					}

					IEnumerable<EntityColumnInfo> oldColumns = Enumerable.Empty<EntityColumnInfo>();

					if (!isNew)
					{
						oldColumns = await _entityColumnRepository.GetsAsync(objEntityInfo.Id);

						// the Columns that must be delete
						foreach (EntityColumnInfo column in oldColumns.Where(c => !entity.Columns.Select(cc => cc.Id).Contains(c.Id)))
						{
							query = $@"IF EXISTS 
                                        (
                                            SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[{entity.TableName}]') and
                                                [name] = '{column.ColumnName}'
                                        ) 
                                            ALTER TABLE [dbo].[{entity.TableName}] DROP COLUMN [{column.ColumnName}]";

							queries.AppendLine(query);

							await _entityColumnRepository.DeleteAsync(column.Id);
						}
					}

					foreach (EntityColumnViewModel column in entity.Columns.Where(c => !c.IsPrimary || isNew).OrderBy(c => c.ViewOrder))
					{
						column.EntityId = objEntityInfo.Id;
						var objEntityColumnInfo = HybridMapper.Map<EntityColumnViewModel, EntityColumnInfo>(column);
						if (isNew && column.IsPrimary)
						{
							await _entityColumnRepository.AddAsync(objEntityColumnInfo);
							continue;
						}

						var isNewColumn = isNew
							? true
							: column.Id == Guid.Empty;
						if (isNewColumn) // add new column
						{
							string allowNull = column.AllowNulls ? "NULL" : "NOT NULL";

							//column must be create
							queries.AppendLine($@"ALTER TABLE {entity.TableName} ADD {column.ColumnName} {column.ColumnType} {allowNull}");
						}
						else if (!isNewColumn) // modify column
						{
							var oldColumn = oldColumns.FirstOrDefault(c => c.Id == column.Id);
							if (oldColumn != null && column.ColumnName != oldColumn.ColumnName) //column name is renamed
							{
								queries.AppendLine($@"
                                    EXEC sp_RENAME '{entity.TableName}.{oldColumn.ColumnName}' , '{column.ColumnName}', 'COLUMN'"
								);
							}

							//column type is changed and new type is formula or old type was formula
							if (oldColumn != null && column.ColumnType != oldColumn.ColumnType && (column.ColumnType.ToLower().StartsWith("as ") || oldColumn.ColumnType.ToLower().StartsWith("as ")))
							{
								string allowNull = column.ColumnType.ToLower().StartsWith("as ")
									? ""
									: (column.AllowNulls ? "NULL" : "NOT NULL");

								queries.AppendLine($@"
                                        ALTER TABLE {entity.TableName} 
                                        DROP COLUMN {column.ColumnName} 
                                        ALTER TABLE {entity.TableName} ADD {column.ColumnName} {column.ColumnType} {allowNull}"
								);
							}

							//column type is changes
							else if (oldColumn != null && (column.ColumnType != oldColumn.ColumnType || column.AllowNulls != oldColumn.AllowNulls))
							{
								string allowNull = column.AllowNulls
									? "NULL"
									: "NOT NULL";

								queries.AppendLine($@"
                                        ALTER TABLE {entity.TableName} 
                                        ALTER COLUMN {column.ColumnName} {column.ColumnType} {allowNull}"
								);
							}
						}

						if (isNewColumn)
							await _entityColumnRepository.AddAsync(objEntityColumnInfo);
						else
							await _entityColumnRepository.UpdateAsync(objEntityColumnInfo);
					}

					if (queries.Length > 0)
					{
						await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, queries.ToString());
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

		public async Task UpdateEntityGroupAsync(Guid? groupId, Guid entityId)
		{
			await _entityRepository.UpdateGroupAsync(groupId, entityId);
		}

		public async Task<bool> DeleteEntityAsync(Guid entityId)
		{
			_unitOfWork.BeginTransaction();

			try
			{
				bool isReadonly = await _entityRepository.GetIsReadonlyAsync(entityId);
				if (!isReadonly)
				{
					string tableName = await _entityRepository.GetTableNameAsync(entityId);
					string query = $"DROP TABLE {tableName};";

					await _sql.ExecuteSqlCommandTextAsync(_unitOfWork, query);
				}

				await _entityColumnRepository.DeleteAsync(entityId);
				await _entityRepository.DeleteAsync(entityId);

				_unitOfWork.Commit();

				return true;
			}
			catch (Exception ex)
			{
				_unitOfWork.Rollback();
				throw ex;
			}
		}

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var generateEntityScripts = context.Get<bool?>("GenerateEntityScripts");
                    var items = await GetEntitiesAndColumnsAsync(context.Get<Guid>("ScenarioId"), generateEntityScripts);

                    return new ExportResponse()
                    {
                        Result = items,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            var items = JsonConvert.DeserializeObject<List<object>>(json);
            var entities = JsonConvert.DeserializeObject<IReadOnlyList<EntityInfo>>(items[0].ToString());
            var entitiesColumns = JsonConvert.DeserializeObject<IReadOnlyList<EntityColumnInfo>>(items[1].ToString());
            var queries = JsonConvert.DeserializeObject<List<string>>(items[2].ToString());

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertEntitiesAndParamsAsync(entities, entitiesColumns);
                await ExecuteSqlQueriesAsync(queries, context);
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetEntitiesAndColumnsAsync(Guid scenarioId, bool? generateEntityScripts)
        {
            var entities = await _entityRepository.GetsAsync(scenarioId);
            var entitiesColumns = new List<EntityColumnInfo>();
            var queries = new List<string>();

            foreach (var entity in entities)
            {
                entitiesColumns.AddRange(await _entityColumnRepository.GetsAsync(entity.Id));

                if (generateEntityScripts.HasValue && generateEntityScripts.Value && !entity.IsReadonly && entity.EntityType == 0)
                {
                    queries.Add(await _sql.GenerateCreateTableScript("dbo", entity.TableName));
                }
            }

            return new List<object>() { entities, entitiesColumns, queries };
        }

        private async Task BulkInsertEntitiesAndParamsAsync(IReadOnlyList<EntityInfo> entities, IReadOnlyList<EntityColumnInfo> entitiesColumns)
        {
            await _entityRepository.BulkInsertAsync(entities);
            await _entityColumnRepository.BulkInsertAsync(entitiesColumns);
        }

        private async Task ExecuteSqlQueriesAsync(List<string> queries, ImportContext context)
        {
            foreach (var query in queries)
            {
                await _sql.ExecuteSqlCommandTextAsync(context.UnitOfWork, query);
            }
        }

        #endregion
    }
}
