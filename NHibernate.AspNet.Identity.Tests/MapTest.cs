//using AbstractApplication.Data.NHibernate;
//using FluentNHibernate.Testing;
//using NHibernate.AspNet.Identity.Tests.Model;
//using NHibernate.AspNet.Identity.Tests.Models;
//using TestClass = NUnit.Framework.TestFixtureAttribute;
//using TestCleanup = NUnit.Framework.TearDownAttribute;
//using TestInitialize = NUnit.Framework.SetUpAttribute;
//using TestMethod = NUnit.Framework.TestAttribute;

namespace NHibernate.AspNet.Identity.Tests
{
    //[TestClass]
    //public class MapTest
    //{
    //    INHibernateProviderFactory _store;

    //    [TestInitialize]
    //    public void Initialize()
    //    {
    //        _store = new UnitOfWorkFake(new UnitOfWorkFactoryFake(), "test");
    //        _store.Configuration();
    //    }

    //    [TestCleanup]
    //    public void Cleanup()
    //    {
    //        _store.Dispose();
    //        _store = null;
    //    }

    //    [TestMethod]
    //    public void CanCorrectlyMapFoo()
    //    {
    //        using (_store.Start())
    //        {
    //            new PersistenceSpecification<Foo>(_store.Current.Session)
    //                .CheckProperty(c => c.String, "Foo")
    //                .CheckReference(r => r.User, new ApplicationUser { UserName = "FooUser" })
    //                .VerifyTheMappings();
    //        }
    //    }

    //    [TestMethod]
    //    public void CanCorrectlyMapIdentityUser()
    //    {
    //        using (_store.Start())
    //        {
    //            new PersistenceSpecification<IdentityUser>(_store.Current.Session)
    //                .CheckProperty(c => c.UserName, "Lukz")
    //                .VerifyTheMappings();
    //        }
    //    }

    //    [TestMethod]
    //    public void CanCorrectlyMapApplicationUser()
    //    {
    //        using (_store.Start())
    //        {
    //            new PersistenceSpecification<ApplicationUser>(_store.Current.Session)
    //                .CheckProperty(c => c.UserName, "Lukz")
    //                .VerifyTheMappings(); 
    //        }
    //    }

    //    [TestMethod]
    //    public void CanCorrectlyMapCascadeLogins()
    //    {
    //        using (_store.Start())
    //        {
    //            new PersistenceSpecification<IdentityUser>(_store.Current.Session)
    //                .CheckProperty(c => c.UserName, "Letícia")
    //                .CheckComponentList(c => c.Logins, new[] { new IdentityUserLogin { LoginProvider = "Provider", ProviderKey = "Key" } })
    //                //.CheckList(l => l.Logins, new[] { new IdentityUserLogin { LoginProvider = "Provider", ProviderKey = "Key" } })
    //                //.CheckList(l => l.Logins, new[] { new IdentityUserLogin { LoginProvider = "Provider", ProviderKey = "Key" } }, (user, login) => user.Logins.Add(login))
    //                .VerifyTheMappings(); 
    //        }
    //    }
    //}
}
