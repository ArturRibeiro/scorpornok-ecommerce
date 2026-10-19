namespace Customers.Domain.Customers;

/// <summary>
/// Cadastro do cliente, identificado pelo <c>sub</c> da conta do Keycloak. Telefone, CPF e CEP
/// são guardados só com dígitos; a loja formata para exibir.
/// </summary>
public class Customer : Entity<Guid>, IAggregateRoot
{
    //EF
    protected Customer() { }

    public Customer(Guid id, string email)
    {
        Id = id;
        Email = email.Trim();
    }

    /// <summary>E-mail da conta, copiado do token a cada gravação; a loja só o exibe.</summary>
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public Address? Address { get; private set; }

    /// <summary>Substitui o cadastro inteiro (o formulário sempre manda todos os campos).</summary>
    public void Update(string email, string? fullName, string? phone, string? cpf, Address? address)
    {
        Email = email.Trim();
        FullName = PersonName.Normalize(fullName);
        Phone = global::Shared.Code.Validation.Phone.Normalize(phone);
        Cpf = global::Shared.Code.Validation.Cpf.Normalize(cpf);
        Address = address;
    }

    /// <summary>Erros por campo, com as chaves do JSON (<c>cpf</c>, <c>address.zipCode</c>).</summary>
    public ValidationResult Validate() => new CustomerValidator().Validate(this);

    public override bool IsValid()
    {
        var result = Validate();
        if (!result.IsValid)
            SetValidation(result);
        return result.IsValid;
    }
}
