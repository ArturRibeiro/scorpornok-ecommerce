namespace Ecommerce.Scenarios.Integration.Spec.Tests.Steps;

[Binding]
public class CatalogStepDefinitions : StepBase
{
    [Given(@"QUE esteja na tela principal")]
    public void GivenQueEstejaNaTelaPrincipal()
    {
        //ScenarioContext.StepIsPending();
    }

    [Then(@"listo todos os produtos em destaques")]
    public async Task ThenListoTodosOsProdutosEmDestaques()
    {
        var pagedList = await base.SendAsync<PagedList<ProductItemMessageResponse>>($"GetAllProducts?PageNumber={1}&PageSize={10}");
        pagedList.EnsureSuccessStatusCode.Should().NotBeNull();
        pagedList.Value.Should().NotBeNull();
        pagedList.Value?.Items.Should().HaveCountGreaterOrEqualTo(1);
        pagedList.Value?.PageInfo.Should().NotBeNull();
    }
}