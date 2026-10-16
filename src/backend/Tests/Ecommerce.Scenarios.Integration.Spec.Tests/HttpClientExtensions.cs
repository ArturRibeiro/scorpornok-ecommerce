namespace Ecommerce.Scenarios.Integration.Spec.Tests;

public static class HttpClientExtensions
{
    public static Hooks.Result<IList<T>> GetList<T>(this HttpResponseMessage response)
        where T : class
    {
        response.EnsureSuccessStatusCode();
        var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonConvert.DeserializeObject<Hooks.Result<IList<T>>>(result)
            ?? throw new InvalidOperationException("A resposta veio vazia.");
    }
        
    public static Hooks.Result<PagedList<T>> PagedList<T>(this HttpResponseMessage response)
        where T : class
    {
        response.EnsureSuccessStatusCode();
        var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonConvert.DeserializeObject<Hooks.Result<PagedList<T>>>(result)
            ?? throw new InvalidOperationException("A resposta veio vazia.");
    }
}