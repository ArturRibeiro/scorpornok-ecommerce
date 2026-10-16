namespace Orders.CommandHandlers.Orders;

/// <summary>
/// Consulta os produtos no Catalog: o pedido usa o preço, o nome e a foto de lá, nunca os
/// valores que chegam do navegador. A implementação (HTTP) fica na Infrastructure.
/// </summary>
public interface ICatalogProducts
{
    /// <summary>
    /// Só os produtos encontrados, por id; os inexistentes ficam fora do dicionário.
    /// Lança <see cref="CatalogUnavailableException"/> se o Catalog não responder.
    /// </summary>
    Task<IReadOnlyDictionary<int, CatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);
}

public record CatalogProduct(int Id, string Name, string PictureUrl, decimal Price);

/// <summary>O Catalog não pôde ser consultado (fora do ar, tempo esgotado ou erro).</summary>
public class CatalogUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
