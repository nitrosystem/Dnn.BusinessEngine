using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Template
{
    public interface ITemplateRepository
    {
        Task<Guid> AddAsync(TemplateInfo objTemplateInfo);

        Task<TemplateInfo> GetAsync(Guid id);
        Task<IReadOnlyList<TemplateInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(TemplateInfo objTemplateInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
