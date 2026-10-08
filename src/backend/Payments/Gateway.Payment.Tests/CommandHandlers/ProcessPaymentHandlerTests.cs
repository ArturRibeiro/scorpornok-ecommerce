using Gateway.Payment.CommandHandlers.Payments;
using Gateway.Payment.Data.Gateways;
using Shared.Code.IntegrationEvents;
using Shared.Code.Models;

namespace Gateway.Payment.Tests.CommandHandlers;

[TestFixture]
public class ProcessPaymentHandlerTests
{
    private Mock<IPaymentRepository> _repository = null!;
    private Mock<IIntegrationEventPublisher> _publisher = null!;
    private ProcessPaymentHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository = new Mock<IPaymentRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(unitOfWork.Object);
        _publisher = new Mock<IIntegrationEventPublisher>();
        // O gateway real é um stub sem dependências; usá-lo deixa o teste mais fiel.
        _handler = new ProcessPaymentHandler(_repository.Object, new PaymentGateway(), _publisher.Object);
    }

    private static ProcessPaymentCommand Command(string cardNumber)
        => new(7, "A123", 25m, 1, new CardData("Fulano de Tal", cardNumber, "12", "2030", "123"));

    [Test]
    public async Task Cartao_aprovado_deve_gravar_e_publicar_PaymentApproved()
    {
        Payment.Domain.Payment? saved = null;
        _repository.Setup(r => r.Add(It.IsAny<Payment.Domain.Payment>())).Callback((Payment.Domain.Payment p) => saved = p);

        await _handler.Handle(Command("4111111111111111"), CancellationToken.None);

        saved.Should().NotBeNull();
        saved!.Approved.Should().BeTrue();
        saved.CardLast4.Should().Be("1111");
        saved.Amount.Should().Be(25m);
        _publisher.Verify(p => p.PublishAsync(new PaymentApproved("A123", saved.Id), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentRejected>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Cartao_recusado_deve_gravar_e_publicar_PaymentRejected()
    {
        Payment.Domain.Payment? saved = null;
        _repository.Setup(r => r.Add(It.IsAny<Payment.Domain.Payment>())).Callback((Payment.Domain.Payment p) => saved = p);

        await _handler.Handle(Command("4111111111110000"), CancellationToken.None);

        saved!.Approved.Should().BeFalse();
        _publisher.Verify(p => p.PublishAsync(new PaymentRejected("A123", PaymentGateway.RejectedReason), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentApproved>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Solicitacao_repetida_nao_deve_cobrar_de_novo_e_deve_republicar_o_resultado()
    {
        var existing = Payment.Domain.Payment.Create(7, "A123", 25m, 1, "Fulano de Tal", "4111111111111111", new PaymentResult(true));
        _repository.Setup(r => r.GetByOrderNumberAsync("A123", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await _handler.Handle(Command("4111111111111111"), CancellationToken.None);

        _repository.Verify(r => r.Add(It.IsAny<Payment.Domain.Payment>()), Times.Never);
        _publisher.Verify(p => p.PublishAsync(new PaymentApproved("A123", existing.Id), It.IsAny<CancellationToken>()), Times.Once);
    }
}
