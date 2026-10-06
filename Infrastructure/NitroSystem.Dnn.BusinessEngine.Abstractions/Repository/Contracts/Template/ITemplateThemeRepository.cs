using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Template
{
    public interface ITemplateThemeRepository
    {
        Task<Guid> AddAsync(TemplateThemeInfo objTemplateThemeInfo);

        Task<TemplateThemeInfo> GetAsync(Guid id);
        Task<IReadOnlyList<TemplateThemeInfo>> GetsAsync(Guid templateId);
        Task<IReadOnlyList<TemplateThemeInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(TemplateThemeInfo objTemplateThemeInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
