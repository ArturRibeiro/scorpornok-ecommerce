namespace Customers.CommandHandlers.Profiles;

public class CustomerProfileHandlers(ICustomerRepository repository)
    : IRequestHandler<GetCustomerProfileQuery, CustomerProfile>,
      IRequestHandler<UpdateCustomerProfileCommand, UpdateCustomerProfileResult>
{
    public async Task<CustomerProfile> Handle(GetCustomerProfileQuery query, CancellationToken cancellationToken)
        => await repository.GetAsync(query.CustomerId, cancellationToken) is { } customer
            ? CustomerProfile.From(customer)
            : CustomerProfile.Empty(query.Email);

    // Upsert: o formulário sempre manda o cadastro inteiro. Valida antes de gravar; com erro, nada muda.
    public async Task<UpdateCustomerProfileResult> Handle(UpdateCustomerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(command.CustomerId, cancellationToken);
        var customer = existing ?? new Customer(command.CustomerId, command.Email);
        customer.Update(command.Email, command.FullName, command.Phone, command.Cpf, command.Address?.ToAddress());

        var validation = customer.Validate();
        if (!validation.IsValid)
            return UpdateCustomerProfileResult.Invalid(validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        if (existing is null)
            repository.Add(customer);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return UpdateCustomerProfileResult.Saved(customer);
    }
}
