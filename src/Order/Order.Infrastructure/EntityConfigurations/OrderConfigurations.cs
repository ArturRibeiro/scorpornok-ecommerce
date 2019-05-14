using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Order.Infrastructure.EntityConfigurations
{
    class OrderConfigurations : IEntityTypeConfiguration<Domain.Models.Orders.Order>
    {
        public void Configure(EntityTypeBuilder<Domain.Models.Orders.Order> builder)
        {
            builder.HasKey(cr => cr.Id)
                .HasName("OrderId");
            //builder.Property(cr => cr.Name).IsRequired();
            //builder.Property(cr => cr.Time).IsRequired();
        }
    }
}
