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
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Price { get; set; }
    }
}
