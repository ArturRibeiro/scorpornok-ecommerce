using System;
using System.Runtime.Serialization;

namespace Catalog.Queries.Products
{
    [Serializable]
    public class ProductItemMessageResponse
    {
        [DataMember]
        public string CatalogId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Sku { get; set; }

        [DataMember]
        public string PictureUri { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public double Price { get; set; }
    }
}
