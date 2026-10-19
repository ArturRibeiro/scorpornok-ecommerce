namespace Customers.CommandHandlers.Profiles;

/// <summary>Lê o cadastro do cliente do token.</summary>
/// <param name="Email">E-mail do token, devolvido quando ainda não há cadastro.</param>
public record GetCustomerProfileQuery(Guid CustomerId, string Email) : Message<CustomerProfile>;

/// <summary>Salva (cria ou substitui) o cadastro do cliente do token.</summary>
/// <param name="Email">E-mail do token: o cadastro sempre guarda o da conta.</param>
public record UpdateCustomerProfileCommand(
    Guid CustomerId,
    string Email,
    string? FullName,
    string? Phone,
    string? Cpf,
    AddressData? Address)
    : Message<UpdateCustomerProfileResult>;

/// <summary>O cadastro salvo, ou os erros por campo (chaves do JSON) sem nada gravado.</summary>
public record UpdateCustomerProfileResult(CustomerProfile? Profile, IReadOnlyDictionary<string, string[]> Errors)
{
    public bool IsValid => Profile is not null;

    public static UpdateCustomerProfileResult Saved(Customer customer)
        => new(CustomerProfile.From(customer), new Dictionary<string, string[]>());

    public static UpdateCustomerProfileResult Invalid(IReadOnlyDictionary<string, string[]> errors) => new(null, errors);
}
