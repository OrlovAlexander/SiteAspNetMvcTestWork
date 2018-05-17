using System;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public interface IUnitOfWorkFactory : IDisposable
    {
        IUnitOfWork Create(INHibernateProviderFactory nHibernateProviderFactory);
        void ConfigurationUp();
        //ISessionFactory SessionFactory { get; }
        //ISession CurrentSession { get; set; }
        //void DisposeUnitOfWork(UnitOfWorkImplementor adapter);
    }
}
