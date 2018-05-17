using AbstractApplication.Data.NHibernate;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NHibernate.AspNet.Identity
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole>, IRoleStore<TRole>, IDisposable where TRole : IdentityRole
    {
        private bool _disposed;
        private INHibernateProviderFactory NHibernateProviderFactory { get; set; }

        /// <summary>
        /// If true then disposing this object will also dispose (close) the session. False means that external code is responsible for disposing the session.
        /// </summary>
        public bool ShouldDisposeSession { get; set; }

        public RoleStore(INHibernateProviderFactory nHibernateProviderFactory)
        {
            if (nHibernateProviderFactory == null)
                throw new ArgumentNullException("NHibernateProviderFactory is null");

            ShouldDisposeSession = true;
            NHibernateProviderFactory = nHibernateProviderFactory;
            NHibernateProviderFactory.Configuration();
        }

        public virtual Task<TRole> FindByIdAsync(string roleId)
        {
            this.ThrowIfDisposed();
            using (NHibernateProviderFactory.Start())
            {
                return Task.FromResult(NHibernateProviderFactory.Current.Session.Get<TRole>((object)roleId));
            }
        }

        public virtual Task<TRole> FindByNameAsync(string roleName)
        {
            this.ThrowIfDisposed();
            using (NHibernateProviderFactory.Start())
            {
                return Task.FromResult<TRole>(Queryable.FirstOrDefault<TRole>(Queryable.Where<TRole>(NHibernateProviderFactory.Current.Session.Query<TRole>(), (Expression<Func<TRole, bool>>)(u => u.Name.ToUpper() == roleName.ToUpper()))));
            }
        }

        public virtual  Task CreateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if ((object)role == null)
                throw new ArgumentNullException("role");
            using (NHibernateProviderFactory.Start())
            {
                NHibernateProviderFactory.Current.Session.Save(role);
                NHibernateProviderFactory.Current.TransactionalFlush();
            }
            return Task.FromResult(0);
        }

        public virtual Task DeleteAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }
            using (NHibernateProviderFactory.Start())
            {
                NHibernateProviderFactory.Current.Session.Delete(role);
                NHibernateProviderFactory.Current.TransactionalFlush();
            }
            return Task.FromResult(0);
        }

        public virtual Task UpdateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if ((object)role == null)
                throw new ArgumentNullException("role");
            using (NHibernateProviderFactory.Start())
            {
                NHibernateProviderFactory.Current.Session.Update(role);
                NHibernateProviderFactory.Current.TransactionalFlush();
            }
            return Task.FromResult(0);
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this._disposed && ShouldDisposeSession)
                NHibernateProviderFactory.Dispose();
            this._disposed = true;
            NHibernateProviderFactory = null;
        }

        public IQueryable<TRole> Roles
        {
            get
            {
                this.ThrowIfDisposed();

                if (!NHibernateProviderFactory.IsStarted)
                {
                    using (NHibernateProviderFactory.Start())
                    {
                        return NHibernateProviderFactory.Current.Session.Query<TRole>().ToList().AsQueryable();
                    } 
                }
                return NHibernateProviderFactory.Current.Session.Query<TRole>().ToList().AsQueryable();
            }
        }
    }
}
