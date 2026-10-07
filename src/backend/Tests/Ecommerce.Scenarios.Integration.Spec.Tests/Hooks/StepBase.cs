namespace Ecommerce.Scenarios.Integration.Spec.Tests.Hooks;

public abstract class StepBase
{
    protected async Task<Result<T>> SendAsync<T>(string uri)
    where T : class
    {
        var client = Hook.Client;
        client.BaseAddress = new Uri("http://localhost:5064/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        return new Result<T>(JsonConvert.DeserializeObject<T>(content), response.EnsureSuccessStatusCode());
    }
}