using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Store.Domain.Models.Orders;

namespace Store.Tests.Domains.Models.Orders
{
    [TestFixture, Category("Store-1")]
    public class OrderValidationTests
    {
        [Test]
        public void Validate_order_with_success_no_items()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .Build();
        
            var order = Builder<Order>
                .CreateNew()
                    .With(x => x.Address, address)
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Submitted);
            order.Status.Should().Be(OrderStatus.Submitted);
        }

        [Test]
        public void Validate_order_with_items_success()
        {
            //Arrange's
            var productId = Guid.NewGuid();
            var productName = "fakeNameProduct";
            var unitPrice = 1932;
            var discount = 0;
            var pictureUrl = "urlpicture";
            var units = 1;
        
            var address = Builder<OrderAddress>
                .CreateNew()
                .Build();
        
            var order = Builder<Order>
                .CreateNew()
                    .With(x => x.Address, address)
                    .Do(x => x.AddItem(productId, productName, unitPrice, discount, pictureUrl, units))
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Submitted);
            order.Items.Should().NotBeNull();
            order.Items.Should().HaveCount(1);
        }
        
        [Test]
        public void Invalid_order_customerId_empty()
        {
            //Arrange's
            var order = Builder<Order>
                .CreateNew()
                    .With(x => x.CustomerId, Guid.Empty)
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeFalse();
        }
        
        [Test]
        public void Invalid_order_number_empty()
        {
            //Arrange's
            var order = Builder<Order>
                .CreateNew()
                .With(x => x.OrderNumber, string.Empty)
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeFalse();
        }
        
        [Test]
        public void Invalid_order_number_null()
        {
            //Arrange's
            var order = Builder<Order>
                .CreateNew()
                .With(x => x.OrderNumber, null)
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeFalse();
        }
        
        [TestCase(1)]
        [TestCase(-2)]
        public void Invalid_order_date_not_equal_current_date(int day)
        {
            //Arrange's
            var order = Builder<Order>
                .CreateNew()
                    .With(x => x.OrderDate, DateTime.Now.AddDays(day))
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeFalse();
        }
        
        [Test]
        public void Invalid_order_date_equal_current_date()
        {
            //Arrange's
            var order = Builder<Order>
                .CreateNew()
                .With(x => x.OrderDate, DateTime.Now)
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeFalse();
        }
        
        [Test]
        public void Invalid_order_paymentId_empty()
        {
            //Arrange's
            var order = Builder<Order>
                .CreateNew()
                .With(x => x.PaymentId, Guid.Empty)
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeFalse();
        }
        
        [Test]
        public void Invalid_order_address_null()
        {
            //Arrange's
            var order = Builder<Order>
                .CreateNew()
                .With(x => x.Address, null)
                .Build();
        
            //Act
            var result = order.IsValid();
        
            //Assert's
            result.Should().BeFalse();
        }
    }
}