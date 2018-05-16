using AbstractApplication.Data.NHibernate.UnitOfWork;

namespace ElmaTestWork_2.DAL.NHibernate.UnitOfWork
{
    public class AuthorityNHibernateProviderFactory : UnitOfWorkBase, IAuthorityNHibernateProviderFactory
    {
        public AuthorityNHibernateProviderFactory(IAuthorityUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory, "Authority")
        {
        }
    }
}