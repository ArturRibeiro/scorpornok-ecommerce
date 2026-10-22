namespace Payments.Tests.Fakes;

// Repositório em memória que imita o banco: guarda cópias (alterar o objeto em memória não
// grava nada), o que foi adicionado ou lido só é persistido no SaveEntitiesAsync, e uma
// gravação que falha descarta as mudanças, como a retentativa do MassTransit num escopo novo.
// FailNextSave simula a queda; a chave duplicada falha como o índice único.
// Também guarda os tokens do cartão, na mesma unidade de trabalho (como o PaymentContext real):
// marcar o token como usado só vale se a gravação do pagamento der certo.
internal sealed class InMemoryPayments : IPaymentRepository, ICardTokenRepository, IUnitOfWork
{
    private static readonly MethodInfo Clone =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly Dictionary<string, Payment> _saved = new();
    private readonly List<Payment> _added = [];
    private readonly List<Payment> _tracked = [];
    private readonly Dictionary<string, CardToken> _tokens = new();
    private readonly List<CardToken> _trackedTokens = [];
    public List<string> Calls { get; } = [];
    public bool FailNextSave { get; set; }

    public IUnitOfWork UnitOfWork => this;
    public IReadOnlyCollection<Payment> Saved => _saved.Values;

    private static Payment Copy(Payment payment) => (Payment)Clone.Invoke(payment, null)!;
    private static CardToken Copy(CardToken token) => (CardToken)Clone.Invoke(token, null)!;

    public IReadOnlyCollection<CardToken> Tokens => _tokens.Values;

    public Task<CardToken> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        if (id is null || !_tokens.TryGetValue(id, out var saved)) return Task.FromResult<CardToken>(null!);
        var loaded = Copy(saved);
        _trackedTokens.Add(loaded);
        return Task.FromResult(loaded);
    }

    public void Add(CardToken token) => _tokens[token.Id] = Copy(token);

    // Cria e grava um token para o cartão, como o POST /card-tokens; devolve o id.
    public string SeedToken(string cardNumber, DateTime? createdAt = null)
    {
        var token = CardToken.Create("Fulano de Tal", cardNumber, "12", "2030", createdAt ?? DateTime.UtcNow);
        _tokens[token.Id] = Copy(token);
        return token.Id;
    }

    public Task<Payment> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (!_saved.TryGetValue(idempotencyKey, out var saved)) return Task.FromResult<Payment>(null!);
        var loaded = Copy(saved);
        _tracked.Add(loaded);
        return Task.FromResult(loaded);
    }

    public void Add(Payment payment) => _added.Add(payment);

    // Grava um pagamento já existente (ex.: o que outra entrega gravou primeiro).
    public void Seed(Payment payment) => _saved[payment.IdempotencyKey] = Copy(payment);

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var changing = _added.Concat(_tracked).ToList();
        Calls.Add("save:" + string.Join(",", changing.Select(p => p.Status)));
        try
        {
            if (FailNextSave)
            {
                FailNextSave = false;
                throw new InvalidOperationException("banco fora do ar");
            }
            if (_added.Any(p => _saved.ContainsKey(p.IdempotencyKey)))
                throw new InvalidOperationException("violação do índice único em IdempotencyKey");
            foreach (var payment in changing)
                _saved[payment.IdempotencyKey] = Copy(payment);
            foreach (var token in _trackedTokens)
                _tokens[token.Id] = Copy(token);
            _tracked.AddRange(_added);
            _added.Clear();
            return Task.FromResult(true);
        }
        catch
        {
            _added.Clear();
            _tracked.Clear();
            _trackedTokens.Clear();
            throw;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    public void Dispose() { }
}
