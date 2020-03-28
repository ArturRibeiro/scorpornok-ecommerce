using System;

namespace Programming.Functional.Tests.Results.Domains.Persons
{
    public class Person
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public Address Address { get; private set; }

        public Person AddAddress(string p1, string p2, string p3)
        {
            this.Address = Address.Factory.Create(p1, p2, p3);
            return this;
        }

        public static class Factory
        {
            public static Person Create(string name, string email)
                => new Person()
                {
                    Name = name,
                    Email = email
                };
        }
    }

    public class Address
    {
        public static class Factory
        {
            public static Address Create(string p1, string p2, string p3)
                =>  new Address();
        }
    }
}