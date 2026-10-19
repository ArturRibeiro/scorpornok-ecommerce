namespace Shared.Code.Validation;

/// <summary>CEP: 8 dígitos.</summary>
public static class ZipCode
{
    /// <summary>Aceita o hífen (20040-020 → 20040020).</summary>
    public static string Normalize(string? value) => Digits.Strip(value, '-', '.', ' ');

    public static bool IsValid(string? value) => Digits.AreAll(Normalize(value), 8);
}
