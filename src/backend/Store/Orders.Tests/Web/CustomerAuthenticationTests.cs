using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Orders.Web.Api.Authentication;

namespace Orders.Tests.Web;

[TestFixture]
public class CustomerAuthenticationTests
{
    private static readonly Guid CustomerId = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private static ClaimsPrincipal User(params (string Type, string Value)[] claims)
        => new(new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), authenticationType: "Bearer"));

    private static ClaimsPrincipal Customer(string emailVerified = "true")
        => User(("sub", CustomerId.ToString()), ("email", "cliente@exemplo.com"), ("email_verified", emailVerified));

    [Test]
    public void Identidade_completa_vem_do_sub_e_do_email()
        => CustomerIdentity.From(Customer()).Should().Be(new CustomerIdentity(CustomerId, "cliente@exemplo.com"));

    [Test]
    public void Sem_sub_nao_ha_identidade()
        => CustomerIdentity.From(User(("email", "cliente@exemplo.com"))).Should().BeNull();

    [Test]
    public void Sub_que_nao_e_GUID_nao_e_identidade()
        => CustomerIdentity.From(User(("sub", "admin"), ("email", "cliente@exemplo.com"))).Should().BeNull();

    [Test]
    public void Sem_email_nao_ha_identidade()
        => CustomerIdentity.From(User(("sub", CustomerId.ToString()))).Should().BeNull();

    private static async Task<bool> Authorize(ClaimsPrincipal user)
    {
        var services = new ServiceCollection().AddLogging()
            .AddAuthorization(options => options.AddPolicy(CustomerAuthentication.VerifiedCustomerPolicy,
                CustomerAuthentication.VerifiedCustomer))
            .BuildServiceProvider();
        var result = await services.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, CustomerAuthentication.VerifiedCustomerPolicy);
        return result.Succeeded;
    }

    [Test]
    public async Task Politica_aprova_email_confirmado()
        => (await Authorize(Customer())).Should().BeTrue();

    [Test]
    public async Task Politica_reprova_email_nao_confirmado()
        => (await Authorize(Customer(emailVerified: "false"))).Should().BeFalse();

    [Test]
    public async Task Politica_reprova_sem_email_verified()
        => (await Authorize(User(("sub", CustomerId.ToString()), ("email", "cliente@exemplo.com")))).Should().BeFalse();

    [Test]
    public async Task Politica_reprova_sem_identidade_completa()
        => (await Authorize(User(("sub", "admin"), ("email", "cliente@exemplo.com"), ("email_verified", "true"))))
            .Should().BeFalse();

    [Test]
    public async Task Politica_reprova_anonimo()
        => (await Authorize(new ClaimsPrincipal(new ClaimsIdentity()))).Should().BeFalse();
}
