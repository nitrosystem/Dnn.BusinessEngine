using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Template
{
    public class TemplateThemeRepository: ITemplateThemeRepository
    {
        private readonly ISql _sql;

        public TemplateThemeRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(TemplateThemeInfo objTemplateThemeInfo)
        {
            return await _sql.InsertAsync<TemplateThemeInfo>(objTemplateThemeInfo);
        }

        public async Task<TemplateThemeInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<TemplateThemeInfo>(id);
        }

        public async Task<IReadOnlyList<TemplateThemeInfo>> GetsAsync(Guid templateId)
        {
            return await _sql.GetByScopeAsync<TemplateThemeInfo>(templateId);
        }

        public async Task<IReadOnlyList<TemplateThemeInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<TemplateThemeInfo>(columns);
        }

        public async Task<bool> UpdateAsync(TemplateThemeInfo objTemplateThemeInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<TemplateThemeInfo>(objTemplateThemeInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<TemplateThemeInfo>(id);
        }
    }
}
