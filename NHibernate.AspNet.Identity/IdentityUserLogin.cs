﻿using AbstractApplication.Domain;

namespace NHibernate.AspNet.Identity
{
    public class IdentityUserLogin : ValueObject
    {
        public virtual string LoginProvider { get; set; }

        public virtual string ProviderKey { get; set; }

    }
}
