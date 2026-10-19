using Customers.CommandHandlers.Purchases;
using MassTransit;
using Shared.Code.IntegrationEvents;

namespace Customers.Infrastructure.Consumers;

// Consumidor fino: traduz a mensagem do Orders em comando e o entrega ao handler pelo IMemoryBus,
// como os endpoints fazem com as requisições HTTP.
public class OrderPlacedConsumer(IMemoryBus bus) : IConsumer<OrderPlaced>
{
    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        var address = message.ShippingAddress;
        return bus.SendAsync(new RecordPurchaseCommand(message.OrderNumber, message.CustomerId, message.Email,
            message.FullName, message.Phone, message.Cpf,
            new AddressData(address.Street, address.Number, address.Complement, address.District, address.City,
                address.State, address.ZipCode),
            message.OrderDate));
    }
}
