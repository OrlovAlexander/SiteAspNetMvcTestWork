using AbstractApplication.Data.NHibernate.UnitOfWork;
using ElmaTestWork_2.Models;
using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.IO;

namespace ElmaTestWork_2.DAL.NHibernate.UnitOfWork
{
    public class StoreUnitOfWorkFactory : UnitOfWorkFactoryBase, IStoreUnitOfWorkFactory
    {
        public override void ConfigurationUp()
        {
            //string path = Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "appSchema.sql");
            base._configuration = Fluently.Configure(ConfigureNHibernate(Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "AppHibernate.cfg.xml"), null))
                .Mappings(m =>
                {
                    m.FluentMappings.Add<DocumentMap>();
                    //m.HbmMappings.AddFromAssemblyOf<Document>();
                })
                //.ExposeConfiguration(cfg => new SchemaExport(cfg).SetOutputFile(path).Create(true, true))
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildConfiguration();
        }

        private Configuration ConfigureNHibernate(string cfgFile, IDictionary<string, string> cfgProperties)
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
    }
}