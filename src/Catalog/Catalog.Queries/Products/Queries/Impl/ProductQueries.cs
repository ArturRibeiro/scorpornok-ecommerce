using System.Threading.Tasks;
using Frameworker.Scorponok.Reading.Database;

namespace Catalog.Queries.Products.Queries.Impl
{
    public class ProductQueries : IProductQueries
    {
        private readonly IApplicationReadDb _context;

        public ProductQueries(IApplicationReadDb context) => _context = context;

        public async Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(int pageNumber, int pageSize)
        {
            var pagedList = await _context.QueryToPagedListAsync<ProductItemMessageResponse>
            (
                sqlCount: $@"SELECT Count(1) FROM Products p"
                , sqlRows: $@"SELECT p.CatalogId, p.ProductName, p.Sku, p.PictureUri,p.Price, p.Description
                            FROM   Products p
                            ORDER  BY ProductName"
                , pageNumber: pageNumber
                , pageSize: pageSize
            );
            return pagedList;
        }
    }
}