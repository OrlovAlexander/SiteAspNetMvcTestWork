using NHibernate;
using System.Collections.Generic;

namespace AbstractApplication.NHibernate
{
    public interface ISessionStorage
    {
        IEnumerable<ISession> GetAllSessions();

        ISession GetSessionForKey(string factoryKey);

        void SetSessionForKey(string factoryKey, ISession session);
    }
}
