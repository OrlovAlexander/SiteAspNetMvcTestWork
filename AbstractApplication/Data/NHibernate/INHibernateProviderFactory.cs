using AbstractApplication.Data.NHibernate.UnitOfWork;
using System;

namespace AbstractApplication.Data.NHibernate
{
    /// <summary>
    /// Интерфейс абстрактной фабрики
    /// </summary>
    public interface INHibernateProviderFactory : IDisposable
    {
        void Configuration();
        IUnitOfWork Start();
        IUnitOfWork Current { get; }
        bool IsStarted { get; }
        //ISession CurrentSession { get; }
    }
}
