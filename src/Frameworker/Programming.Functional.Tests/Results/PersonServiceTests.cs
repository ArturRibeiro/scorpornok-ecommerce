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
        [TestCase("Artur Ribeiro", "arturrj@gmail.com")]
        public void Create_person_with_success(string name, string email)
        {
            var mockPersonRepository = new Mock<IPersonRepository>();
            mockPersonRepository.Setup(x => x.Save(It.IsAny<Person>()))
                .Returns(Result.Ok);
            
            mockPersonRepository.Setup(x => x.Rollback());
            
            var service = new PersonService(mockPersonRepository.Object);
            var result = service.CreatePerson(name, email);

            result.Should().Be("OK");
            mockPersonRepository.Verify(x => x.GetPerson(It.IsAny<Guid>()), Times.Never);
            mockPersonRepository.Verify(x => x.Save(It.IsAny<Person>()), Times.Once);
            mockPersonRepository.Verify(x => x.Rollback(), Times.Never);

        }
        
        [TestCase(null, "arturrj@gmail.com", "Name is null")]
        [TestCase("Artur Ribeiro", null, "Email is null")]
        public void Create_person_missing_attributes(string name, string email, string message)
        {
            var mockPersonRepository = new Mock<IPersonRepository>();
            mockPersonRepository.Setup(x => x.Save(It.IsAny<Person>()))
                .Returns(Result.Ok);
            
            mockPersonRepository.Setup(x => x.Rollback());
            
            var service = new PersonService(mockPersonRepository.Object);
            var result = service.CreatePerson(name, email);

            result.Should().Be(message);
            mockPersonRepository.Verify(x => x.GetPerson(It.IsAny<Guid>()), Times.Never);
            mockPersonRepository.Verify(x => x.Save(It.IsAny<Person>()), Times.Never);
            mockPersonRepository.Verify(x => x.Rollback(), Times.Never);
        }

        [TestCase("Artur Ribeiro", "arturrj@gmail.com")]
        public void Create_person_error_occurred_while_saving(string name, string email)
        {
            var mockPersonRepository = new Mock<IPersonRepository>();
            mockPersonRepository.Setup(x => x.Save(It.IsAny<Person>()))
                .Returns(Result.Fail("Occurred while saving"));
            
            mockPersonRepository.Setup(x => x.Rollback());
            
            var service = new PersonService(mockPersonRepository.Object);
            var result = service.CreatePerson(name, email);

            result.Should().Be("Occurred while saving");
            mockPersonRepository.Verify(x => x.GetPerson(It.IsAny<Guid>()), Times.Never);
            mockPersonRepository.Verify(x => x.Save(It.IsAny<Person>()), Times.Once);
            mockPersonRepository.Verify(x => x.Rollback(), Times.Once);
        }
        
        [TestCase("Artur Ribeiro", "arturrj@gmail.com", "v1", "v2", "v3")]
        public void Create_person_with_address_success(string name, string email, string p1, string p2, string p3)
        {
            var mockPersonRepository = new Mock<IPersonRepository>();
            mockPersonRepository.Setup(x => x.Save(It.IsAny<Person>()))
                .Returns(Result.Ok);
            
            mockPersonRepository.Setup(x => x.Rollback());
            
            var service = new PersonService(mockPersonRepository.Object);
            var result = service.CreatePersonWithAddress(name, email, p1, p2, p3);

            // result.Should().Be("Occurred while saving");
             mockPersonRepository.Verify(x => x.GetPerson(It.IsAny<Guid>()), Times.Never);
             mockPersonRepository.Verify(x => x.Save(It.IsAny<Person>()), Times.Once);
             mockPersonRepository.Verify(x => x.Rollback(), Times.Never);
        }
    }
}