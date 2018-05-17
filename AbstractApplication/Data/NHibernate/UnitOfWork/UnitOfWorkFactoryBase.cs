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
        protected INHibernateProviderFactory _nHibernateProviderFactory;
        protected ISessionFactory _sessionFactory;
        protected Configuration _configuration;

        public abstract void ConfigurationUp();

        public IUnitOfWork Create(INHibernateProviderFactory nHibernateProviderFactory)
        {
            Check.Require(_configuration != null, "Configuration is null.");
            Check.Require(nHibernateProviderFactory != null, "NHibernateProviderFactory is null.");

            _nHibernateProviderFactory = nHibernateProviderFactory;

            if (_sessionFactory == null)
                _sessionFactory = _configuration.BuildSessionFactory();

            ISession session = _sessionFactory.OpenSession();
            session.FlushMode = FlushMode.Commit;
            return new UnitOfWorkImplementor(this, session);
        }

        public void Dispose()
        {
            _nHibernateProviderFactory.Dispose();
        }

        //public void DisposeUnitOfWork(UnitOfWorkImplementor adapter)
        //{
        //    CurrentSession = null;
        //    UnitOfWork.DisposeUnitOfWork(adapter);
        //}
    }
}
