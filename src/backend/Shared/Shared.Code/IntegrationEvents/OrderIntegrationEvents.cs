namespace Shared.Code.IntegrationEvents;

/// <summary>
/// Orders → Customers: pedido registrado. Sai pelo outbox junto com o pedido, com ou sem cartão. O
/// Customers guarda o endereço no histórico do cliente e, na primeira compra, cria o cadastro com o
/// comprador e esse endereço; um cadastro existente não muda.
/// </summary>
/// <param name="CustomerId">Conta do cliente (<c>sub</c> do Keycloak).</param>
/// <param name="FullName">Nome do comprador, informado no checkout (não fica guardado no pedido).</param>
/// <param name="Phone">Telefone do comprador, só com dígitos.</param>
/// <param name="Cpf">CPF do comprador, só com dígitos.</param>
/// <param name="ShippingAddress">Endereço de entrega do pedido, que pode ser diferente do cadastro.</param>
public record OrderPlaced(
    string OrderNumber,
    Guid CustomerId,
    string Email,
    string FullName,
    string Phone,
    string Cpf,
    ShippingAddress ShippingAddress,
    DateTime OrderDate);

/// <summary>Endereço de entrega no formato do cadastro, sempre no Brasil (UF e CEP só com dígitos).</summary>
public record ShippingAddress(
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode);
