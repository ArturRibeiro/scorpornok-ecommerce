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

        var value = JsonConvert.DeserializeObject<T>(content)
            ?? throw new InvalidOperationException($"A resposta de {uri} veio vazia.");

        return new Result<T>(value, response.EnsureSuccessStatusCode());
    }

    // Consulta ao /graphql do Catalog (subgraph do gateway). Devolve a resposta inteira (data e errors).
    protected async Task<Newtonsoft.Json.Linq.JObject> GraphQLAsync(string query, object? variables = null)
    {
        var client = Hook.Client;
        client.BaseAddress = new Uri("http://localhost:5064/");
        var body = JsonConvert.SerializeObject(new { query, variables });
        var response = await client.PostAsync("graphql",
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        return Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync());
    }
}