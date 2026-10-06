using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Extension
{
    public class ExtensionRepository : IExtensionRepository
    {
        private readonly ISql _sql;

        public ExtensionRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ExtensionInfo objExtensionInfo)
        {
            return await _sql.InsertAsync<ExtensionInfo>(objExtensionInfo);
        }

        public async Task<string> GetVersionAsync(string extensionName)
        {
            return await _sql.GetColumnValueAsync<ExtensionInfo, string>("Version", "ExtensionName", extensionName);
        }

        public async Task<ExtensionInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ExtensionInfo>(id);
        }

        public async Task<IReadOnlyList<ExtensionInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ExtensionInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ExtensionInfo objExtensionInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ExtensionInfo>(objExtensionInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ExtensionInfo>(id);
        }
    }
}
