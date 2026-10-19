namespace Shared.Code.Validation;

/// <summary>Nome completo: ao menos 2 palavras, até 120 caracteres.</summary>
public static class PersonName
{
    public const int MaxLength = 120;

    /// <summary>Tira os espaços das pontas e junta os repetidos.</summary>
    public static string Normalize(string? value)
        => string.Join(' ', (value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    public static bool IsValid(string? value)
    {
        var name = Normalize(value);
        return name.Length <= MaxLength && name.Split(' ').Count(word => word.Any(char.IsLetter)) >= 2;
    }
}
