using AbstractApplication.Data.NHibernate.UnitOfWork;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Reflection;

namespace AbstractApplication.Tests
{
    [TestFixture]
    public class Domain_UnitOfWorkFixture_Tests
    {
        private readonly MockRepository _mocks = new MockRepository();
        private IUnitOfWorkFactory _factory;
        private IUnitOfWork _unitOfWork;
        private ISession _session;

        [SetUp]
        public void SetupContext()
        {
            _factory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            _session = _mocks.DynamicMock<ISession>();

            // brute force attack to set my own factory via reflection
            var fieldInfo = typeof(UnitOfWork).GetField("_unitOfWorkFactory", BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, _factory);

            _mocks.BackToRecordAll();
            SetupResult.For(_factory.Create()).Return(_unitOfWork);
            SetupResult.For(_factory.CurrentSession).Return(_session);
            _mocks.ReplayAll();
        }

        [TearDown]
        public void TearDownContext()
        {
            _mocks.VerifyAll();

            // assert that the UnitOfWork is reset
            var propertyInfo = typeof(UnitOfWork).GetProperty("CurrentUnitOfWork", BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic);
            propertyInfo.SetValue(null, null, null);
        }

        [Test]
        public void Can_Start_UnitOfWork()
        {
            var factory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();

            // brute force attack to set my own factory via reflection
            var fieldInfo = typeof(UnitOfWork).GetField("_unitOfWorkFactory", BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, factory);

            using (_mocks.Record())
            {
                Expect.Call(factory.Create()).Return(unitOfWork);
            }
            using (_mocks.Playback())
            {
                var uow = UnitOfWork.Start();
            }
        }

        [Test]
        public void Starting_UnitOfWork_if_already_started_throws()
        {
            UnitOfWork.Start();
            try
            {
                UnitOfWork.Start();
            }
            catch (InvalidOperationException)
            { }
        }

        [Test]
        public void Can_access_current_unit_of_work()
        {
            IUnitOfWork uow = UnitOfWork.Start();
            var current = UnitOfWork.Current;
            Assert.AreSame(uow, current);
        }

        [Test]
        public void Accessing_Current_UnitOfWork_if_not_started_throws()
        {
            try
            {
                var current = UnitOfWork.Current;
            }
            catch (InvalidOperationException)
            { }
        }

        [Test]
        public void Can_test_if_UnitOfWork_Is_Started()
        {
            Assert.IsFalse(UnitOfWork.IsStarted);

            IUnitOfWork uow = UnitOfWork.Start();
            Assert.IsTrue(UnitOfWork.IsStarted);
        }

        [Test]
        public void Can_get_valid_current_session_if_UoW_is_started()
        {
            using (UnitOfWork.Start())
            {
                ISession session = UnitOfWork.CurrentSession;
                Assert.IsNotNull(session);
            }
        }
    }
}
