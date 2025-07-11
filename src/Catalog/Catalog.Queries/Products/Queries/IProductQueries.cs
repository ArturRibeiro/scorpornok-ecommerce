using System.Threading.Tasks;
using Frameworker.EntityFrameworkCore;

namespace Catalog.Queries.Products.Queries
{
    public interface IProductQueries
    {
        Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(PagingModel paging);
    }
}
