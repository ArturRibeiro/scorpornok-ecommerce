using Shared.Code.Models;

namespace Catalog.Domain.Products
{
    public class Product : Entity<long>, IAggregateRoot
    {
        public Product()
        {
            
        }
        public Product(string product, string sku, string photo, decimal @decimal, string descriptionForProduct)
        {
            Name = product;
            Sku = sku;
            PictureUri = photo;
            Price = @decimal;
            Description = descriptionForProduct;
        }
        
        public string Name { get; private set; }

        public string Sku { get; private set; }

        public string PictureUri { get; private set; }

        public decimal Price { get; private set; }

        public string Description { get; private set; }
    }
}
