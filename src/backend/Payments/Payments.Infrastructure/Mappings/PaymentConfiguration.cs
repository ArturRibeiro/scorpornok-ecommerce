namespace Payments.Infrastructure.Mappings;

public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.OrderId).IsRequired();

        // Um pagamento por pedido: a solicitação repetida não pode gerar outro registro.
        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(32);
        builder.HasIndex(x => x.OrderNumber).IsUnique();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Installments).IsRequired();

        builder.Property(x => x.CardHolderName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.CardLast4)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.Approved).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(250);
        builder.Property(x => x.ProcessedAt).IsRequired();
    }
}
