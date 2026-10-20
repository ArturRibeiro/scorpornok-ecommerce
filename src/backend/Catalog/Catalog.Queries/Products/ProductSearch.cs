namespace Catalog.Queries.Products;

// Filtros da busca, todos opcionais. Words(), SelectedBrands() e IsEmptyRange() devolvem a entrada
// normalizada que a consulta usa; nada aqui gera erro.
public class ProductSearch
{
    public const int MaxTermLength = 100;

    public const int MaxWords = 10;

    public string Term { get; set; }

    public IReadOnlyCollection<string> Brands { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    // Palavras do termo; o que passa de 100 caracteres ou de 10 palavras é ignorado. Maiúsculas e
    // acentos ficam para a consulta (ILIKE com unaccent).
    public IReadOnlyList<string> Words()
    {
        var term = (Term ?? "").Trim();
        if (term.Length > MaxTermLength) term = term[..MaxTermLength];

        return term
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxWords)
            .ToList();
    }

    public IReadOnlyList<string> SelectedBrands()
        => (Brands ?? []).Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => b.Trim()).Distinct().ToList();

    // Mínimo acima do máximo: nenhum produto cabe na faixa.
    public bool IsEmptyRange() => MinPrice > MaxPrice;
}
