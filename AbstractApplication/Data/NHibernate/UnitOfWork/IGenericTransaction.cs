using System;

namespace AbstractApplication.Data.NHibernate.UnitOfWork
{
    public interface IGenericTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
