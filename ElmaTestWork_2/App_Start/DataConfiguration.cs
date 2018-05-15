using AbstractApplication.NHibernate;
using ElmaTestWork_2.Models;
using FluentNHibernate.Cfg;
using NHibernate.AspNet.Identity.Helpers;
using NHibernate.Tool.hbm2ddl;
using System;
using System.IO;

namespace ElmaTestWork_2.App_Start
{
    public class DataConfiguration
    {
        public static void Configure()
        {
            var internalTypes = new[] {
                typeof(ApplicationUser)
            };

            string path = Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "schema.sql");
            var mapping = MappingHelper.GetIdentityMappings(internalTypes);
            var configuration = Fluently.Configure(NHibernateSession.ConfigureNHibernate(null, null, mapping))
                //.ExposeConfiguration(cfg => new SchemaExport(cfg).SetOutputFile(path).Create(true, true))
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildConfiguration();
            NHibernateSession.BuildAuthenticationSession(configuration);

            path = Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "appSchema.sql");
            configuration = Fluently.Configure(NHibernateSession.ConfigureNHibernate(Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "AppHibernate.cfg.xml"), null))
                .Mappings(m => { m.FluentMappings.Add<DocumentMap>();
                    //m.HbmMappings.AddFromAssemblyOf<Document>();
                    })
                //.ExposeConfiguration(cfg => new SchemaExport(cfg).SetOutputFile(path).Create(true, true))
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildConfiguration();
            NHibernateSession.BuildElmaTestWorkSession(configuration);
        }
    }
}