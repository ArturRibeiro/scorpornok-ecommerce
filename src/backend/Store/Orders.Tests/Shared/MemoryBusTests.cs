namespace Orders.Tests.Shared;

[TestFixture]
public class MemoryBusTests
{
    private sealed record PingCommand(string Text) : Message<string>;

    [Test]
    public async Task RequestAsync_deve_devolver_a_resposta_do_handler()
    {
        var command = new PingCommand("ping");
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<IRequest<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("pong");
        var bus = new MemoryBus(mediator.Object);

        var response = await bus.RequestAsync(command);

        response.Should().Be("pong");
        mediator.Verify(m => m.Send(It.Is<IRequest<string>>(r => r == command), It.IsAny<CancellationToken>()), Times.Once);
    }
}
