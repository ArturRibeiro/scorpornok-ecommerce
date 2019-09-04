using FluentAssertions;
using NUnit.Framework;
using Programming.Functional.Options;
using F = Programming.Functional.Options.Asserts;

namespace Programming.Functional.Tests.Options
{
    [TestFixture, Category("Frameworker.Programming.Functional")]
    public class OptionTests
    {
        [Test]
        public void Option_recebe_nulo()
        {
            Option<string> @value = null;

            @value.IsNone.Should().BeTrue();
            @value.IsSome.Should().BeFalse();
        }

        [Test]
        public void Option_recebe_none()
        {
            var @value = Option<string>.None();

            @value.IsNone.Should().BeTrue();
            @value.IsSome.Should().BeFalse();
        }

        [Test]
        public void Option_recebe_algum_valor()
        {
            Option<string> @value = null;

            @value.IsNone.Should().BeTrue();
            @value.IsSome.Should().BeFalse();
        }

        [TestCase("algum valor")]
        public void Option_recebe_some(string algumValor)
        {
            var @value = Option<string>.Some(algumValor);

            @value.IsNone.Should().BeFalse();
            @value.IsSome.Should().BeTrue();
        }

        [TestCase(1, 3.3)]
        [TestCase(1222, 4)]
        [TestCase(13, 3.9)]
        public void Map(int v1, double v2)
        {
            var v = Option<int>.Some(v1);

            var optionResult = v.Map(x => x * v2);
            double result = optionResult.Match(value => value, () => 0);

            optionResult.IsNone.Should().BeFalse();
            optionResult.IsSome.Should().BeTrue();
            result.Should().Be(v1 * v2);

        }

        [Test]
        public void Combine()
        {
            //Arrange's
            object obj1 = null;
            string message1 = "message1";

            string obj2 = "name3";
            string message2 = "message2";

            int obj3 = 10;
            string message3 = "message3";

            double obj4 = 10;
            string message4 = "message4";

            decimal obj5 = 1;
            string message5 = "message5";

            //Act
            var assert1 = F.Assert.IsNotNull(obj1, message1);
            var assert2 = F.Assert.IsNotNull(obj2, message2);
            var assert3 = F.Assert.IsGreaterThan(0, obj3, message3);
            var assert4 = F.Assert.IsGreaterThan(0, obj4, message4);
            var assert5 = F.Assert.IsGreaterThan(0, obj5, message5);
            var result = F.Option.Combine(assert1, assert2, assert3, assert4);

            //Assert's
            assert1.IsNone.Should().BeTrue();
            assert1.IsSome.Should().BeFalse();
            assert1.Message.Should().Be(message1);

            assert2.IsNone.Should().BeFalse();
            assert2.IsSome.Should().BeTrue();
            assert2.Message.Should().BeNull();

            assert3.IsNone.Should().BeFalse();
            assert3.IsSome.Should().BeTrue();
            assert3.Message.Should().BeNull();

            assert4.IsNone.Should().BeFalse();
            assert4.IsSome.Should().BeTrue();
            assert4.Message.Should().BeNull();

            assert5.IsNone.Should().BeFalse();
            assert5.IsSome.Should().BeTrue();
            assert5.Message.Should().BeNull();

            result.IsNone.Should().BeTrue();
            result.IsSome.Should().BeFalse();
            result.Message.Should().Be(message1);


        }


        [Test]
        public void Then()
        {
            //Arrange's
            Option<Person> opt = new Person()
            {
                Email = "artur", ID = 2
            };

            //Act
            var result = opt
                .OnSuccess(x => F.Assert.IsNotNull(x.Name, "name"))
                .OnSuccess(x => F.Assert.IsGreaterThan(0, x.ID, "ID"))
                .OnSuccess(x => F.Assert.IsNotNull(x.Email, "Email"));



            //Assert's
        }
    }

    public class Person
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}