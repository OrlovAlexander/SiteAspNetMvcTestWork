using AbstractApplication.Data.NHibernate;
using AbstractApplication.Data.NHibernate.UnitOfWork;

namespace ElmaTestWork_2.DAL.NHibernate.UnitOfWork
{
    public class StoreNHibernateProviderFactory : UnitOfWorkBase, IStoreNHibernateProviderFactory
    {
        public StoreNHibernateProviderFactory(IStoreUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory, "Store")
        {
        }
    }
}