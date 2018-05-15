using AbstractApplication.Infrastructure.LocalData;
using NHibernate;
using NHibernate.Cfg;
using System;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public static class UnitOfWork
    {
        private static IUnitOfWorkFactory _unitOfWorkFactory = new UnitOfWorkFactory();

        public const string CurrentUnitOfWorkKey = "CurrentUnitOfWork.Key";

        public static IUnitOfWork Start()
        {
            if (CurrentUnitOfWork != null)
                throw new InvalidOperationException("You cannot start more than one unit of work at the same time.");

            CurrentUnitOfWork = _unitOfWorkFactory.Create();
            return CurrentUnitOfWork;
        }

        private static IUnitOfWork CurrentUnitOfWork
        {
            get { return Local.Data[CurrentUnitOfWorkKey] as IUnitOfWork; }
            set { Local.Data[CurrentUnitOfWorkKey] = value; }
        }

        public static IUnitOfWork Current
        {
            get
            {
                if (CurrentUnitOfWork == null)
                    throw new InvalidOperationException("You are not in a unit of work.");
                return CurrentUnitOfWork;
            }
        }

        public static bool IsStarted
        {
            get { return CurrentUnitOfWork != null; }
        }

        public static ISession CurrentSession
        {
            get { return _unitOfWorkFactory.CurrentSession; }
            internal set { _unitOfWorkFactory.CurrentSession = value; }
        }

        public static Configuration Configuration
        {
            get { return _unitOfWorkFactory.Configuration; }
        }

        public static void DisposeUnitOfWork(UnitOfWorkImplementor adapter)
        {
            CurrentUnitOfWork = null;
        }
    }
}
