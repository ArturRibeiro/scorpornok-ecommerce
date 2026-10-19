namespace Customers.Domain.Customers.Validations;

/// <summary>
/// Cadastro completo: todos os campos obrigatórios, menos o complemento do endereço. As chaves
/// dos erros são as do JSON, para a loja marcar o campo (<c>address.number</c>).
/// </summary>
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty().WithMessage("Informe o nome completo.")
            .Must(PersonName.IsValid).WithMessage("Informe nome e sobrenome (até 120 caracteres).")
            .OverridePropertyName("fullName");

        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty().WithMessage("Informe o telefone.")
            .Must(Phone.IsValid).WithMessage("Telefone inválido: informe o DDD e o número, com 10 ou 11 dígitos.")
            .OverridePropertyName("phone");

        RuleFor(x => x.Cpf)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty().WithMessage("Informe o CPF.")
            .Must(Cpf.IsValid).WithMessage("CPF inválido.")
            .OverridePropertyName("cpf");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Informe o endereço de entrega.")
            .SetValidator(new AddressValidator()!)
            .OverridePropertyName("address");
    }
}
