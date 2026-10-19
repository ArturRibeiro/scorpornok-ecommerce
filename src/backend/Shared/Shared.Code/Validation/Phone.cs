namespace Shared.Code.Validation;

/// <summary>Telefone com DDD: 10 (fixo) ou 11 dígitos (celular), DDD sem zero.</summary>
public static class Phone
{
    /// <summary>Aceita pontuação ((21) 99999-8888 → 21999998888).</summary>
    public static string Normalize(string? value) => Digits.Strip(value, '(', ')', '-', ' ');

    public static bool IsValid(string? value)
    {
        var digits = Normalize(value);
        return (Digits.AreAll(digits, 10) || Digits.AreAll(digits, 11)) && digits[0] != '0' && digits[1] != '0';
    }
}
