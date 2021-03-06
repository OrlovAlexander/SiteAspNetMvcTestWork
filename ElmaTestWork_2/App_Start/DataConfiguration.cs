﻿using Autofac;
using ElmaTestWork_2.DAL.NHibernate.UnitOfWork;

namespace ElmaTestWork_2.App_Start
{
    public class DataConfiguration
    {
        public static void Configure()
        {
            var authority = AutofacConfiguration.Current.Resolve<IAuthorityNHibernateProviderFactory>();
            authority.Configuration();

            var store = AutofacConfiguration.Current.Resolve<IStoreNHibernateProviderFactory>();
            store.Configuration();

            //var internalTypes = new[] {
            //    typeof(ApplicationUser)
            //};

            //string path = Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "schema.sql");
            //var mapping = MappingHelper.GetIdentityMappings(internalTypes);
            //var configuration = Fluently.Configure(NHibernateSession.ConfigureNHibernate(null, null, mapping))
            //    //.ExposeConfiguration(cfg => new SchemaExport(cfg).SetOutputFile(path).Create(true, true))
            //    .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
            //    .BuildConfiguration();
            //NHibernateSession.BuildAuthenticationSession(configuration);

            //path = Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "appSchema.sql");
            //configuration = Fluently.Configure(NHibernateSession.ConfigureNHibernate(Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "AppHibernate.cfg.xml"), null))
            //    .Mappings(m => { m.FluentMappings.Add<DocumentMap>();
            //        //m.HbmMappings.AddFromAssemblyOf<Document>();
            //        })
            //    //.ExposeConfiguration(cfg => new SchemaExport(cfg).SetOutputFile(path).Create(true, true))
            //    .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
            //    .BuildConfiguration();
            //NHibernateSession.BuildElmaTestWorkSession(configuration);
        }
    }
}