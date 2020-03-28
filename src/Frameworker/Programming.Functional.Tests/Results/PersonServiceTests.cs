using System;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Programming.Functional.Results;
using Programming.Functional.Tests.Results.Domains.Persons;

namespace Programming.Functional.Tests.Results
{
    [TestFixture]
    public class PersonServiceTests
    {
        [TestCase("test")]
        public void Create_person_with_success(string test)
        {
            var mockPersonRepository = new Mock<IPersonRepository>();
            mockPersonRepository.Setup(x => x.GetPerson(It.IsAny<Guid>()))
                .Returns(Builder<Person>.CreateNew().Build());
            
            mockPersonRepository.Setup(x => x.Save(It.IsAny<Person>()))
                .Returns(Result.Ok);
            
            mockPersonRepository.Setup(x => x.Rollback());
            
            var service = new PersonService(mockPersonRepository.Object);
            var result = service.CreatePerson(Guid.NewGuid(), "Artur Ribeiro", "arturrj@gmail.com");

            result.Should().Be("OK");
            mockPersonRepository.Verify(x => x.GetPerson(It.IsAny<Guid>()), Times.Once);
            mockPersonRepository.Verify(x => x.Save(It.IsAny<Person>()), Times.Once);
            mockPersonRepository.Verify(x => x.Rollback(), Times.Never);

        }
    }
}