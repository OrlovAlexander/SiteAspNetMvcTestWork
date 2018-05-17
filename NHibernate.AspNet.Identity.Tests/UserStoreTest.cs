﻿using AbstractApplication.Data.NHibernate;
using Microsoft.AspNet.Identity;
using NHibernate.AspNet.Identity.Tests.Model;
using NHibernate.AspNet.Identity.Tests.Models;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace NHibernate.AspNet.Identity.Tests
{
    [TestClass]
    public class UserStoreTest
    {
        INHibernateProviderFactory _store;

        [TestInitialize]
        public void Initialize()
        {
            _store = new UnitOfWorkFake(new UnitOfWorkFactoryFake(), "test");
            _store.Configuration();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _store.Dispose();
            _store = null;
        }

        [TestMethod]
        public void WhenHaveNoUser()
        {
            var login = new UserLoginInfo("ProviderTest", "ProviderKey");
            var store = new UserStore<IdentityUser>(_store);
            var user = store.FindAsync(login).Result;

            Assert.IsNull(user);
        }

        [TestMethod]
        public void WhenAddLoginAsync()
        {
            var user = new IdentityUser("Lukz");
            var login = new UserLoginInfo("ProviderTest02", "ProviderKey02");
            var store = new UserStore<IdentityUser>(_store);
            using (var ts = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                var result = store.AddLoginAsync(user, login);
                ts.Complete();
                Assert.IsNull(result.Exception);
            }

            IdentityUser actual;
            using (_store.Start())
            {
                actual = _store.Current.Session.Query<IdentityUser>().FirstOrDefault(x => x.UserName == user.UserName);
            }
            var userStored = store.FindAsync(login).Result;

            Assert.IsNotNull(actual);
            Assert.AreEqual(user.UserName, actual.UserName);
            Assert.AreEqual(user.UserName, userStored.UserName);
        }

        [TestMethod]
        public void WhenRemoveLoginAsync()
        {
            var user = new IdentityUser("Lukz 03");
            var login = new UserLoginInfo("ProviderTest03", "ProviderKey03");
            var store = new UserStore<IdentityUser>(_store);
            store.AddLoginAsync(user, login);

            Assert.IsTrue(user.Logins.Any());

            using (var ts = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                var result = store.RemoveLoginAsync(user, login);
                ts.Complete();
                Assert.IsNull(result.Exception);
            }

            IdentityUser actual;
            using (_store.Start())
            {
                actual = _store.Current.Session.Query<IdentityUser>().FirstOrDefault(x => x.UserName == user.UserName);
                Assert.IsFalse(actual.Logins.Any());
            }
        }

        [TestMethod]
        public void WhenCreateUserAsync()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_store));
            var user = new ApplicationUser() { UserName = "RealUserName" };

            using (var transaction = new TransactionScope())
            {
                IdentityResult result;
                using (_store.Start())
                {
                    result = userManager.CreateAsync(user, "RealPassword").GetAwaiter().GetResult();
                }
                transaction.Complete();
                Assert.AreEqual(0, result.Errors.Count());
            }

            ApplicationUser actual;
            using (_store.Start())
            {
                actual = _store.Current.Session.Query<ApplicationUser>().FirstOrDefault(x => x.UserName == user.UserName);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(user.UserName, actual.UserName);
        }

        [TestMethod]
        public void GivenHaveRoles_WhenDeleteUser_ThenDeletingCausesNoCascade()
        {
            var user = new IdentityUser("Lukz 04");
            var role = new IdentityRole("ADM");
            var store = new UserStore<IdentityUser>(_store);
            var roleStore = new RoleStore<IdentityRole>(_store);

            roleStore.CreateAsync(role);
            store.CreateAsync(user);
            store.AddToRoleAsync(user, "ADM");

            using (_store.Start())
            {
                Assert.IsTrue(_store.Current.Session.Query<IdentityRole>().Any(x => x.Name == "ADM"));
                Assert.IsTrue(_store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Lukz 04"));
            }

            var result = store.DeleteAsync(user);

            Assert.IsNull(result.Exception);
            using (_store.Start())
            {
                Assert.IsFalse(this._store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Lukz 04"));
                Assert.IsTrue(this._store.Current.Session.Query<IdentityRole>().Any(x => x.Name == "ADM"));
            }
        }

        [TestMethod]
        public void WhenRemoveUserFromRole_ThenDoNotDeleteRole_BugFix()
        {
            var user = new IdentityUser("Lukz 05");
            var role = new IdentityRole("ADM05");
            var store = new UserStore<IdentityUser>(_store);
            var roleStore = new RoleStore<IdentityRole>(_store);

            roleStore.CreateAsync(role);
            store.CreateAsync(user);
            store.AddToRoleAsync(user, "ADM05");

            using (_store.Start())
            {
                Assert.IsTrue(_store.Current.Session.Query<IdentityRole>().Any(x => x.Name == "ADM05"));
                Assert.IsTrue(_store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Lukz 05"));
                Assert.IsTrue(store.IsInRoleAsync(user, "ADM05").Result);

                var result = store.RemoveFromRoleAsync(user, "ADM05");

                Assert.IsNull(result.Exception);
                Assert.IsFalse(store.IsInRoleAsync(user, "ADM05").Result);
                Assert.IsTrue(_store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Lukz 05"));
                Assert.IsTrue(_store.Current.Session.Query<IdentityRole>().Any(x => x.Name == "ADM05"));
            }
        }

        [TestMethod]
        public void GetAllUsers()
        {
            var user1 = new IdentityUser("Lukz 04");
            var user2 = new IdentityUser("Moa 01");
            var user3 = new IdentityUser("Win 02");
            var user4 = new IdentityUser("Andre 03");
            var role = new IdentityRole("ADM");
            var store = new UserStore<IdentityUser>(this._store);
            var roleStore = new RoleStore<IdentityRole>(this._store);

            roleStore.CreateAsync(role);
            store.CreateAsync(user1);
            store.CreateAsync(user2);
            store.CreateAsync(user3);
            store.CreateAsync(user4);
            store.AddToRoleAsync(user1, "ADM");
            store.AddToRoleAsync(user2, "ADM");
            store.AddToRoleAsync(user3, "ADM");
            store.AddToRoleAsync(user4, "ADM");

            using (_store.Start())
            {
                Assert.IsTrue(this._store.Current.Session.Query<IdentityRole>().Any(x => x.Name == "ADM"));
                Assert.IsTrue(this._store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Lukz 04"));

                Assert.IsTrue(this._store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Andre 03"));
            }

            var resul = store.Users;

            Assert.AreEqual(4, resul.Count());
        }

        [TestMethod]
        public void GetAllRoles()
        {
            var user1 = new IdentityUser("Lukz 04");
            var user2 = new IdentityUser("Moa 01");
            var user3 = new IdentityUser("Win 02");
            var user4 = new IdentityUser("Andre 03");
            var role = new IdentityRole("ADM");
            var role2 = new IdentityRole("USR");
            var store = new UserStore<IdentityUser>(this._store);
            var roleStore = new RoleStore<IdentityRole>(this._store);

            roleStore.CreateAsync(role);
            roleStore.CreateAsync(role2);
            store.CreateAsync(user1);
            store.CreateAsync(user2);
            store.CreateAsync(user3);
            store.CreateAsync(user4);
            store.AddToRoleAsync(user1, "ADM");
            store.AddToRoleAsync(user2, "ADM");
            store.AddToRoleAsync(user3, "ADM");
            store.AddToRoleAsync(user4, "ADM");
            store.AddToRoleAsync(user1, "USR");
            store.AddToRoleAsync(user4, "USR");

            using (_store.Start())
            {
                Assert.IsTrue(this._store.Current.Session.Query<IdentityRole>().Any(x => x.Name == "ADM"));
                Assert.IsTrue(this._store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Lukz 04"));

                Assert.IsTrue(this._store.Current.Session.Query<IdentityUser>().Any(x => x.UserName == "Andre 03"));
            }

            var resul = roleStore.Roles;

            Assert.AreEqual(2, resul.Count());
        }

        [TestMethod]
        public void LockoutAccount()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
            userManager.MaxFailedAccessAttemptsBeforeLockout = 3;
            userManager.UserLockoutEnabledByDefault = true;
            userManager.DefaultAccountLockoutTimeSpan = new TimeSpan(0, 10, 0);
            userManager.Create(new ApplicationUser() { UserName = "test", LockoutEnabled = true }, "Welcome");
            var user = userManager.Find("test", "Welcome");
            Assert.AreEqual(0, userManager.GetAccessFailedCount(user.Id));
            userManager.AccessFailed(user.Id);
            Assert.AreEqual(1, userManager.GetAccessFailedCount(user.Id));
            userManager.AccessFailed(user.Id);
            Assert.AreEqual(2, userManager.GetAccessFailedCount(user.Id));
            userManager.AccessFailed(user.Id);
            Assert.IsTrue(userManager.IsLockedOut(user.Id));
        }

        [TestMethod]
        public void FindByName()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
            userManager.Create(new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = true }, "Welcome");
            var x = userManager.FindByName("tEsT");
            Assert.IsNotNull(x);
            Assert.IsTrue(userManager.IsEmailConfirmed(x.Id));
        }

        [TestMethod]
        public void FindByNameWithRoles()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(this._store));
            roleManager.Create(new IdentityRole("Admin"));
            roleManager.Create(new IdentityRole("AO"));
            var user = new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = true };
            userManager.Create(user, "Welcome");
            userManager.AddToRole(user.Id, "Admin");
            userManager.AddToRole(user.Id, "AO");

            var x = userManager.FindByName("tEsT");
            Assert.IsNotNull(x);
            Assert.AreEqual(2, x.Roles.Count);
        }

        [TestMethod]
        public void FindByEmail()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
            userManager.Create(new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = true }, "Welcome");
            var x = userManager.FindByEmail("AaA@bBb.com");
            Assert.IsNotNull(x);
            Assert.IsTrue(userManager.IsEmailConfirmed(x.Id));
        }

        [TestMethod]
        public void AddClaim()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
            var user = new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = true };
            userManager.Create(user, "Welcome");
            userManager.AddClaim(user.Id, new Claim(ClaimTypes.Role, "Admin"));
            Assert.AreEqual(1, userManager.GetClaims(user.Id).Count());
        }

        [TestMethod]
        public void EmailConfirmationToken()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
            userManager.UserTokenProvider = new EmailTokenProvider<ApplicationUser, string>() { BodyFormat = "xxxx {0}", Subject = "Reset password" };
            userManager.Create(new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = false }, "Welcome");
            var x = userManager.FindByEmail("aaa@bbb.com");
            string token = userManager.GeneratePasswordResetToken(x.Id);
            userManager.ResetPassword(x.Id, token, "Welcome!");
        }

        [TestMethod]
        public void FindByEmailAggregated()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(this._store));
            userManager.Create(new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = true }, "Welcome");
            var x = userManager.FindByEmail("aaa@bbb.com");
            roleManager.CreateAsync(new IdentityRole("Admin"));
            userManager.AddClaim(x.Id, new Claim("role", "admin"));
            userManager.AddClaim(x.Id, new Claim("role", "user"));
            userManager.AddToRole(x.Id, "Admin");
            userManager.AddLogin(x.Id, new UserLoginInfo("facebook", "1234"));
            x = userManager.FindByEmail("aaa@bbb.com");
            Assert.IsNotNull(x);
            Assert.AreEqual(2, x.Claims.Count);
            Assert.AreEqual(1, x.Roles.Count);
            Assert.AreEqual(1, x.Logins.Count);
        }

        //[TestMethod]
        //public void CreateWithoutCommitingTransactionScopeShouldNotInsertRows()
        //{
        //    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
        //    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(this._store));
        //    using (var ts = new TransactionScope(TransactionScopeOption.RequiresNew))
        //    {
        //        using (_store.Start())
        //        {
        //            // session is not opened inside the scope so we need to enlist it manually
        //            ((System.Data.Common.DbConnection)_store.Current.Session.Connection).EnlistTransaction(System.Transactions.Transaction.Current);
        //            userManager.Create(new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = true }, "Welcome1");
        //            var x = userManager.FindByEmail("aaa@bbb.com");
        //            roleManager.Create(new IdentityRole("Admin"));
        //            userManager.AddClaim(x.Id, new Claim("role", "admin"));
        //            userManager.AddClaim(x.Id, new Claim("role", "user"));
        //            userManager.AddToRole(x.Id, "Admin");
        //            userManager.AddLogin(x.Id, new UserLoginInfo("facebook", "1234")); 
        //        }
        //    }
        //    var x2 = userManager.FindByEmail("aaa@bbb.com");
        //    Assert.IsNull(x2);
        //}

        //[TestMethod]
        //public void CreateWithoutCommitingNHibernateTransactionShouldNotInsertRows()
        //{
        //    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._store));
        //    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(this._store));
        //    using (var ts = _store.Current.Session.BeginTransaction())
        //    {
        //        userManager.Create(new ApplicationUser() { UserName = "test", Email = "aaa@bbb.com", EmailConfirmed = true }, "Welcome1");
        //        var x = userManager.FindByEmail("aaa@bbb.com");
        //        roleManager.Create(new IdentityRole("Admin"));
        //        userManager.AddClaim(x.Id, new Claim("role", "admin"));
        //        userManager.AddClaim(x.Id, new Claim("role", "user"));
        //        userManager.AddToRole(x.Id, "Admin");
        //        userManager.AddLogin(x.Id, new UserLoginInfo("facebook", "1234"));
        //    }
        //    var x2 = userManager.FindByEmail("aaa@bbb.com");
        //    Assert.IsNull(x2);
        //}
    }
}
