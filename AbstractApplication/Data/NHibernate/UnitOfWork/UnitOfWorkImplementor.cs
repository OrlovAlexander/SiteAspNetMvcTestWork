using NHibernate;
using System.Data;
using System.Diagnostics;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public class UnitOfWorkImplementor : IUnitOfWork
    {
        private readonly IUnitOfWorkFactory _factory;
        private readonly ISession _session;

        public UnitOfWorkImplementor(IUnitOfWorkFactory factory, ISession session)
        {
            _factory = factory;
            _session = session;
        }

        public IGenericTransaction BeginTransaction()
        {
            return new GenericTransaction(_session.BeginTransaction());
        }

        public IGenericTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return new GenericTransaction(_session.BeginTransaction(isolationLevel));
        }

        public void TransactionalFlush()
        {
            TransactionalFlush(IsolationLevel.ReadCommitted);
        }

        public void TransactionalFlush(IsolationLevel isolationLevel)
        {
            IGenericTransaction tx = BeginTransaction(isolationLevel);
            try
            {
                //forces a flush of the current unit of work
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
            finally
            {
                tx.Dispose();
            }
        }

        public bool IsInActiveTransaction
        {
            get
            {
                return _session.Transaction.IsActive;
            }
        }

        public ISession Session
        {
            get { return _session; }
        }

        public void Dispose()
        {
            Debug.WriteLine($"-Dispose - UnitOfWorkImplementor");
            _factory.Dispose();
            _session.Dispose();
        }

        //public IUnitOfWorkFactory Factory
        //{
        //    get { return _factory; }
        //}

        //public void Flush()
        //{
        //    _session.Flush();
        //}
    }
}
