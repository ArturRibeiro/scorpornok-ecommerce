using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Models.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Infrastructure.EntityConfigurations
{
    class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> orderItemConfiguration)
        {
            orderItemConfiguration
                .Property<int>("OrderId")
                .IsRequired();

            orderItemConfiguration
                .Property<decimal>("Discount")
                .IsRequired();

            orderItemConfiguration
                .Property<int>("ProductId")
                .IsRequired();

            orderItemConfiguration
                .Property<string>("ProductName")
                .IsRequired();

            orderItemConfiguration
                .Property<decimal>("UnitPrice")
                .IsRequired();
        }
    }
}
