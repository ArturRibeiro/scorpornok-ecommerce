using System.Collections.Generic;
using System.Threading.Tasks;
using Frameworker.EntityFrameworkCore;

namespace Catalog.Queries.Products.Queries
{
    public interface IProductQueries
    {
        Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(PagingModel paging);

        Task<ProductItemMessageResponse> GetProductById(long id);

        // Só os produtos encontrados; os ids inexistentes não aparecem no resultado.
        Task<IReadOnlyList<ProductItemMessageResponse>> GetProductsByIds(IReadOnlyCollection<long> ids);
    }
}
