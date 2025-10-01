using System;
using System.Data;

namespace NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed = false;

        public IDbTransaction Transaction => _transaction;
        public IDbConnection Connection => _connection;

        public UnitOfWork(IDbConnection connection)
        {
            _connection = connection;

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
        }

        public void BeginTransaction()
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit.");

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        public void Rollback()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to rollback.");

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _transaction?.Dispose();
            _connection?.Dispose();

            _disposed = true;
        }
    }
}
