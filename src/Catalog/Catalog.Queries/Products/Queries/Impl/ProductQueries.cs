using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Products;
using Frameworker.EntityFrameworkCore;
using Frameworker.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Queries.Products.Queries.Impl
{
    public class ProductQueries : IProductQueries
    {
        private readonly IApplicationCatalogDbContext _context;

        public ProductQueries(IApplicationCatalogDbContext context) => _context = context;

        public async Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(PagingModel paging)
        {
            var result = await _context.DbSet<Product>().AsNoTracking()
                .Select(x=> new ProductItemMessageResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                    PictureUri = x.PictureUri,
                    Price = x.Price,
                    Description = x.Description
                })
                .ToPagedList(paging.PageNumber, paging.PageSize);
            return result;
        }
    }
}