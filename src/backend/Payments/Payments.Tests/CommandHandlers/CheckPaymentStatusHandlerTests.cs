using Payments.CommandHandlers.Payments;
using Payments.Infrastructure.Gateways;
using Payments.Tests.Fakes;
using Shared.Code.IntegrationEvents;
using Shared.Code.Models;

namespace Payments.Tests.CommandHandlers;

[TestFixture]
public class CheckPaymentStatusHandlerTests
{
    private InMemoryPayments _repository = null!;
    private Mock<IIntegrationEventPublisher> _publisher = null!;
    private CheckPaymentStatusHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new InMemoryPayments();
        _publisher = new Mock<IIntegrationEventPublisher>();
        _handler = new CheckPaymentStatusHandler(_repository, _publisher.Object);
    }

    private static CheckPaymentStatusCommand Query() => new("A123", 1);

    private static Payment Processing() => Payment.Start(7, "A123", 1, 25m, 1, "Fulano de Tal", "4111111111111111");

    [Test]
    public async Task Pagamento_aprovado_deve_republicar_PaymentApproved_sem_cobrar()
    {
        var payment = Processing();
        payment.Complete(new PaymentResult(true));
        _repository.Seed(payment);

        await _handler.Handle(Query(), CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(new PaymentApproved("A123", payment.Id), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Calls.Should().BeEmpty();
    }

    [Test]
    public async Task Pagamento_recusado_deve_republicar_PaymentRejected()
    {
        var payment = Processing();
        payment.Complete(new PaymentResult(false, "Cartão recusado"));
        _repository.Seed(payment);

        await _handler.Handle(Query(), CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(new PaymentRejected("A123", "Cartão recusado"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pagamento_em_processamento_deve_publicar_PaymentStatusPending()
    {
        _repository.Seed(Processing());

        await _handler.Handle(Query(), CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(new PaymentStatusPending("A123"), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Saved.Single().Status.Should().Be(PaymentStatus.Processing);
    }

    [Test]
    public async Task Sem_pagamento_deve_anular_e_publicar_PaymentNotProcessed()
    {
        await _handler.Handle(Query(), CancellationToken.None);

        var voided = _repository.Saved.Single();
        voided.Status.Should().Be(PaymentStatus.Voided);
        voided.IdempotencyKey.Should().Be("A123-1");
        voided.Reason.Should().Be(Payment.VoidedReason);
        _publisher.Verify(p => p.PublishAsync(new PaymentNotProcessed("A123"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consulta_repetida_depois_da_anulacao_deve_responder_de_novo_sem_outro_registro()
    {
        await _handler.Handle(Query(), CancellationToken.None);
        await _handler.Handle(Query(), CancellationToken.None);

        _repository.Saved.Should().ContainSingle();
        _publisher.Verify(p => p.PublishAsync(new PaymentNotProcessed("A123"), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Solicitacao_que_chega_depois_da_anulacao_deve_ser_recusada_sem_gateway()
    {
        await _handler.Handle(Query(), CancellationToken.None);
        var gateway = new Mock<IPaymentGateway>(MockBehavior.Strict);
        var process = new ProcessPaymentHandler(_repository, _repository, gateway.Object, _publisher.Object);

        await process.Handle(new ProcessPaymentCommand(7, "A123", 25m, 1, _repository.SeedToken("4111111111111111")),
            CancellationToken.None);

        _repository.Saved.Single().Status.Should().Be(PaymentStatus.Voided);
        _publisher.Verify(p => p.PublishAsync(new PaymentRejected("A123", Payment.VoidedReason), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Corrida_com_a_cobranca_deve_ser_resolvida_pela_chave_duplicada()
    {
        // A solicitação gravou "em processamento" depois que a consulta já tinha lido a chave.
        var stale = new StaleLookup(_repository);
        var handler = new CheckPaymentStatusHandler(stale, _publisher.Object);
        _repository.Seed(Processing());

        await ((Func<Task>)(() => handler.Handle(Query(), CancellationToken.None)))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("*índice único*");
        _publisher.VerifyNoOtherCalls();

        // Retentativa: encontra a cobrança em andamento e não anula.
        await _handler.Handle(Query(), CancellationToken.None);

        _repository.Saved.Single().Status.Should().Be(PaymentStatus.Processing);
        _publisher.Verify(p => p.PublishAsync(new PaymentStatusPending("A123"), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Consulta "atrasada": não vê o registro que a cobrança acabou de gravar.
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
