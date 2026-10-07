using System;

namespace Catalog.Queries;

// Os dois parâmetros são opcionais na query string. Page() e Size()
// devolvem os valores normalizados que a consulta usa.
public class PagingModel
{
    public const int MaxPageSize = 20;

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public int Page() => Math.Max(PageNumber ?? 1, 1);

    public int Size() => Math.Clamp(PageSize ?? MaxPageSize, 1, MaxPageSize);
}
