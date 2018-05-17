using AbstractApplication.Data.NHibernate.UnitOfWork;
using AbstractApplication.Domain;
using NHibernate.AspNet.Identity.Tests.Models;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System;
using System.IO;
using System.Linq;

namespace NHibernate.AspNet.Identity.Tests.Model
{
    internal class UnitOfWorkFactoryFake : UnitOfWorkFactoryBase
    {
        private const string Default_HibernateConfig = "mssql-nhibernate-config.xml";

        public override void ConfigurationUp()
        {
            var baseEntityToIgnore = new[] {
                typeof(EntityWithTypedId<int>),
                typeof(EntityWithTypedId<string>),
            };

            var allEntities = new[] {
                typeof(IdentityUser),
                typeof(ApplicationUser),
                typeof(IdentityRole),
                typeof(IdentityUserLogin),
                typeof(IdentityUserClaim),
                typeof(Foo),
            };

            var mapper = new ConventionModelMapper();
            DefineBaseClass(mapper, baseEntityToIgnore);
            mapper.IsComponent((type, declared) => typeof(ValueObject).IsAssignableFrom(type));

            mapper.AddMapping<IdentityUserMap>();
            mapper.AddMapping<IdentityRoleMap>();
            mapper.AddMapping<IdentityUserClaimMap>();

            var mapping = mapper.CompileMappingForEach(allEntities);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Default_HibernateConfig);
            _configuration = new Configuration();
            _configuration.Configure(path);
            foreach (var map in mapping)
            {
                Console.WriteLine(map.AsString());
                _configuration.AddDeserializedMapping(map, null);
            }
            BuildSchema();
        }

        private static void DefineBaseClass(ConventionModelMapper mapper, System.Type[] baseEntityToIgnore)
        {
            if (baseEntityToIgnore == null) return;
            mapper.IsEntity((type, declared) =>
                baseEntityToIgnore.Any(x => x.IsAssignableFrom(type)) &&
                !baseEntityToIgnore.Any(x => x == type) &&
                !type.IsInterface);
            mapper.IsRootEntity((type, declared) => baseEntityToIgnore.Any(x => x == type.BaseType));
        }

        public void BuildSchema()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"schema.sql");

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(_configuration)
                .SetOutputFile(path)
                .Create(true, true /* DROP AND CREATE SCHEMA */);
        }

    }
}
