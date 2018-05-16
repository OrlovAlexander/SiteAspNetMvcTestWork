using NHibernate;
using System;
using System.Data;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericTransaction BeginTransaction();
        IGenericTransaction BeginTransaction(IsolationLevel isolationLevel);
        void TransactionalFlush();
        void TransactionalFlush(IsolationLevel isolationLevel);
        bool IsInActiveTransaction { get; }
        //IUnitOfWorkFactory Factory { get; }
        ISession Session { get; }
        //void Flush();
    }
}
