using System;
using System.Data;

namespace NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        public void BeginTransaction();
        public void Commit();
        public void Rollback();
    }
}
