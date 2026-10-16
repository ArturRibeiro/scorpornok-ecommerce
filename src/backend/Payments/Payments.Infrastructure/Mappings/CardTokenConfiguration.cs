namespace Payments.Infrastructure.Mappings;

public class CardTokenConfiguration : IEntityTypeConfiguration<CardToken>
{
    public void Configure(EntityTypeBuilder<CardToken> builder)
    {
        // Só o que a cobrança simulada precisa: nada de número completo nem CVV.
        builder.ToTable("CardTokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(40).ValueGeneratedNever();
        builder.Property(x => x.CardHolderName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Last4).IsRequired().HasMaxLength(4);
        builder.Property(x => x.ExpirationMonth).IsRequired().HasMaxLength(2);
        builder.Property(x => x.ExpirationYear).IsRequired().HasMaxLength(4);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        // Uso único também sob concorrência: o UPDATE exige o UsedAt que foi lido (nulo), então duas
        // cobranças simultâneas com o mesmo token não passam juntas. A segunda falha, o SaveChanges
        // desfaz também o pagamento dela, e a retentativa encontra o token usado e recusa.
        builder.Property(x => x.UsedAt).IsConcurrencyToken();
    }
}
