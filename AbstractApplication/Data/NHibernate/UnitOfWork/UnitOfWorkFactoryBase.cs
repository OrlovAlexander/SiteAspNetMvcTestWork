using AbstractApplication.DesignByContract;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.IO;
using System.Xml;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public abstract class UnitOfWorkFactoryBase : IUnitOfWorkFactory
    {
        protected ISessionFactory SessionFactory;
        protected Configuration Configuration;

        public abstract void ConfigurationUp();

        public IUnitOfWork Create()
        {
            Check.Require(Configuration != null, "Configuration is null.");

            if (SessionFactory == null)
                SessionFactory = Configuration.BuildSessionFactory();

            ISession session = SessionFactory.OpenSession();
            session.FlushMode = FlushMode.Commit;
            return new UnitOfWorkImplementor(this, session);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        //public ISession CurrentSession
        //{
        //    get
        //    {
        //        if (_currentSession == null)
        //            throw new InvalidOperationException("You are not in a unit of work.");
        //        return _currentSession;
        //    }
        //    set { _currentSession = value; }
        //}

        //public void DisposeUnitOfWork(UnitOfWorkImplementor adapter)
        //{
        //    CurrentSession = null;
        //    UnitOfWork.DisposeUnitOfWork(adapter);
        //}
    }
}
