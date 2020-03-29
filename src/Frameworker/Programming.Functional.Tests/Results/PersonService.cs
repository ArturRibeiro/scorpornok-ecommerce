using System;
using Programming.Functional.Results;
using Programming.Functional.Results.Asserts;
using Programming.Functional.Tests.Results.Domains.Persons;

namespace Programming.Functional.Tests.Results
{
    public class PersonService
    {
        private IPersonRepository _personRepository;

        public PersonService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public string CreatePerson(string name, string email)
        {
            var nameIsNullOrEmpty = Assert2.IsNullOrEmpty(name, "Name is null");
            var emailIsNullOrEmpty = Assert2.IsNullOrEmpty(email, "Email is null");
            var person = Person.Factory.Create(name, email)
                .ToResult("Error occorred ");
            var combine = Result.Combine(nameIsNullOrEmpty, emailIsNullOrEmpty, person);

            return combine
                .OnSuccess(() => person.Value.Valid())
                .OnSuccess(() => _personRepository.Save(person.Value)
                    .Cath(() => _personRepository.Rollback()))
                .Finally(CreateLog)
                .Finally(CreateResult);
        }

        public string CreatePersonWithAddress(string name, string email, string p1, string p2, string p3)
        {
            var nameIsNullOrEmpty = Assert2.IsNullOrEmpty(name, "Name is null");
            var emailIsNullOrEmpty = Assert2.IsNullOrEmpty(email, "Email is null");
            var person = Person.Factory.Create(name, email)
                .ToResult("Error occorred ");
            var combine = Result.Combine(person, nameIsNullOrEmpty, emailIsNullOrEmpty);
            
            return combine
                .With(() => person.Value.AddAddress(p1, p2, p3))
                .OnSuccess(() => person.Value.Valid())
                .OnSuccess(() => _personRepository.Save(person.Value)
                    .Cath(() => _personRepository.Rollback()))
                .Finally(CreateLog)
                .Finally(CreateResult);
        }

        private string CreateResult(Result result)
            =>  result.IsSome ? "OK" : result.Message;

        #region Private Methods

        private static void CreateLog(Result result)
        {
            if (result.IsNone)
                ; //TODO: Create log
        }

        #endregion
    }
}