using System.Reflection;
using Shared.Code.Models;

namespace Payments.Tests.Fakes;

// Repositório em memória que imita o banco: guarda cópias (alterar o objeto em memória não
// grava nada), o que foi adicionado ou lido só é persistido no SaveEntitiesAsync, e uma
// gravação que falha descarta as mudanças, como a retentativa do MassTransit num escopo novo.
// FailNextSave simula a queda; a chave duplicada falha como o índice único.
internal sealed class InMemoryPayments : IPaymentRepository, IUnitOfWork
{
    private static readonly MethodInfo Clone =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly Dictionary<string, Payment> _saved = new();
    private readonly List<Payment> _added = [];
    private readonly List<Payment> _tracked = [];
    public List<string> Calls { get; } = [];
    public bool FailNextSave { get; set; }

    public IUnitOfWork UnitOfWork => this;
    public IReadOnlyCollection<Payment> Saved => _saved.Values;

    private static Payment Copy(Payment payment) => (Payment)Clone.Invoke(payment, null)!;

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
            _tracked.AddRange(_added);
            _added.Clear();
            return Task.FromResult(true);
        }
        catch
        {
            _added.Clear();
            _tracked.Clear();
            throw;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    public void Dispose() { }
}
