using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Web.Api.Hubs;
using Shared.Code;

namespace Orders.Tests.Web;

[TestFixture]
public class OrderHubTests
{
    private static readonly Guid Owner = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private Mock<IMemoryBus> _bus = null!;
    private Mock<IGroupManager> _groups = null!;
    private Mock<ISingleClientProxy> _caller = null!;
    private OrderHub _hub = null!;

    [SetUp]
    public void SetUp()
    {
        _bus = new Mock<IMemoryBus>();
        _groups = new Mock<IGroupManager>();
        _caller = new Mock<ISingleClientProxy>();
        var clients = new Mock<IHubCallerClients>();
        clients.SetupGet(c => c.Caller).Returns(_caller.Object);
        var context = new Mock<HubCallerContext>();
        context.SetupGet(c => c.ConnectionId).Returns("conn-1");
        context.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", Owner.ToString()), new Claim("email", "cliente@exemplo.com")], "Bearer")));
        _hub = new OrderHub(_bus.Object) { Context = context.Object, Groups = _groups.Object, Clients = clients.Object };
    }

    // Só responde ao dono: a consulta de outro cliente (ou de número inexistente) devolve nulo.
    private void OrderOf(Guid customerId, string orderNumber, string status)
        => _bus.Setup(b => b.RequestAsync(It.IsAny<Message<OrderPaymentState?>>()))
            .ReturnsAsync((Message<OrderPaymentState?> query) =>
            {
                var q = (GetOrderStatusQuery)query;
                return q.OrderNumber == orderNumber && q.CustomerId == customerId ? new OrderPaymentState(status, 1) : null;
            });

    [Test]
    public async Task Pedido_de_outro_cliente_e_recusado_sem_entrar_no_grupo()
    {
        OrderOf(Guid.NewGuid(), "A123", OrderStatus.Confirmed.Name);

        var watch = () => _hub.WatchOrder("A123");

        (await watch.Should().ThrowAsync<HubException>()).WithMessage(OrderHub.OrderNotFound);
        _groups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _caller.Verify(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_proprio_ja_resolvido_envia_o_resultado()
    {
        OrderOf(Owner, "A123", OrderStatus.Failed.Name);

        await _hub.WatchOrder("A123");

        _groups.Verify(g => g.AddToGroupAsync("conn-1", OrderHub.GroupName("A123"), It.IsAny<CancellationToken>()), Times.Once);
        _caller.Verify(c => c.SendCoreAsync(OrderHub.PaymentStatusChangedMethod,
            It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], new PaymentStatusChanged("A123", false, OrderStatus.Failed.Name, 1))),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedido_proprio_nao_processado_envia_o_status_e_nao_como_recusado()
    {
        OrderOf(Owner, "A123", OrderStatus.NotProcessed.Name);

        await _hub.WatchOrder("A123");

        _caller.Verify(c => c.SendCoreAsync(OrderHub.PaymentStatusChangedMethod,
            It.Is<object?[]>(args => args.Length == 1
                && Equals(args[0], new PaymentStatusChanged("A123", false, OrderStatus.NotProcessed.Name, 1))),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedido_proprio_pendente_entra_no_grupo_sem_resultado()
    {
        OrderOf(Owner, "A123", OrderStatus.Pending.Name);

        await _hub.WatchOrder("A123");

        _groups.Verify(g => g.AddToGroupAsync("conn-1", OrderHub.GroupName("A123"), It.IsAny<CancellationToken>()), Times.Once);
        _caller.Verify(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
