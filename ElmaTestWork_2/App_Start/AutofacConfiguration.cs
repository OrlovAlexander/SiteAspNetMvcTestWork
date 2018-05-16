using Autofac;
using Autofac.Integration.Mvc;
using ElmaTestWork_2.DAL.NHibernate.UnitOfWork;
using System.Web.Mvc;

namespace ElmaTestWork_2.App_Start
{
    public static class AutofacConfiguration
    {
        private static IContainer _currentContainer;

        static AutofacConfiguration()
        {
            _currentContainer = GetBuilder()
                .AutofacConfigure()
                .Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_currentContainer));
        }

        public static IContainer Current => _currentContainer;

        private static ContainerBuilder AutofacConfigure(this ContainerBuilder builder)
        {
            builder.RegisterType<AuthorityUnitOfWorkFactory>().As<IAuthorityUnitOfWorkFactory>().InstancePerRequest();
            builder.RegisterType<AuthorityNHibernateProviderFactory>().As<IAuthorityNHibernateProviderFactory>().InstancePerRequest();
            builder.RegisterType<StoreUnitOfWorkFactory>().As<IStoreUnitOfWorkFactory>().InstancePerRequest();
            builder.RegisterType<StoreNHibernateProviderFactory>().As<IStoreNHibernateProviderFactory>().InstancePerRequest();
            return builder;
        }

        private static ContainerBuilder GetBuilder()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            return builder;
        }
    }
}