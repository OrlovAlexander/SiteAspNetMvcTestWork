﻿using AbstractApplication.Domain;

namespace NHibernate.AspNet.Identity.Tests.Models
{
    public class Foo : Entity
    {
        public virtual string String { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}