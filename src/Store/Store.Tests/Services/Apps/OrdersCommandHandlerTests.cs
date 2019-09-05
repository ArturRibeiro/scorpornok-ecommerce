using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shared.Code.Bus;
using Shared.Code.Models;
using Shared.Code.Notifications;
using Store.Domain.Models.Orders;
using Store.Web.Api.App.CommandHandlers;
using Store.Web.Api.App.Commands;

namespace Store.Tests.Services.Apps
{
    [TestFixture]
    public class OrdersCommandHandlerTests
    {
        private static CreateOrderCommand[] CreateOrderCommand = new CreateOrderCommand[]
        {
            new CreateOrderCommand()
            {
                Address = null,
                Items = Builder<OrderItemMessageResponse>.CreateListOfSize(1).Build(),
                UserId = Guid.NewGuid()
            },
            new CreateOrderCommand()
            {
                Address = new OrderAddressMessageResponse(),
                Items = Builder<OrderItemMessageResponse>.CreateListOfSize(1).Build(),
                UserId = Guid.NewGuid()
            },
            new CreateOrderCommand()
            {
                Address = Builder<OrderAddressMessageResponse>.CreateNew().Build(),
                Items = null,
                UserId = Guid.NewGuid()
            },
            new CreateOrderCommand()
            {
                Address = Builder<OrderAddressMessageResponse>.CreateNew().Build(),
                Items = new List<OrderItemMessageResponse>(),
                UserId = Guid.NewGuid()
            },
            new CreateOrderCommand()
            {
                Address = Builder<OrderAddressMessageResponse>.CreateNew().Build(),
                Items = new List<OrderItemMessageResponse>() { new OrderItemMessageResponse()},
                UserId = Guid.NewGuid()
            }
        };

        [Test, TestCaseSource("CreateOrderCommand")]
        public async Task Create_order_command_Invalid_contract_items(CreateOrderCommand command)
        {
            //Arrange's
            var uow = new Mock<IUnitOfWork>();
            var mediator = new Mock<IMediatorHandler>();
            var orderRepository = new Mock<IOrderRepository>();
            var notificationHandler = new Mock<DomainNotificationHandler>();
            var logger = new Mock<ILogger<CreateOrderCommand>>();
            var handler = new OrdersCommandHandler(mediator.Object, notificationHandler.Object, orderRepository.Object, logger.Object);

            //Stub's
            orderRepository.SetupGet(x => x.UnitOfWork).Returns(uow.Object);

            //Act
            await handler.Handle(command, CancellationToken.None);

            //Assert's
            orderRepository.Verify(x => x.Save(It.Is<Order>(it => !string.IsNullOrEmpty(it.OrderNumber))), Times.Never);
            orderRepository.Verify(x => x.UnitOfWork.SaveEntitiesAsync(CancellationToken.None), Times.Never);
            mediator.Verify(x => x.RaiseEvent(It.IsAny<DomainNotification>()));
        }

        [Test]
        public async Task Create_order_command()
        {
            //Arrange's
            var uow = new Mock<IUnitOfWork>();
            var mediator = new Mock<IMediatorHandler>();
            var orderRepository = new Mock<IOrderRepository>();
            var notificationHandler = new Mock<DomainNotificationHandler>();
            var logger = new Mock<ILogger<CreateOrderCommand>>();
            var handler = new OrdersCommandHandler(mediator.Object, notificationHandler.Object, orderRepository.Object, logger.Object);

            //Stub's
            orderRepository.SetupGet(x => x.UnitOfWork).Returns(uow.Object);

            var command = new CreateOrderCommand()
            {
                Address = Builder<OrderAddressMessageResponse>.CreateNew().Build(),
                Items = Builder<OrderItemMessageResponse>.CreateListOfSize(1).Build().ToArray(),
                UserId = Guid.NewGuid()
            };

            //Act
            await handler.Handle(command, CancellationToken.None);

            //Assert's
            orderRepository.Verify(x => x.Save(It.Is<Order>(it => !string.IsNullOrEmpty(it.OrderNumber))), Times.Once);
            orderRepository.Verify(x => x.UnitOfWork.SaveEntitiesAsync(CancellationToken.None), Times.Once);
            mediator.Verify(x => x.RaiseEvent(It.IsAny<DomainNotification>()), Times.Never());
        }

        [Test]
        public async Task Create_order_with_errors_address()
        {
            //Arrange's
            var uow = new Mock<IUnitOfWork>();
            var mediator = new Mock<IMediatorHandler>();
            var orderRepository = new Mock<IOrderRepository>();
            var notificationHandler = new Mock<DomainNotificationHandler>();
            var logger = new Mock<ILogger<CreateOrderCommand>>();
            var handler = new OrdersCommandHandler(mediator.Object, notificationHandler.Object, orderRepository.Object, logger.Object);

            //Stub's
            orderRepository.SetupGet(x => x.UnitOfWork).Returns(uow.Object);

            var command = new CreateOrderCommand()
            {
                Address = new OrderAddressMessageResponse(),
                Items = Builder<OrderItemMessageResponse>.CreateListOfSize(1).Build(),
                UserId = Guid.NewGuid()
            };

            //Act
            await handler.Handle(command, CancellationToken.None);

            //Assert's
            orderRepository.Verify(x => x.Save(It.Is<Order>(it => !string.IsNullOrEmpty(it.OrderNumber))), Times.Never);
            orderRepository.Verify(x => x.UnitOfWork.SaveEntitiesAsync(CancellationToken.None), Times.Never);
            mediator.Verify(x => x.RaiseEvent(It.IsAny<DomainNotification>()));
        }

        [Test]
        public async Task Create_order_with_errors_order_items()
        {
            //Arrange's
            var uow = new Mock<IUnitOfWork>();
            var mediator = new Mock<IMediatorHandler>();
            var orderRepository = new Mock<IOrderRepository>();
            var notificationHandler = new Mock<DomainNotificationHandler>();
            var logger = new Mock<ILogger<CreateOrderCommand>>();
            var handler = new OrdersCommandHandler(mediator.Object, notificationHandler.Object, orderRepository.Object, logger.Object);

            //Stub's
            orderRepository.SetupGet(x => x.UnitOfWork).Returns(uow.Object);

            var command = new CreateOrderCommand()
            {
                Address = Builder<OrderAddressMessageResponse>.CreateNew().Build(),
                Items = new List<OrderItemMessageResponse>() { new OrderItemMessageResponse() },
                UserId = Guid.NewGuid()
            };

            //Act
            await handler.Handle(command, CancellationToken.None);

            //Assert's
            orderRepository.Verify(x => x.Save(It.Is<Order>(it => !string.IsNullOrEmpty(it.OrderNumber))), Times.Never);
            orderRepository.Verify(x => x.UnitOfWork.SaveEntitiesAsync(CancellationToken.None), Times.Never);
            mediator.Verify(x => x.RaiseEvent(It.IsAny<DomainNotification>()));
        }

    }
}