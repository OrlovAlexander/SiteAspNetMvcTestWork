using AbstractApplication.Infrastructure.LocalData;
using System;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public abstract class UnitOfWorkBase : INHibernateProviderFactory
    {
        private string _currentUnitOfWorkKey;

        protected IUnitOfWorkFactory _unitOfWorkFactory { get; private set; }

        public UnitOfWorkBase(IUnitOfWorkFactory unitOfWorkFactory, string unitOfWorkKey)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _currentUnitOfWorkKey = unitOfWorkKey;
        }

        public IUnitOfWork Start()
        {
            if (CurrentUnitOfWork != null)
                throw new InvalidOperationException("You cannot start more than one unit of work at the same time per one thread.");

            CurrentUnitOfWork = _unitOfWorkFactory.Create(this);
            return CurrentUnitOfWork;
        }

        private IUnitOfWork CurrentUnitOfWork
        {
            get { return Local.Data[_currentUnitOfWorkKey] as IUnitOfWork; }
            set { Local.Data[_currentUnitOfWorkKey] = value; }
        }

        public IUnitOfWork Current
        {
            get
            {
                if (CurrentUnitOfWork == null)
                    throw new InvalidOperationException("You are not in a unit of work.");
                return CurrentUnitOfWork;
            }
        }

        public bool IsStarted
        {
            get { return CurrentUnitOfWork != null; }
        }

        public void Configuration()
        {
            _unitOfWorkFactory.ConfigurationUp();
        }

        public void Dispose()
        {
            CurrentUnitOfWork = null;
        }

        //public ISession CurrentSession
        //{
        //    get { return _unitOfWorkFactory.CurrentSession; }
        //    internal set { _unitOfWorkFactory.CurrentSession = value; }
        //}

        //public void DisposeUnitOfWork(UnitOfWorkImplementor adapter)
        //{
        //    CurrentUnitOfWork = null;
        //}
    }
}
