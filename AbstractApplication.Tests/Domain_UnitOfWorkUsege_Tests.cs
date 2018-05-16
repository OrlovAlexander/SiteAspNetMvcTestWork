using AbstractApplication.Data.NHibernate.UnitOfWork;
using AbstractApplication.Domain;
using AbstractApplication.Tests.Model;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System;
using System.Reflection;

namespace AbstractApplication.Tests
{
    [TestFixture]
    public class Domain_UnitOfWorkUsege_Tests
    {
        UnitOfWorkFake _fake;

        [SetUp]
        public void SetupContext()
        {
            _fake = new UnitOfWorkFake(new UnitOfWorkFactoryFake(), "test");
        }

        [Test]
        public void Can_add_a_new_instance_of_an_entity_to_the_database()
        {
            using (_fake.Start())
            {
                var person = new Person { Name = "John Doe", Birthdate = new DateTime(1915, 12, 15) };
                _fake.Current.Session.Save(person);
                _fake.Current.TransactionalFlush();
            }
        }
    }
}
