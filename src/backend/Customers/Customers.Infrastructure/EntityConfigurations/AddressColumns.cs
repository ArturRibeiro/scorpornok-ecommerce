namespace Customers.Infrastructure.EntityConfigurations;

[ExcludeFromCodeCoverage]
static class AddressColumns
{
    // Mesmas colunas no cadastro e no histórico (owned type na tabela do dono).
    public static void Map<TOwner>(OwnedNavigationBuilder<TOwner, Address> address) where TOwner : class
    {
        address.Property(p => p.Street).HasColumnName("AddressStreet").HasMaxLength(200);
        address.Property(p => p.Number).HasColumnName("AddressNumber").HasMaxLength(20);
        address.Property(p => p.Complement).HasColumnName("AddressComplement").HasMaxLength(100);
        address.Property(p => p.District).HasColumnName("AddressDistrict").HasMaxLength(100);
        address.Property(p => p.City).HasColumnName("AddressCity").HasMaxLength(100);
        address.Property(p => p.State).HasColumnName("AddressState").HasMaxLength(2);
        address.Property(p => p.ZipCode).HasColumnName("AddressZipCode").HasMaxLength(8);
    }
}
