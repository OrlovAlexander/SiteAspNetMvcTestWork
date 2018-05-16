using AbstractApplication.Data.NHibernate.UnitOfWork;
using NUnit.Framework;
using System;

namespace AbstractApplication.Tests
{
    [TestFixture]
    public class Domain_UnitOfWorkFactoryFixture_Tests
    {
        private IUnitOfWorkFactory _factory;

        [SetUp]
        public void SetupContext()
        {
            _factory = (IUnitOfWorkFactory)Activator.CreateInstance(typeof(UnitOfWorkFactoryBase), true);
        }

        [Test]
        public void Can_create_unit_of_work()
        {
            IUnitOfWork implementor = _factory.Create();
            Assert.IsNotNull(implementor);
            //Assert.IsNotNull(_factory.CurrentSession);
            //Assert.AreEqual(FlushMode.Commit, _factory.CurrentSession.FlushMode);
        }

        [Test]
        public void Can_configure_NHibernate()
        {
            Assert.DoesNotThrow(() => { _factory.ConfigurationUp(); });
            //var configuration = _factory.ConfigurationUp();
            //Assert.IsNotNull(configuration);
            //Assert.AreEqual("NHibernate.Connection.DriverConnectionProvider",
            //                configuration.Properties["connection.provider"]);
            //Assert.AreEqual("NHibernate.Dialect.MsSql2008Dialect",
            //                configuration.Properties["dialect"]);
            //Assert.AreEqual("NHibernate.Driver.SqlClientDriver",
            //                configuration.Properties["connection.driver_class"]);
            //Assert.AreEqual("Server=(local);Database=Test;Integrated Security=SSPI;",
            //                configuration.Properties["connection.connection_string"]);
        }

        //[Test]
        //public void Can_create_and_access_session_factory()
        //{
        //    ISessionFactory sessionFactory = _factory.SessionFactory;
        //    Assert.IsNotNull(sessionFactory);
        //    //Assert.AreEqual("NHibernate.Dialect.MsSql2008Dialect", sessionFactory.Dialect.ToString());
        //}

        //[Test]
        //public void Accessing_CurrentSession_when_no_session_open_throws()
        //{
        //    try
        //    {
        //        var session = _factory.CurrentSession;
        //    }
        //    catch (InvalidOperationException)
        //    { }
        //}
    }
}
