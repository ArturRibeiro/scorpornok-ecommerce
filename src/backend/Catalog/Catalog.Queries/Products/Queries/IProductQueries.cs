using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frameworker.EntityFrameworkCore;

namespace Catalog.Queries.Products.Queries
{
    public interface IProductQueries
    {
        // Todos os produtos, em ordem estável (por id), sem paginar: a base da página do REST e do
        // GraphQL, para as duas APIs devolverem os mesmos produtos na mesma ordem.
        IQueryable<ProductItemMessageResponse> QueryProducts();

        Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(PagingModel paging);

        Task<ProductItemMessageResponse> GetProductById(long id);

        // Só os produtos encontrados; os ids inexistentes não aparecem no resultado.
        Task<IReadOnlyList<ProductItemMessageResponse>> GetProductsByIds(IReadOnlyCollection<long> ids);
    }
}
