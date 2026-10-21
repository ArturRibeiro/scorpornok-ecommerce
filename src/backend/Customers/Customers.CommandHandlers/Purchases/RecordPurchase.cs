namespace Customers.CommandHandlers.Purchases;

/// <summary>Pedido registrado no Orders (mensagem <c>OrderPlaced</c>): comprador e endereço de entrega.</summary>
public record RecordPurchaseCommand(
    string OrderNumber,
    Guid CustomerId,
    string Email,
    string FullName,
    string Phone,
    string Cpf,
    AddressData ShippingAddress,
    DateTime OrderDate)
    : CommandBase;

/// <summary>
/// Guarda o endereço de entrega no histórico do cliente e, na primeira compra, cria o cadastro com o
/// comprador e esse endereço. Um cadastro existente nunca muda pela compra. Tudo numa gravação só.
/// </summary>
public class RecordPurchaseHandler(ICustomerRepository repository, ILogger<RecordPurchaseHandler> logger)
    : IRequestHandler<RecordPurchaseCommand>
{
    public async Task Handle(RecordPurchaseCommand command, CancellationToken cancellationToken)
    {
        OrderTelemetry.Tag(command.OrderNumber);

        // Mensagem repetida: o pedido já foi registrado (histórico e cadastro saem na mesma gravação).
        // Entregas simultâneas esbarram no índice único do pedido, e a retentativa cai aqui.
        if (await repository.HasDeliveryAddressAsync(command.OrderNumber, cancellationToken))
            return;

        repository.AddDeliveryAddress(new DeliveryAddress(command.CustomerId, command.OrderNumber,
            command.ShippingAddress.ToAddress(), command.OrderDate));

        // Duas primeiras compras ao mesmo tempo esbarram na chave (sub); a retentativa encontra o cadastro.
        if (await repository.GetAsync(command.CustomerId, cancellationToken) is null)
        {
            var customer = new Customer(command.CustomerId, command.Email);
            // Um Address próprio: o endereço é owned no EF, e a mesma instância no histórico e no cadastro
            // seria rastreada como uma só ("The same entity is being tracked as different entity types").
            customer.Update(command.Email, command.FullName, command.Phone, command.Cpf,
                command.ShippingAddress.ToAddress());
            // O Orders valida com as mesmas regras; se algo escapar, o histórico fica e o cadastro não nasce.
            if (customer.IsValid())
                repository.Add(customer);
            else
                logger.LogWarning("Pedido {OrderNumber}: dados do comprador inválidos ({Errors}); cadastro não criado.",
                    command.OrderNumber, string.Join(" ", customer.Errors));
        }

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
