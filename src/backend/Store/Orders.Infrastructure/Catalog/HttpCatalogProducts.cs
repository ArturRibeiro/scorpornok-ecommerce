namespace Orders.Infrastructure.Catalog;

/// <summary>
/// <see cref="ICatalogProducts"/> sobre o GET /GetProductsByIds do Catalog.Web.Api: uma chamada
/// por pedido. O endereço vem de Catalog:BaseAddress; o tempo-limite (5 s) é do HttpClient.
/// </summary>
public class HttpCatalogProducts(HttpClient httpClient) : ICatalogProducts
{
    public const string Section = "Catalog";
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    // Formato de ProductItemMessageResponse no Catalog (o id lá é long).
    private sealed record ProductResponse(long Id, string Name, string PictureUri, decimal Price);

    public async Task<IReadOnlyDictionary<int, CatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, CatalogProduct>();

        var query = string.Join("&", ids.Select(id => $"ids={id}"));
        List<ProductResponse>? products;
        try
        {
            using var response = await httpClient.GetAsync($"GetProductsByIds?{query}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new CatalogUnavailableException($"Catalog respondeu {(int)response.StatusCode}.");
            products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new CatalogUnavailableException("Catalog fora do ar.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CatalogUnavailableException("Catalog não respondeu a tempo.", ex);
        }
        catch (JsonException ex)
        {
            throw new CatalogUnavailableException("Resposta inválida do Catalog.", ex);
        }

        // Ids fora da faixa de int não correspondem a nenhum item do pedido: ficam como inexistentes.
        return (products ?? [])
            .Where(p => p.Id is >= int.MinValue and <= int.MaxValue)
            .ToDictionary(p => (int)p.Id, p => new CatalogProduct((int)p.Id, p.Name, p.PictureUri, p.Price));
    }
}
