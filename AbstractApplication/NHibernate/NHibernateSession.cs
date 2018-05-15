using AbstractApplication.DesignByContract;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using System.Collections.Generic;
using System.Web;

namespace AbstractApplication.NHibernate
{
    public static class NHibernateSession
    {
        private static readonly string AuthenticationSession = "AuthenticationSession";
        private static readonly string ElmaTestWorkSession = "ElmaTestWorkSession";

        private static Dictionary<string, ISessionFactory> SessionFactories;
        private static ISessionStorage Storage;

        public static void Initialize(HttpApplication app)
        {
            SessionFactories = new Dictionary<string, ISessionFactory>();
            Storage = new WebSessionStorage(app);
        }

        public static void CloseAllSessions()
        {
            foreach (var session in Storage.GetAllSessions())
            {
                if (session.IsOpen)
                {
                    session.Close();
                }
            }
        }

        public static ISession GetSessionForAuthentication()
        {
            Check.Require(Storage != null, "An ISessionStorage has not been configured");
            Check.Require(
                SessionFactories.ContainsKey(AuthenticationSession),
                "An ISessionFactory does not exist with a factory key of " + AuthenticationSession);

            var session = Storage.GetSessionForKey(AuthenticationSession);

            if (session == null)
            {
                session = SessionFactories[AuthenticationSession].OpenSession();
                Storage.SetSessionForKey(AuthenticationSession, session);
            }

            return session;
        }

        public static ISession GetSessionForElmaTestWork()
        {
            Check.Require(Storage != null, "An ISessionStorage has not been configured");
            Check.Require(
                SessionFactories.ContainsKey(ElmaTestWorkSession),
                "An ISessionFactory does not exist with a factory key of " + ElmaTestWorkSession);

            var session = Storage.GetSessionForKey(ElmaTestWorkSession);

            if (session == null)
            {
                session = SessionFactories[ElmaTestWorkSession].OpenSession();
                Storage.SetSessionForKey(ElmaTestWorkSession, session);
            }

            return session;
        }

        public static Configuration ConfigureNHibernate(string cfgFile, IDictionary<string, string> cfgProperties)
        {
            var cfg = new Configuration();

            if (cfgProperties != null)
            {
                cfg.AddProperties(cfgProperties);
            }

            if (string.IsNullOrEmpty(cfgFile) == false)
            {
                return cfg.Configure(cfgFile);
            }

            return cfg.Configure();
        }

        public static Configuration ConfigureNHibernate(string cfgFile, IDictionary<string, string> cfgProperties, params HbmMapping[] mapping)
        {
            Configuration config = ConfigureNHibernate(cfgFile, cfgProperties);
            try
            {
                foreach (var map in mapping)
                    config.AddDeserializedMapping(map, null);

                return config;
            }
            catch
            {
                // If this NHibernate config throws an exception, null the Storage reference so 
                // the config can be corrected without having to restart the web application.
                throw;
            }
        }

        public static void BuildAuthenticationSession(Configuration configuration)
        {
            SessionFactories[AuthenticationSession] = configuration.BuildSessionFactory();
        }

        public static void BuildElmaTestWorkSession(Configuration configuration)
        {
            SessionFactories[ElmaTestWorkSession] = configuration.BuildSessionFactory();
        }

        public static ISessionFactory Authentication => SessionFactories[AuthenticationSession];

        public static ISessionFactory ElmaTestWork => SessionFactories[ElmaTestWorkSession];

    }
}
