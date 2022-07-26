using Shared.Code.Models;
using System;

namespace Catalog.Domain.Products
{
    public class Product : Entity<long>, IAggregateRoot
    {
        public string Name { get; private set; }

        public string Sku { get; private set; }

        public string PictureUri { get; private set; }

        public decimal Price { get; private set; }

        public string Description { get; private set; }
    }
}
