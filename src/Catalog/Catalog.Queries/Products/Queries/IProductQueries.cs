using System.Threading.Tasks;
using Frameworker.Scorponok.Reading.Database.Impl;

namespace Catalog.Queries.Products.Queries
{
    public interface IProductQueries
    {
        Task<PagedList<ProductItemMessageResponse>> GetAllProducts();
    }
}
