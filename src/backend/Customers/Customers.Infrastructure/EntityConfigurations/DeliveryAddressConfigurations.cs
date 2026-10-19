namespace Customers.Infrastructure.EntityConfigurations;

[ExcludeFromCodeCoverage]
class DeliveryAddressConfigurations : IEntityTypeConfiguration<DeliveryAddress>
{
    public void Configure(EntityTypeBuilder<DeliveryAddress> builder)
    {
        builder.ToTable("DeliveryAddresses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.CustomerId).HasColumnName("CustomerId").IsRequired();
        builder.HasIndex(x => x.CustomerId);
        // Um registro por pedido: a mesma mensagem entregue duas vezes falha a segunda gravação.
        builder.Property(x => x.OrderNumber).HasColumnName("OrderNumber").HasMaxLength(16).IsRequired();
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.Property(x => x.OrderDate).HasColumnName("OrderDate").IsRequired();
        builder.OwnsOne(x => x.Address, AddressColumns.Map);
        builder.Navigation(x => x.Address).IsRequired();
        builder.Ignore(x => x.Errors);
    }
}
