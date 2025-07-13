namespace Orders.Infrastructure.EntityConfigurations;

public class CreditCardPaymentConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("CreditCardPayments");

        builder.HasKey(x => x.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.OrderId).IsRequired();
        
        builder.Property(c => c.CardHolderName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.CardNumber)
            .IsRequired()
            .HasMaxLength(20); // limite prático para PAN mascarado ou token

        builder.Property(c => c.ExpirationMonth)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(c => c.ExpirationYear)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(c => c.Cvv)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(c => c.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Installments)
            .IsRequired();
    }
}