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
        var pagedList = await base.SendAsync<PagedList<ProductItemMessageResponse>>($"Products?PageNumber={1}&PageSize={10}");
        pagedList.EnsureSuccessStatusCode.Should().NotBeNull();
        pagedList.Value.Should().NotBeNull();
        pagedList.Value?.Items.Should().HaveCountGreaterOrEqualTo(1);
        pagedList.Value?.PageInfo.Should().NotBeNull();
    }

    private List<ProductItemMessageResponse> _products = [];

    [When(@"busco os produtos (\d+), (\d+) e (\d+)")]
    public async Task WhenBuscoOsProdutos(long first, long second, long third)
    {
        var result = await base.SendAsync<List<ProductItemMessageResponse>>(
            $"GetProductsByIds?ids={first}&ids={second}&ids={third}");
        _products = result.Value;
    }

    [Then(@"recebo só os produtos (\d+) e (\d+)")]
    public void ThenReceboSoOsProdutos(long first, long second)
    {
        _products.Select(p => p.Id).Should().BeEquivalentTo([first, second]);
    }
}
