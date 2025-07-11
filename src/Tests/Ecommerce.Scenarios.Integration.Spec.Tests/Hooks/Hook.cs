namespace Ecommerce.Scenarios.Integration.Spec.Tests.Hooks;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Permite customizar a configuração antes de subir o host
        builder.ConfigureServices(services =>
        {
            // Exemplo: remover DbContext real
            // var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MyDbContext));
            // if (descriptor != null) services.Remove(descriptor);

            // Exemplo: injetar serviços fake
            // services.AddSingleton<IMyService, FakeMyService>();
        });
    }
}

[Binding]
public sealed class Hook
{
    private static CustomWebApplicationFactory Factory => new CustomWebApplicationFactory();
    public static HttpClient Client => Factory.CreateClient();

    [BeforeScenario]
    public void BeforeScenario()
    {

    }


    [AfterScenario]
    public void AfterScenario()
    {
        
    }
}