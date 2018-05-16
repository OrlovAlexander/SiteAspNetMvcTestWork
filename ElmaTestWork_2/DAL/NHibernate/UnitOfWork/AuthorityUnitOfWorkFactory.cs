using AbstractApplication.Data.NHibernate.UnitOfWork;
using ElmaTestWork_2.Models;
using FluentNHibernate.Cfg;
using NHibernate.AspNet.Identity.Helpers;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Tool.hbm2ddl;

namespace ElmaTestWork_2.DAL.NHibernate.UnitOfWork
{
    public class AuthorityUnitOfWorkFactory : UnitOfWorkFactoryBase, IAuthorityUnitOfWorkFactory
    {
        public override void ConfigurationUp()
        {
            var internalTypes = new[] {
                typeof(ApplicationUser)
            };

            //string path = Path.Combine(AppDomain.CurrentDomain.GetData("APPBASE").ToString(), "schema.sql");
            var mapping = MappingHelper.GetIdentityMappings(internalTypes);
            base.Configuration = Fluently.Configure(ConfigureNHibernate(mapping))
                //.ExposeConfiguration(cfg => new SchemaExport(cfg).SetOutputFile(path).Create(true, true))
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildConfiguration();
        }

        private Configuration ConfigureNHibernate(params HbmMapping[] mapping)
        {
            Configuration config = new Configuration();
            config.Configure();
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
    }
}