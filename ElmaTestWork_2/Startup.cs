using AbstractApplication.NHibernate;
using ElmaTestWork_2.App_Start;
using log4net.Config;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ElmaTestWork_2.Startup))]
namespace ElmaTestWork_2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            XmlConfigurator.Configure();
            ConfigureAuth(app);
            ConfigureData();
        }

        private static void ConfigureData()
        {
            NHibernateSession.Initialize(System.Web.HttpContext.Current.ApplicationInstance);
            DataConfiguration.Configure();
        }
    }
}
