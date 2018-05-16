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
            // OWIN MVC SETUP:
            // Register the Autofac middleware FIRST, then the Autofac MVC middleware.
            app.UseAutofacMiddleware(AutofacConfiguration.Current);
            app.UseAutofacMvc();

            XmlConfigurator.Configure();
            ConfigureAuth(app);
            ConfigureData();
        }

        private static void ConfigureData()
        {
            //NHibernateSession.Initialize(System.Web.HttpContext.Current.ApplicationInstance);
            DataConfiguration.Configure();
        }
    }
}
