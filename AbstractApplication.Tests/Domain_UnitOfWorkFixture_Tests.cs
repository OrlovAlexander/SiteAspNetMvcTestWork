using AbstractApplication.Data.NHibernate.UnitOfWork;
using AbstractApplication.Tests.Model;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using System;

namespace AbstractApplication.Tests
{
    [TestFixture]
    public class Domain_UnitOfWorkFixture_Tests
    {
        private readonly MockRepository _mocks = new MockRepository();
        private IUnitOfWorkFactory _factory;
        private IUnitOfWork _unitOfWork;
        private ISession _session;
        private UnitOfWorkFake _fake;

        [SetUp]
        public void SetupContext()
        {
            _factory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            _session = _mocks.DynamicMock<ISession>();

            _fake = new UnitOfWorkFake(new UnitOfWorkFactoryFake(), "test");

            _mocks.BackToRecordAll();
            SetupResult.For(_factory.Create()).Return(_unitOfWork);
            _mocks.ReplayAll();
        }

        [TearDown]
        public void TearDownContext()
        {
            _mocks.VerifyAll();
        }

        [Test]
        public void Can_Start_UnitOfWork()
        {
            using (_mocks.Record())
            {
                Expect.Call(_factory.Create()).Return(_unitOfWork);
            }
            using (_mocks.Playback())
            {
                var uow = _fake.Start();
            }
        }

        [Test]
        public void Starting_UnitOfWork_if_already_started_throws()
        {
            _fake.Start();
            try
            {
                _fake.Start();
            }
            catch (InvalidOperationException)
            { }
        }

        [Test]
        public void Can_access_current_unit_of_work()
        {
            IUnitOfWork uow = _fake.Start();
            var current = _fake.Current;
            Assert.AreSame(uow, current);
        }

        [Test]
        public void Accessing_Current_UnitOfWork_if_not_started_throws()
        {
            try
            {
                var current = _fake.Current;
            }
            catch (InvalidOperationException)
            { }
        }

        [Test]
        public void Can_test_if_UnitOfWork_Is_Started()
        {
            Assert.IsFalse(_fake.IsStarted);

            IUnitOfWork uow = _fake.Start();
            Assert.IsTrue(_fake.IsStarted);
        }

        //[Test]
        //public void Can_get_valid_current_session_if_UoW_is_started()
        //{
        //    using (_fake.Start())
        //    {
        //        ISession session = _fake.CurrentSession;
        //        Assert.IsNotNull(session);
        //    }
        //}
    }
}
