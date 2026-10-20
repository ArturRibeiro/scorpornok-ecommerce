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
        _tokens.Clear();
        _gateway = new SpyGateway(_repository.Calls);
        _publisher = new Mock<IIntegrationEventPublisher>();
        _handler = new ProcessPaymentHandler(_repository, _repository, _gateway, _publisher.Object);
    }

    // Um token por cartão e tentativa, criado uma vez: a reentrega traz a mesma mensagem, com o mesmo
    // token; uma nova tentativa tokeniza o cartão de novo no navegador.
    private readonly Dictionary<string, string> _tokens = new();

    private ProcessPaymentCommand Command(string cardNumber = "4111111111111111", string orderNumber = "A123",
        int attempt = 1)
    {
        var tokenKey = $"{cardNumber}#{attempt}";
        if (!_tokens.TryGetValue(tokenKey, out var token))
            _tokens[tokenKey] = token = _repository.SeedToken(cardNumber);
        return new(7, orderNumber, 25m, 1, token, attempt);
    }

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
        var handler = new ProcessPaymentHandler(failingOnResult, _repository, _gateway, _publisher.Object);
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
        var handler = new ProcessPaymentHandler(stale, _repository, _gateway, _publisher.Object);
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

    [Test]
    public async Task Token_valido_deve_ser_marcado_como_usado_na_gravacao_do_pagamento()
    {
        var command = Command();

        await _handler.Handle(command, CancellationToken.None);

        _repository.Tokens.Single(t => t.Id == command.CardToken).UsedAt.Should().NotBeNull();
        _repository.Saved.Single().CardHolderName.Should().Be("Fulano de Tal");
    }

    [Test]
    public async Task Falha_ao_gravar_o_pagamento_nao_deve_gastar_o_token()
    {
        var command = Command();
        _repository.FailNextSave = true;

        await ((Func<Task>)(() => _handler.Handle(command, CancellationToken.None)))
            .Should().ThrowAsync<InvalidOperationException>();

        _repository.Tokens.Single().UsedAt.Should().BeNull();
        _gateway.Keys.Should().BeEmpty();
    }

    [Test]
    public async Task Token_desconhecido_deve_recusar_sem_gateway_nem_erro()
    {
        await _handler.Handle(new ProcessPaymentCommand(7, "A123", 25m, 1, "tok_inexistente"), CancellationToken.None);

        AssertRejectedByToken();
    }

    [Test]
    public async Task Token_vencido_deve_recusar_sem_gateway()
    {
        var expired = _repository.SeedToken("4111111111111111", createdAt: DateTime.UtcNow.AddHours(-25));

        await _handler.Handle(new ProcessPaymentCommand(7, "A123", 25m, 1, expired), CancellationToken.None);

        AssertRejectedByToken();
    }

    [Test]
    public async Task Token_usado_por_outro_pedido_deve_recusar()
    {
        var token = Command().CardToken;
        await _handler.Handle(new ProcessPaymentCommand(8, "B456", 25m, 1, token), CancellationToken.None);
        _gateway.Keys.Clear();
        _publisher.Invocations.Clear();

        await _handler.Handle(new ProcessPaymentCommand(7, "A123", 25m, 1, token), CancellationToken.None);

        AssertRejectedByToken();
    }

    private void AssertRejectedByToken()
    {
        var payment = _repository.Saved.Single(p => p.OrderNumber == "A123");
        payment.Status.Should().Be(PaymentStatus.Rejected);
        payment.Reason.Should().Be(ProcessPaymentHandler.InvalidTokenReason);
        _gateway.Keys.Should().BeEmpty();
        _publisher.Verify(p => p.PublishAsync(new PaymentRejected("A123", ProcessPaymentHandler.InvalidTokenReason),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Nova_tentativa_do_mesmo_pedido_gera_novo_pagamento_e_a_repeticao_dela_nao()
    {
        await _handler.Handle(Command("4111111111110000", attempt: 1), CancellationToken.None);

        await _handler.Handle(Command("4111111111111111", attempt: 2), CancellationToken.None);
        await _handler.Handle(Command("4111111111111111", attempt: 2), CancellationToken.None);

        _repository.Saved.Should().HaveCount(2);
        _repository.Saved.Select(p => (p.IdempotencyKey, p.Status)).Should().BeEquivalentTo(new[]
        {
            ("A123-1", PaymentStatus.Rejected),
            ("A123-2", PaymentStatus.Approved)
        });
        _gateway.Charges.Should().Be(2);
        var approved = _repository.Saved.Single(p => p.Attempt == 2);
        _publisher.Verify(p => p.PublishAsync(new PaymentRejected("A123", PaymentGateway.RejectedReason, 1),
            It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.PublishAsync(new PaymentApproved("A123", approved.Id, 2),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
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
