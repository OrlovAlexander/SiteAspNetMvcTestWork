﻿using NHibernate;
using NHibernate.Cfg;
using System;
using System.IO;
using System.Xml;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private static ISession _currentSession;
        private ISessionFactory _sessionFactory;
        private Configuration _configuration;

        private const string Default_HibernateConfig = "hibernate.cfg.xml";

        internal UnitOfWorkFactory()
        { }

        public IUnitOfWork Create()
        {
            ISession session = CreateSession();
            session.FlushMode = FlushMode.Commit;
            _currentSession = session;
            return new UnitOfWorkImplementor(this, session);
        }

        public Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new Configuration();
                    string hibernateConfig = Default_HibernateConfig;
                    //if not rooted, assume path from base directory
                    if (Path.IsPathRooted(hibernateConfig) == false)
                        hibernateConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hibernateConfig);
                    if (File.Exists(hibernateConfig))
                        _configuration.Configure(new XmlTextReader(hibernateConfig));
                }
                return _configuration;
            }
        }

        public ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                    _sessionFactory = Configuration.BuildSessionFactory();
                return _sessionFactory;
            }
        }

        private ISession CreateSession()
        {
            return SessionFactory.OpenSession();
        }

        public ISession CurrentSession
        {
            get
            {
                if (_currentSession == null)
                    throw new InvalidOperationException("You are not in a unit of work.");
                return _currentSession;
            }
            set { _currentSession = value; }
        }

        public void DisposeUnitOfWork(UnitOfWorkImplementor adapter)
        {
            CurrentSession = null;
            UnitOfWork.DisposeUnitOfWork(adapter);
        }
    }
}
