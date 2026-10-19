namespace Customers.Infrastructure.EntityConfigurations;

[ExcludeFromCodeCoverage]
class CustomerConfigurations : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        // A chave é o sub do Keycloak: duas primeiras compras simultâneas esbarram nela.
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(254).IsRequired();
        builder.Property(x => x.FullName).HasColumnName("FullName").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Phone).HasColumnName("Phone").HasMaxLength(11).IsRequired();
        builder.Property(x => x.Cpf).HasColumnName("Cpf").HasMaxLength(11).IsRequired();
        builder.OwnsOne(x => x.Address, AddressColumns.Map);
        builder.Ignore(x => x.Errors);
    }
}
