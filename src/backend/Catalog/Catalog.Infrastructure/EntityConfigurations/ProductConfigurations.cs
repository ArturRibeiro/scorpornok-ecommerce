namespace Catalog.Infrastructure.EntityConfigurations
{
    [ExcludeFromCodeCoverage]
    class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(x => x.Id)
                .HasColumnName("CatalogId")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("ProductName")
                .HasMaxLength(400)
                .IsRequired();


            builder.Property(x => x.Brand)
                .HasColumnName("Brand")
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(x => x.Sku)
                .HasColumnName("Sku")
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(x => x.PictureUri)
                .HasColumnName("PictureUri")
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(x => x.Price)
                //.HasColumnType("decimal(9, 4)")
                .HasColumnName("Price")
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(4000)
                .IsRequired();
        }
    }
}
