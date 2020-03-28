using System;
using Programming.Functional.Results;
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

        public string CreatePerson(Guid personId, string name, string email)
        {
            var nameIsNullOrEmpty = Result.Ok(!string.IsNullOrEmpty(name));
            var emailIsNullOrEmpty = Result.Ok(!string.IsNullOrEmpty(email));
            var person = _personRepository.GetPerson(personId).ToResult("Person is not exists");

            return Result.Combine(nameIsNullOrEmpty, emailIsNullOrEmpty, person)
                .OnSuccess(() => _personRepository.Save(CreateNewPerson(name, email))
                    .Cath(() => _personRepository.Rollback()))
                .Finally(CreateLog)
                .Finally(result => result.IsSome ? "OK" : result.Message);
            ;
        }

        #region Private Methods

        private static Person CreateNewPerson(string name, string email)
            => Person.Factory.Create(name, email)
                .AddAddress("p1", "p2", "p3");


        private static void CreateLog(Result result)
        {
            if (result.IsNone)
                ; //TODO: Create log
        }

        #endregion
    }
}