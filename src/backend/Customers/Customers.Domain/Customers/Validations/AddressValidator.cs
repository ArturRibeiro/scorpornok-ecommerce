namespace Customers.Domain.Customers.Validations;

/// <summary>Endereço completo: tudo obrigatório, menos o complemento.</summary>
public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().WithMessage("Informe a rua.").OverridePropertyName("street");
        RuleFor(x => x.Number).NotEmpty().WithMessage("Informe o número.").OverridePropertyName("number");
        RuleFor(x => x.District).NotEmpty().WithMessage("Informe o bairro.").OverridePropertyName("district");
        RuleFor(x => x.City).NotEmpty().WithMessage("Informe a cidade.").OverridePropertyName("city");

        RuleFor(x => x.State)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty().WithMessage("Informe a UF.")
            .Must(BrazilianState.IsValid).WithMessage("UF inválida.")
            .OverridePropertyName("state");

        RuleFor(x => x.ZipCode)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .NotEmpty().WithMessage("Informe o CEP.")
            .Must(ZipCode.IsValid).WithMessage("CEP inválido: informe 8 dígitos.")
            .OverridePropertyName("zipCode");
    }
}
