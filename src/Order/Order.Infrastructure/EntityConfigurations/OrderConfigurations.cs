using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Order.Infrastructure.EntityConfigurations
{
    class OrderConfigurations : IEntityTypeConfiguration<Domain.Models.Orders.Order>
    {
        public void Configure(EntityTypeBuilder<Domain.Models.Orders.Order> builder)
        {
            builder.HasKey(cr => cr.Id)
                .HasName("OrderId");

            builder.Property(x => x.CustomerId)
                .HasColumnName("CustomerId")
                .IsRequired();

            builder.Property(x => x.OrderNumber)
                .HasColumnName("OrderNumber")
                .IsRequired();

            builder.Property(x => x.OrderDate)
                .HasColumnName("OrderDate")
                .IsRequired();

            builder.Property(x => x.PaymentId)
                .HasColumnName("PaymentId")
                .IsRequired();

            //Address Value Object
            //https://docs.microsoft.com/pt-br/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
            builder.OwnsOne(o => o.Address);

            //builder.HasOne<Domain.Models.Orders.OrderAddress>()
            //    .WithMany()
            //    .IsRequired()
            //    .HasForeignKey("AddressId");
        }
    }
}
