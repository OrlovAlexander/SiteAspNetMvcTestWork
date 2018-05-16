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
            base.Configuration = new Configuration();
            string hibernateConfig = Default_HibernateConfig;
            //if not rooted, assume path from base directory
            if (Path.IsPathRooted(hibernateConfig) == false)
                hibernateConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hibernateConfig);
            if (File.Exists(hibernateConfig))
                base.Configuration.Configure(new XmlTextReader(hibernateConfig));

            base.Configuration.AddAssembly(Assembly.GetExecutingAssembly());
            new SchemaExport(base.Configuration).Execute(false, true, false);
        }
    }
}
