using AbstractApplication.Data.NHibernate;
using AbstractApplication.Data.NHibernate.UnitOfWork;

namespace NHibernate.AspNet.Identity.Tests.Model
{
    internal class UnitOfWorkFake : UnitOfWorkBase, INHibernateProviderFactory
    {
        public UnitOfWorkFake(IUnitOfWorkFactory unitOfWorkFactory, string unitOfWorkKey)
            : base(unitOfWorkFactory, unitOfWorkKey)
        {
        }
    }
}
