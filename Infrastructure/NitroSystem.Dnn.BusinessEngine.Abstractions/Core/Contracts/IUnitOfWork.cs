using System;
using System.Data;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}
