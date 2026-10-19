namespace Shared.Code.Validation;

/// <summary>CPF: 11 dígitos, com os dois dígitos verificadores corretos e não todos iguais.</summary>
public static class Cpf
{
    /// <summary>Aceita pontuação (529.982.247-25 → 52998224725).</summary>
    public static string Normalize(string? value) => Digits.Strip(value, '.', '-', ' ');

    public static bool IsValid(string? value)
    {
        var digits = Normalize(value);
        if (!Digits.AreAll(digits, 11) || digits.Distinct().Count() == 1)
            return false;

        return CheckDigit(digits, 9) == digits[9] - '0' && CheckDigit(digits, 10) == digits[10] - '0';
    }

    // Soma ponderada dos primeiros "length" dígitos, com pesos de length + 1 até 2.
    private static int CheckDigit(string digits, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
            sum += (digits[i] - '0') * (length + 1 - i);
        var rest = sum % 11;
        return rest < 2 ? 0 : 11 - rest;
    }
}
