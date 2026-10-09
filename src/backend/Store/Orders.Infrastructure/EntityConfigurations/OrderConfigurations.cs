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

            builder.Property(x => x.Email)
                .HasColumnName("Email")
                .IsRequired();

            builder.Property(x => x.OrderNumber)
                .HasColumnName("Number")
                .IsRequired();

            builder.Property(x => x.OrderDate)
                .HasColumnName("Date")
                .IsRequired();

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

            // 1:1 em tabela própria: a chave do OrderPayment é o Id do pedido (OrderId).
            builder.OwnsOne(o => o.Payment, p =>
            {
                p.ToTable("OrderPayment");
                p.WithOwner().HasForeignKey("OrderId");
                p.HasKey("OrderId");
                p.Property(x => x.PaymentId).HasColumnName("PaymentId");
                p.Property(x => x.EmailSentAt).HasColumnName("EmailSentAt");
            });
            builder.Navigation(o => o.Payment).IsRequired();

            builder.OwnsOne(o => o.Status, r =>
            {
                r.ToTable("Orders");
                r.Property(p => p.Code).HasColumnName("StatusCode");
                r.Property(p => p.Name).HasColumnName("StatusName");
                r.Property(p => p.Description).HasColumnName("StatusDescription");
            });
            
        }
    }
}
