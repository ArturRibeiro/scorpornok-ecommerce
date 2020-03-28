using System;
using Programming.Functional.Results;

namespace Programming.Functional.Tests.Results.Domains.Persons
{
    public interface IPersonRepository
    {
        Maybe<Person> GetPerson(Guid personId);
        Result Save(Person person);
        void Rollback();
    }
}