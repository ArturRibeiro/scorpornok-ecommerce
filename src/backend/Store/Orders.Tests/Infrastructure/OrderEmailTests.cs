using MailKit.Security;
using Orders.CommandHandlers.Orders;
using Orders.Infrastructure.Email;

namespace Orders.Tests.Infrastructure;

[TestFixture]
public class OrderEmailTests
{
    private static Order ResolvedOrder(bool approved)
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@exemplo.com");
        order.AddProduct([OrderItem.Create(42, "Produto", 10m, 0m, "http://img", 2),
            OrderItem.Create(7, "Outro", 5m, 0m, "http://img", 1)]);
        if (approved) order.ConfirmPayment(Guid.NewGuid());
        else order.FailPayment();
        return order;
    }

    [Test]
    public void Email_de_pagamento_aprovado_deve_confirmar_o_pedido_com_o_total()
    {
        var order = ResolvedOrder(approved: true);

        var email = OrderPaymentEmail.For(order);

        email.Subject.Should().Be($"Order {order.OrderNumber} confirmed");
        email.Body.Should().Contain($"order {order.OrderNumber} is confirmed").And.Contain("Total: $25.00");
    }

    [Test]
    public void Email_de_pagamento_recusado_deve_informar_a_recusa_com_o_total()
    {
        var order = ResolvedOrder(approved: false);

        var email = OrderPaymentEmail.For(order);

        email.Subject.Should().Be($"Payment declined for order {order.OrderNumber}");
        email.Body.Should().Contain("Your card was declined").And.Contain("Total: $25.00");
    }

    [Test]
    public void Mailpit_deve_conectar_sem_TLS_e_sem_autenticacao()
    {
        var smtp = new SmtpOptions { Host = "mailpit", Port = 1025 };

        smtp.SocketOptions.Should().Be(SecureSocketOptions.None);
        smtp.RequiresAuthentication.Should().BeFalse();
    }

    [Test]
    public void Provedor_real_deve_usar_STARTTLS_e_autenticacao()
    {
        var smtp = new SmtpOptions
        {
            Host = "smtp.gmail.com", Port = 587, UseStartTls = true,
            Username = "voce@gmail.com", Password = "senha-de-app"
        };

        smtp.SocketOptions.Should().Be(SecureSocketOptions.StartTls);
        smtp.RequiresAuthentication.Should().BeTrue();
    }
}
