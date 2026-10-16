using System.Reflection;
using Payments.CommandHandlers.Payments;
using Payments.Infrastructure.Gateways;
using Payments.Tests.Fakes;
using Shared.Code.IntegrationEvents;
using Shared.Code.Models;

namespace Payments.Tests.CommandHandlers;

[TestFixture]
public class ProcessPaymentHandlerTests
{
    // Gateway simulado real, com registro das chamadas: a chave e se a chamada gerou cobrança nova.
    private sealed class SpyGateway(List<string> calls) : IPaymentGateway
    {
        private readonly PaymentGateway _gateway = new();
        public List<string> Keys { get; } = [];
        public int Charges => Keys.Distinct().Count();

        public Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request)
        {
            calls.Add("charge");
            Keys.Add(request.IdempotencyKey);
            return _gateway.SendProcessPaymentAsync(request);
        }
    }

    private InMemoryPayments _repository = null!;
    private SpyGateway _gateway = null!;
    private Mock<IIntegrationEventPublisher> _publisher = null!;
    private ProcessPaymentHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new InMemoryPayments();
        _gateway = new SpyGateway(_repository.Calls);
        _publisher = new Mock<IIntegrationEventPublisher>();
        _handler = new ProcessPaymentHandler(_repository, _gateway, _publisher.Object);
    }

    private static ProcessPaymentCommand Command(string cardNumber = "4111111111111111")
        => new(7, "A123", 25m, 1, new CardData("Fulano de Tal", cardNumber, "12", "2030", "123"));

    [Test]
    public async Task Deve_gravar_em_processamento_antes_de_cobrar_e_depois_completar()
    {
        await _handler.Handle(Command(), CancellationToken.None);

        _repository.Calls.Should().Equal("save:Processing", "charge", "save:Approved");
        _gateway.Keys.Should().Equal("A123-1");
    }

    [Test]
    public async Task Cartao_aprovado_deve_gravar_e_publicar_PaymentApproved()
    {
        await _handler.Handle(Command("4111111111111111"), CancellationToken.None);

        var saved = _repository.Saved.Single();
        saved.Status.Should().Be(PaymentStatus.Approved);
        saved.CardLast4.Should().Be("1111");
        saved.Amount.Should().Be(25m);
        saved.IdempotencyKey.Should().Be("A123-1");
        _publisher.Verify(p => p.PublishAsync(new PaymentApproved("A123", saved.Id), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentRejected>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Cartao_recusado_deve_gravar_e_publicar_PaymentRejected()
    {
        await _handler.Handle(Command("4111111111110000"), CancellationToken.None);

        _repository.Saved.Single().Status.Should().Be(PaymentStatus.Rejected);
        _publisher.Verify(p => p.PublishAsync(new PaymentRejected("A123", PaymentGateway.RejectedReason), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentApproved>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Falha_ao_gravar_o_resultado_e_reentrega_nao_devem_cobrar_duas_vezes()
    {
        // 1ª entrega: grava "em processamento", cobra e cai na segunda gravação (a do resultado).
        var failingOnResult = new FailOnSecondSave(_repository);
        var handler = new ProcessPaymentHandler(failingOnResult, _gateway, _publisher.Object);
        await ((Func<Task>)(() => handler.Handle(Command(), CancellationToken.None)))
            .Should().ThrowAsync<InvalidOperationException>();
        _repository.Saved.Single().Status.Should().Be(PaymentStatus.Processing);
        _publisher.VerifyNoOtherCalls();

        // 2ª entrega (reentrega): mesmo registro, mesma chave; o gateway devolve o resultado original.
        await _handler.Handle(Command(), CancellationToken.None);

        _gateway.Keys.Should().Equal("A123-1", "A123-1");
        _gateway.Charges.Should().Be(1);
        var payment = _repository.Saved.Single();
        payment.Status.Should().Be(PaymentStatus.Approved);
        _publisher.Verify(p => p.PublishAsync(new PaymentApproved("A123", payment.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Entrega_simultanea_que_perde_a_gravacao_falha_e_a_retentativa_nao_cobra_de_novo()
    {
        // A outra entrega gravou "em processamento" depois que esta já tinha consultado a chave.
        var stale = new StaleLookup(_repository);
        var handler = new ProcessPaymentHandler(stale, _gateway, _publisher.Object);
        _repository.Seed(Payment.Start(7, "A123", 1, 25m, 1, "Fulano de Tal", "4111111111111111"));

        await ((Func<Task>)(() => handler.Handle(Command(), CancellationToken.None)))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("*índice único*");
        _gateway.Keys.Should().BeEmpty();

        // Retentativa: encontra o registro da outra entrega e completa com a mesma chave.
        await _handler.Handle(Command(), CancellationToken.None);

        _repository.Saved.Should().ContainSingle().Which.Status.Should().Be(PaymentStatus.Approved);
        _gateway.Charges.Should().Be(1);
    }

    [Test]
    public async Task Pagamento_ja_completo_so_deve_republicar_o_resultado()
    {
        var existing = Payment.Start(7, "A123", 1, 25m, 1, "Fulano de Tal", "4111111111111111");
        existing.Complete(new PaymentResult(true));
        _repository.Seed(existing);

        await _handler.Handle(Command(), CancellationToken.None);

        _gateway.Keys.Should().BeEmpty();
        _repository.Calls.Should().BeEmpty();
        _publisher.Verify(p => p.PublishAsync(new PaymentApproved("A123", existing.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Deixa a primeira gravação passar e derruba a segunda (a do resultado, depois de cobrar).
    private sealed class FailOnSecondSave(InMemoryPayments inner) : IPaymentRepository, IUnitOfWork
    {
        private int _saves;
        public IUnitOfWork UnitOfWork => this;
        public Task<Payment> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default)
            => inner.GetByIdempotencyKeyAsync(key, ct);
        public void Add(Payment payment) => inner.Add(payment);
        public Task<bool> SaveEntitiesAsync(CancellationToken ct = default)
        {
            if (++_saves == 2) inner.FailNextSave = true;
            return inner.SaveEntitiesAsync(ct);
        }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
        public void Dispose() { }
    }

    // Consulta "atrasada": não vê o registro que outra entrega acabou de gravar.
    private sealed class StaleLookup(InMemoryPayments inner) : IPaymentRepository, IUnitOfWork
    {
        public IUnitOfWork UnitOfWork => this;
        public Task<Payment> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default)
            => Task.FromResult<Payment>(null!);
        public void Add(Payment payment) => inner.Add(payment);
        public Task<bool> SaveEntitiesAsync(CancellationToken ct = default) => inner.SaveEntitiesAsync(ct);
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
        public void Dispose() { }
    }
}
