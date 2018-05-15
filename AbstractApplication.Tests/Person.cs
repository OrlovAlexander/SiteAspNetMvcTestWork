using System;

namespace AbstractApplication.Tests
{
    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime Birthdate { get; set; }
    }
}
