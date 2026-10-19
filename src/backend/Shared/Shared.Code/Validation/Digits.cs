namespace Shared.Code.Validation;

/// <summary>
/// Tira a pontuação aceita em CPF, telefone e CEP. Letras e outros símbolos ficam, para a
/// validação recusar o valor em vez de ele virar outro número.
/// </summary>
public static class Digits
{
    public static string Strip(string? value, params char[] punctuation)
        => value is null ? string.Empty : new string(value.Where(c => !punctuation.Contains(c)).ToArray());

    public static bool AreAll(string value, int length)
        => value.Length == length && value.All(char.IsAsciiDigit);
}
