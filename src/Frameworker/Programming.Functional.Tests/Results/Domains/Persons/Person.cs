using System;
using Programming.Functional.Results;

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
        
        public Result Valid()
            =>  Result.Ok();

        #region Factory

        public static class Factory
        {
            public static Maybe<Person> Create(string name, string email)
            {
                return new Person()
                {
                    Name = name,
                    Email = email
                };
            }

            public static Maybe<Person> Create(string name, string email, string p1, string p2, string p3)
            {
                return new Person()
                {
                    Name = name,
                    Email = email
                }.AddAddress(p1, p2, p3);
            }
        }

        #endregion
    }
}