namespace Payments.Infrastructure.Mappings;

public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.OrderId).IsRequired();

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(32);
        builder.HasIndex(x => x.OrderNumber);

        builder.Property(x => x.Attempt).IsRequired();

        // Um pagamento por tentativa: a solicitação repetida, mesmo simultânea, não gera outro
        // registro (a segunda gravação viola o índice e a retentativa encontra o primeiro).
        builder.Property(x => x.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(48);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();

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

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);
        builder.Ignore(x => x.Approved);
        builder.Property(x => x.Reason).HasMaxLength(250);
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.Property(x => x.ProcessedAt);
    }
}
