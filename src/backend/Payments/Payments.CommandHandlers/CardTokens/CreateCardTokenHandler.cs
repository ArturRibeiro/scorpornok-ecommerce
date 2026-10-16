namespace Payments.CommandHandlers.CardTokens;

public class CreateCardTokenHandler(ICardTokenRepository cardTokens)
    : IRequestHandler<CreateCardTokenCommand, CreateCardTokenResult>
{
    public async Task<CreateCardTokenResult> Handle(CreateCardTokenCommand command, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var errors = CardValidation.Validate(command.CardHolderName, command.CardNumber, command.ExpirationMonth,
            command.ExpirationYear, command.Cvv, now);
        if (errors.Count > 0)
            return CreateCardTokenResult.Invalid(errors);

        var token = CardToken.Create(command.CardHolderName, command.CardNumber, command.ExpirationMonth,
            command.ExpirationYear, now);
        cardTokens.Add(token);
        await cardTokens.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return CreateCardTokenResult.From(token);
    }
}
