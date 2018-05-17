using AbstractApplication.Data.NHibernate.UnitOfWork;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace AbstractApplication.Tests.Model
{
    internal class UnitOfWorkFactoryFake : UnitOfWorkFactoryBase
    {
        private const string Default_HibernateConfig = "hibernate.cfg.xml";

        public override void ConfigurationUp()
        {
            base._configuration = new Configuration();
            string hibernateConfig = Default_HibernateConfig;
            //if not rooted, assume path from base directory
            if (Path.IsPathRooted(hibernateConfig) == false)
                hibernateConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hibernateConfig);
            if (File.Exists(hibernateConfig))
                base._configuration.Configure(new XmlTextReader(hibernateConfig));

            base._configuration.AddAssembly(Assembly.GetExecutingAssembly());
            new SchemaExport(base._configuration).Execute(false, true, false);
        }
    }
}
