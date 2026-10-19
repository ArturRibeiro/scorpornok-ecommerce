namespace Customers.CommandHandlers.Profiles;

/// <summary>Endereço como a API recebe e devolve (JSON <c>address</c>).</summary>
public record AddressData(
    string? Street,
    string? Number,
    string? Complement,
    string? District,
    string? City,
    string? State,
    string? ZipCode)
{
    public static AddressData From(Address address)
        => new(address.Street, address.Number, address.Complement, address.District, address.City, address.State,
            address.ZipCode);

    public Address ToAddress() => Address.Create(Street, Number, Complement, District, City, State, ZipCode);
}

/// <summary>
/// Cadastro como a loja o vê. Sem cadastro salvo, só o e-mail da conta, e o resto nulo: a loja
/// mostra o formulário em branco sem tratar 404.
/// </summary>
public record CustomerProfile(string Email, string? FullName, string? Phone, string? Cpf, AddressData? Address)
{
    public static CustomerProfile Empty(string email) => new(email, null, null, null, null);

    public static CustomerProfile From(Customer customer)
        => new(customer.Email, customer.FullName, customer.Phone, customer.Cpf,
            customer.Address is null ? null : AddressData.From(customer.Address));
}
