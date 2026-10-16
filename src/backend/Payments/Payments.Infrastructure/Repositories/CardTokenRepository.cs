namespace Payments.Infrastructure.Repositories;

public class CardTokenRepository(PaymentContext context) : ICardTokenRepository
{
    // Mesmo contexto do PaymentRepository (escopo da mensagem): marcar o token como usado e
    // gravar o pagamento saem no mesmo SaveChanges.
    public IUnitOfWork UnitOfWork => context;

    public Task<CardToken> GetAsync(string id, CancellationToken cancellationToken = default)
        => context.CardTokens.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void Add(CardToken token) => context.CardTokens.Add(token);
}
