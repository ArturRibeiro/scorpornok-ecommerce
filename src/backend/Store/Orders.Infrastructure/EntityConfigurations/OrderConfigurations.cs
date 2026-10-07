namespace Orders.Infrastructure.EntityConfigurations
{
    [ExcludeFromCodeCoverage]
    class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(cr => cr.Id);
            
            builder.Property(cr => cr.Id)
                .HasColumnName("Id");

            builder.Property(x => x.CustomerId)
                .HasColumnName("CustomerId")
                .IsRequired();

            builder.Property(x => x.OrderNumber)
                .HasColumnName("Number")
                .IsRequired();

            builder.Property(x => x.OrderDate)
                .HasColumnName("Date")
                .IsRequired();

            builder.Property(x => x.PaymentId)
                .HasColumnName("PaymentId");
            
            builder.Property(x => x.Total)
                .HasColumnName("Total")
                .IsRequired();

            builder.OwnsOne(o => o.Address, r =>
            {
                r.Property(p => p.Street).HasColumnName("AddressStreet");
                r.Property(p => p.City).HasColumnName("AddressCity");
                r.Property(p => p.State).HasColumnName("AddressState");
                r.Property(p => p.Country).HasColumnName("AddressCountry");
                r.Property(p => p.ZipCode).HasColumnName("AddressZipCode");
            });

            builder.OwnsOne(o => o.Status, r =>
            {
                r.ToTable("Orders");
                r.Property(p => p.Code).HasColumnName("StatusCode");
                r.Property(p => p.Name).HasColumnName("StatusName");
                r.Property(p => p.Description).HasColumnName("StatusDescription");
            });
            
            builder.HasOne(o => o.PaymentMethod);
        }
    }
}
