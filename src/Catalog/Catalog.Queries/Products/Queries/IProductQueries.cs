using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Queries.Products.Queries
{
    public interface IProductQueries
    {
        Task<ProductItemMessageResponse[]> GetAllProducts();
    }
}
