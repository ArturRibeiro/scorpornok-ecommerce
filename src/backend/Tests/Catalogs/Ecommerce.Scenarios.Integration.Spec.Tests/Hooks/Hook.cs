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
    // Um host só para todos os cenários: criar uma factory por requisição abre um FileSystemWatcher por host e
    // esgota o limite de inotify do sistema nos passos que percorrem o catálogo página a página.
    private static readonly CustomWebApplicationFactory Factory = new();
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