using System.Threading.Tasks;
using Frameworker.Scorponok.Reading.Database;

namespace Catalog.Queries.Products.Queries
{
    public interface IProductQueries
    {
        Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(int pageNumber, int pageSize);
    }
}
