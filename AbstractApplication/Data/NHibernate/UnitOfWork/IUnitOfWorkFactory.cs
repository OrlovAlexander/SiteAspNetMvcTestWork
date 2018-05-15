using NHibernate;
using NHibernate.Cfg;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
        Configuration Configuration { get; }
        ISessionFactory SessionFactory { get; }
        ISession CurrentSession { get; set; }
        void DisposeUnitOfWork(UnitOfWorkImplementor adapter);
    }
}
