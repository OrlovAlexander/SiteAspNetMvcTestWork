using AbstractApplication.Data.NHibernate.UnitOfWork;
using AbstractApplication.Domain;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System;
using System.Reflection;

namespace AbstractApplication.Tests
{
    [TestFixture]
    public class Domain_UnitOfWorkUsege_Tests
    {
        private IUnitOfWorkFactory _factory;

        [SetUp]
        public void SetupContext()
        {
            _factory = (IUnitOfWorkFactory)Activator.CreateInstance(typeof(UnitOfWorkFactory), true);
            // brute force attack to set my own factory via reflection
            var fieldInfo = typeof(UnitOfWork).GetField("_unitOfWorkFactory", BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, _factory);


            UnitOfWork.Configuration.AddAssembly(Assembly.GetExecutingAssembly());
            new SchemaExport(UnitOfWork.Configuration).Execute(false, true, false);
        }

        [Test]
        public void Can_add_a_new_instance_of_an_entity_to_the_database()
        {
            using (UnitOfWork.Start())
            {
                var person = new Person { Name = "John Doe", Birthdate = new DateTime(1915, 12, 15) };
                UnitOfWork.CurrentSession.Save(person);
                UnitOfWork.Current.TransactionalFlush();
            }
        }
    }
}
