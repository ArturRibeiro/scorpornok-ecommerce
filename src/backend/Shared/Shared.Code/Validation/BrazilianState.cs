namespace Shared.Code.Validation;

/// <summary>UF: sigla de um dos 26 estados ou do Distrito Federal.</summary>
public static class BrazilianState
{
    public static readonly IReadOnlySet<string> Codes = new HashSet<string>
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public static string Normalize(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;

    public static bool IsValid(string? value) => Codes.Contains(Normalize(value));
}
