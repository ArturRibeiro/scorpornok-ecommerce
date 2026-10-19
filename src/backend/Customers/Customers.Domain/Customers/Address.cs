namespace Customers.Domain.Customers;

/// <summary>
/// Endereço de entrega no formato brasileiro. UF em maiúsculas e CEP só com dígitos; o
/// complemento é o único campo opcional.
/// </summary>
public class Address : ValueObject
{
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string District { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;

    //EF
    protected Address() { }

    public static Address Create(string? street, string? number, string? complement, string? district, string? city,
        string? state, string? zipCode)
        => new()
        {
            Street = street?.Trim() ?? string.Empty,
            Number = number?.Trim() ?? string.Empty,
            Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim(),
            District = district?.Trim() ?? string.Empty,
            City = city?.Trim() ?? string.Empty,
            State = BrazilianState.Normalize(state),
            ZipCode = global::Shared.Code.Validation.ZipCode.Normalize(zipCode)
        };

    protected override IEnumerable<object> GetValues()
    {
        yield return Street;
        yield return Number;
        yield return Complement ?? string.Empty;
        yield return District;
        yield return City;
        yield return State;
        yield return ZipCode;
    }
}
