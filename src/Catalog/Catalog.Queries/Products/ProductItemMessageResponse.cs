using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog.Queries.Products
{
    [DataContract]
    public class ProductItemMessageResponse
    {
        [DataMember]
        public Guid CatalogId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Sku { get; set; }

        [DataMember]
        public string PictureUri { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public decimal Price { get; set; }
    }
}
