using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Template
{
    public class TemplateRepository: ITemplateRepository
    {
        private readonly ISql _sql;

        public TemplateRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(TemplateInfo objTemplateInfo)
        {
            return await _sql.InsertAsync<TemplateInfo>(objTemplateInfo);
        }

        public async Task<TemplateInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<TemplateInfo>(id);
        }

        public async Task<IReadOnlyList<TemplateInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<TemplateInfo>(columns);
        }

        public async Task<bool> UpdateAsync(TemplateInfo objTemplateInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<TemplateInfo>(objTemplateInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<TemplateInfo>(id);
        }
    }
}
