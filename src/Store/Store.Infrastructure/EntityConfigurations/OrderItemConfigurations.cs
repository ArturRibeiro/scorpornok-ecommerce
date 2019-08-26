using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Models.Orders;

namespace Store.Infrastructure.EntityConfigurations
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
