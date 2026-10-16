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

        email.Subject.Should().Be($"Pedido {order.OrderNumber} confirmado");
        email.Body.Should().Contain($"o pedido {order.OrderNumber} está confirmado").And.Contain("Total: R$ 25,00");
    }

    [Test]
    public void Email_de_pagamento_recusado_deve_informar_a_recusa_com_o_total()
    {
        var order = ResolvedOrder(approved: false);

        var email = OrderPaymentEmail.For(order);

        email.Subject.Should().Be($"Pagamento recusado no pedido {order.OrderNumber}");
        email.Body.Should().Contain("Seu cartão foi recusado").And.Contain("Total: R$ 25,00");
    }

    [Test]
    public void Email_de_pagamento_nao_processado_deve_dizer_que_nada_foi_cobrado()
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@exemplo.com");
        order.AddProduct([OrderItem.Create(42, "Produto", 10m, 0m, "http://img", 2),
            OrderItem.Create(7, "Outro", 5m, 0m, "http://img", 1)]);
        order.MarkPaymentNotProcessed();

        var email = OrderPaymentEmail.For(order);

        email.Subject.Should().Be($"Pagamento não processado no pedido {order.OrderNumber}");
        email.Body.Should().Contain("nada foi cobrado").And.Contain("refazer a compra").And.Contain("Total: R$ 25,00");
        email.Body.Should().NotContain("recusado");
    }

    [Test]
    public void Total_do_email_deve_usar_o_formato_brasileiro()
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@exemplo.com");
        order.AddProduct([OrderItem.Create(42, "Produto", 1299.99m, 0m, "http://img", 1)]);
        order.ConfirmPayment(Guid.NewGuid());

        OrderPaymentEmail.For(order).Body.Should().Contain("Total: R$ 1.299,99");
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
