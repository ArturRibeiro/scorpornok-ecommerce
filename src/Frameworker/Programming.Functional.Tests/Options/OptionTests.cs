using FluentAssertions;
using NUnit.Framework;
using Programming.Functional.Options;

namespace Programming.Functional.Tests.Options
{
    [TestFixture]
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
    }
}