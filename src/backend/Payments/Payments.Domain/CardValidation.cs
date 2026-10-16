namespace Payments.Domain;

/// <summary>
/// Regras do cartão na tokenização, as mesmas do checkout (src/frontend/src/lib/checkout.ts).
/// Os erros vêm por campo, com os nomes que a loja usa, para ela marcar o campo certo.
/// </summary>
public static class CardValidation
{
    public static IReadOnlyDictionary<string, string> Validate(string cardHolderName, string cardNumber,
        string expirationMonth, string expirationYear, string cvv, DateTime today)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(cardHolderName))
            errors["cardHolderName"] = "Obrigatório";

        // O número pode vir com espaços ou hífens, como o cliente digitou.
        var digits = (cardNumber ?? "").Replace(" ", "").Replace("-", "");
        if (digits.Length is < 13 or > 19 || !digits.All(char.IsDigit))
            errors["cardNumber"] = "O número do cartão deve ter de 13 a 19 dígitos";

        if ((cvv ?? "").Length is < 3 or > 4 || !cvv.All(char.IsDigit))
            errors["cvv"] = "O CVV deve ter 3 ou 4 dígitos";

        if (!int.TryParse(expirationMonth, out var month) || (expirationMonth ?? "").Length > 2 || month is < 1 or > 12)
            errors["expirationMonth"] = "Mês inválido";
        else if ((expirationYear ?? "").Length != 4 || !int.TryParse(expirationYear, out var year))
            errors["expirationYear"] = "Ano inválido";
        // O cartão vale até o fim do mês de validade.
        else if (year < today.Year || (year == today.Year && month < today.Month))
            errors["expirationYear"] = "Cartão vencido";

        return errors;
    }
}
