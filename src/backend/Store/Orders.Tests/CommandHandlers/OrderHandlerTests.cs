namespace Orders.Tests.CommandHandlers;

[TestFixture]
public class OrderHandlerTests
{
    private Mock<IOrderRepository> _repository = null!;
    private Mock<IIntegrationEventPublisher> _publisher = null!;
    private Mock<ICatalogProducts> _catalog = null!;
    // O que o Catalog conhece: os itens dos testes usam os mesmos preços, salvo quando o teste adultera.
    private Dictionary<int, CatalogProduct> _catalogProducts = null!;
    private OrderHandler _handler = null!;
    // Registra a ordem das chamadas: begin/commit da transação, add (Save), save (SaveEntitiesAsync), publish.
    private List<string> _calls = null!;

    [SetUp]
    public void SetUp()
    {
        _calls = [];
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save")).ReturnsAsync(true);
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(unitOfWork.Object);
        _repository.Setup(r => r.Save(It.IsAny<Order>()))
            .Callback(() => _calls.Add("add")).Returns((Order order) => order);
        _repository.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task> work, CancellationToken _) =>
            {
                _calls.Add("begin");
                await work();
                _calls.Add("commit");
            });
        _publisher = new Mock<IIntegrationEventPublisher>();
        _publisher.Setup(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("publish")).Returns(Task.CompletedTask);
        _publisher.Setup(p => p.PublishAsync(It.IsAny<OrderPlaced>(), It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("placed")).Returns(Task.CompletedTask);
        _catalogProducts = new()
        {
            [42] = new CatalogProduct(42, "Camiseta", "/catalog-imgs/42.jpg", 10m),
            [7] = new CatalogProduct(7, "Boné", "/catalog-imgs/7.jpg", 5m)
        };
        _catalog = new Mock<ICatalogProducts>();
        _catalog.Setup(c => c.GetAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<int> ids, CancellationToken _) =>
                ids.Where(_catalogProducts.ContainsKey).Distinct().ToDictionary(id => id, id => _catalogProducts[id]));
        _handler = new OrderHandler(_repository.Object, _publisher.Object, _catalog.Object,
            NullLogger<OrderHandler>.Instance);
    }

    private static OrderItemMessageResponse Item(int productId, decimal unitPrice, int units)
        => new()
        {
            ProductId = productId,
            ProductName = $"Produto {productId}",
            PictureUrl = "http://img",
            UnitPrice = unitPrice,
            Units = units
        };

    private static PaymentMethod Payment() => new("tok_abc123", 3);

    private static OrderBuyerMessage Buyer(string? cpf = "529.982.247-25")
        => new("Maria Souza", "(21) 99999-8888", cpf);

    private static CreateCommand Command(Guid userId, string city = "São Paulo", params OrderItemMessageResponse[] items)
        => new(
            userId,
            new OrderAddressMessageResponse
            {
                Street = "Rua A", Number = "10", Complement = "Apto 1", District = "Centro", City = city, State = "SP",
                ZipCode = "01000-000"
            },
            items,
            Payment(),
            "cliente@exemplo.com",
            Buyer());

    [Test]
    public async Task Pedido_valido_deve_ser_salvo_para_o_cliente_informado()
    {
        var userId = Guid.NewGuid();
        Order? saved = null;
        _repository.Setup(r => r.Save(It.IsAny<Order>())).Callback((Order o) => saved = o).Returns((Order o) => o);

        var result = await _handler.Handle(
            Command(userId, items: [Item(42, 10m, 2), Item(7, 5m, 1)]), CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Created);
        result.OrderNumber.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(OrderStatus.Pending.Name);
        result.Total.Should().Be(25m);
        result.Errors.Should().BeEmpty();
        saved.Should().NotBeNull();
        saved!.CustomerId.Should().Be(userId);
        saved.Email.Should().Be("cliente@exemplo.com");
        saved.Items.Select(i => i.ProductId).Should().BeEquivalentTo([42, 7]);
    }

    [Test]
    public async Task Pedido_invalido_nao_deve_ser_salvo()
    {
        var result = await _handler.Handle(Command(Guid.NewGuid(), city: "", items: []), CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Invalid);
        result.OrderNumber.Should().BeNull();
        result.Errors.Should().Contain("O pedido não tem itens.");
        result.Errors.Should().Contain(error => error == "Informe a cidade.");
        _repository.Verify(r => r.Save(It.IsAny<Order>()), Times.Never);
    }

    [Test]
    public async Task Pedido_sem_endereco_e_sem_itens_deve_devolver_erros_sem_excecao()
    {
        var command = new CreateCommand(Guid.NewGuid(), null!, null!, Payment());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Invalid);
        result.Errors.Should().NotBeEmpty();
        _repository.Verify(r => r.Save(It.IsAny<Order>()), Times.Never);
    }

    [Test]
    public async Task Pedido_valido_deve_solicitar_o_pagamento_com_o_total_do_pedido()
    {
        var userId = Guid.NewGuid();

        var result = await _handler.Handle(
            Command(userId, items: [Item(42, 10m, 2), Item(7, 5m, 1)]), CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(
            It.Is<PaymentRequested>(m =>
                m.OrderNumber == result.OrderNumber && m.CustomerId == userId && m.Amount == 25m
                && m.Installments == 3 && m.CardToken == "tok_abc123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedido_invalido_nao_deve_solicitar_pagamento()
    {
        await _handler.Handle(Command(Guid.NewGuid(), items: []), CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_valido_deve_publicar_antes_do_ultimo_save_dentro_da_transacao()
    {
        await _handler.Handle(Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]), CancellationToken.None);

        // O primeiro save gera o Id do pedido; as mensagens do outbox só são gravadas no save seguinte.
        _calls.Should().Equal("begin", "add", "save", "placed", "publish", "save", "commit");
    }

    [Test]
    public async Task Falha_ao_publicar_deve_desfazer_o_registro_do_pedido()
    {
        _publisher.Setup(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("falha ao gravar no outbox"));

        var act = () => _handler.Handle(Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _calls.Should().NotContain("commit");
    }

    [Test]
    public async Task Pedido_sem_cartao_deve_ser_salvo_sem_solicitar_pagamento()
    {
        var command = Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]) with { Payment = null };

        var result = await _handler.Handle(command, CancellationToken.None);

        // Sem cartão não há cobrança, mas o Customers recebe o pedido registrado.
        result.Outcome.Should().Be(CreateOrderOutcome.Created);
        _calls.Should().Equal("begin", "add", "save", "placed", "save", "commit");
    }

    [Test]
    public async Task Pedido_invalido_nao_deve_abrir_transacao()
    {
        await _handler.Handle(Command(Guid.NewGuid(), items: []), CancellationToken.None);

        _repository.Verify(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Preco_nome_e_foto_devem_vir_do_Catalog_e_nao_do_pedido()
    {
        Order? saved = null;
        _repository.Setup(r => r.Save(It.IsAny<Order>())).Callback((Order o) => saved = o).Returns((Order o) => o);
        var tampered = Item(42, 0.01m, 2);
        tampered.ProductName = "Brinde";
        tampered.PictureUrl = "http://outra-imagem";

        var result = await _handler.Handle(Command(Guid.NewGuid(), items: [tampered]), CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Created);
        result.Total.Should().Be(20m);
        var item = saved!.Items.Single();
        item.UnitPrice.Should().Be(10m);
        item.ProductName.Should().Be("Camiseta");
        item.PictureUrl.Should().Be("/catalog-imgs/42.jpg");
        _publisher.Verify(p => p.PublishAsync(It.Is<PaymentRequested>(m => m.Amount == 20m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Produto_inexistente_no_Catalog_deve_invalidar_sem_abrir_transacao()
    {
        var result = await _handler.Handle(
            Command(Guid.NewGuid(), items: [Item(42, 10m, 1), Item(999, 1m, 1)]), CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Invalid);
        result.Errors.Should().Contain("Produto 999 não encontrado.");
        _repository.Verify(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Catalog_indisponivel_deve_devolver_Unavailable_sem_gravar_nem_publicar()
    {
        _catalog.Setup(c => c.GetAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CatalogUnavailableException("fora do ar"));

        var result = await _handler.Handle(Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]), CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Unavailable);
        result.OrderNumber.Should().BeNull();
        _repository.Verify(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_sem_itens_nao_deve_consultar_o_Catalog()
    {
        await _handler.Handle(Command(Guid.NewGuid(), items: []), CancellationToken.None);

        _catalog.Verify(c => c.GetAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Pagamento_sem_token_deve_ser_recusado_sem_abrir_transacao()
    {
        var command = Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]) with { Payment = new PaymentMethod(null) };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Invalid);
        result.Errors.Should().Contain("Informe o cartão.");
        _repository.Verify(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_valido_deve_publicar_OrderPlaced_com_o_comprador_e_o_endereco_do_pedido()
    {
        var userId = Guid.NewGuid();
        OrderPlaced? placed = null;
        _publisher.Setup(p => p.PublishAsync(It.IsAny<OrderPlaced>(), It.IsAny<CancellationToken>()))
            .Callback((OrderPlaced m, CancellationToken _) => placed = m).Returns(Task.CompletedTask);

        var result = await _handler.Handle(Command(userId, city: "Rio de Janeiro", items: [Item(42, 10m, 1)]),
            CancellationToken.None);

        placed.Should().NotBeNull();
        placed!.OrderNumber.Should().Be(result.OrderNumber);
        placed.CustomerId.Should().Be(userId);
        placed.Email.Should().Be("cliente@exemplo.com");
        placed.FullName.Should().Be("Maria Souza");
        placed.Phone.Should().Be("21999998888");
        placed.Cpf.Should().Be("52998224725");
        placed.ShippingAddress.Should().Be(
            new ShippingAddress("Rua A", "10", "Apto 1", "Centro", "Rio de Janeiro", "SP", "01000000"));
    }

    [Test]
    public async Task Falha_ao_gravar_nao_deve_publicar_OrderPlaced()
    {
        _repository.Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("banco fora"));

        var act = () => _handler.Handle(Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _publisher.Verify(p => p.PublishAsync(It.IsAny<OrderPlaced>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Cpf_invalido_do_comprador_deve_recusar_sem_abrir_transacao()
    {
        var command = Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]) with { Customer = Buyer(cpf: "111.111.111-11") };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Invalid);
        result.Errors.Should().Contain("CPF inválido.");
        _repository.Verify(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Pedido_sem_comprador_deve_pedir_nome_telefone_e_cpf()
    {
        var command = Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]) with { Customer = null };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Outcome.Should().Be(CreateOrderOutcome.Invalid);
        result.Errors.Should().Contain(["Informe o nome completo.", "Informe o telefone.", "Informe o CPF."]);
    }

    [Test]
    public void Corpo_com_country_e_comprador_deve_ser_lido_pelo_comando()
    {
        var json = """
            {"address":{"street":"Rua A","number":"10","district":"Centro","city":"Rio","state":"RJ","country":"Portugal","zipCode":"20040-020"},
             "items":[],"customer":{"fullName":"Maria Souza","phone":"(21) 99999-8888","cpf":"529.982.247-25"}}
            """;

        var command = System.Text.Json.JsonSerializer.Deserialize<CreateCommand>(json,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;

        command.Customer.Should().Be(new OrderBuyerMessage("Maria Souza", "(21) 99999-8888", "529.982.247-25"));
        command.Address.Number.Should().Be("10");
        command.Address.District.Should().Be("Centro");
    }

    [Test]
    public void Corpo_com_numero_e_CVV_nao_deve_levar_o_cartao_para_o_comando()
    {
        // Como o POST /createOrder desserializa o corpo (JsonSerializerDefaults.Web): o cartão antigo
        // ("card") é ignorado, e do "payment" só existem token e parcelas.
        var json = """
            {"address":{},"items":[],
             "card":{"cardNumber":"4111111111111111","cvv":"123"},
             "payment":{"cardToken":"tok_abc123","installments":2,"cardNumber":"4111111111111111","cvv":"123"}}
            """;

        var command = System.Text.Json.JsonSerializer.Deserialize<CreateCommand>(json,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;

        command.Payment.Should().Be(new PaymentMethod("tok_abc123", 2));
        System.Text.Json.JsonSerializer.Serialize(command).Should().NotContain("4111111111111111").And.NotContain("\"123\"");
    }
}
