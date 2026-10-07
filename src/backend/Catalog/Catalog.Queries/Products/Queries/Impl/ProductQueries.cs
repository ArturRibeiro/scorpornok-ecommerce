using System;
using System.Linq;
using System.Linq.Expressions;
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

        private static readonly Expression<Func<Product, ProductItemMessageResponse>> ToResponse =
            x => new ProductItemMessageResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Sku = x.Sku,
                PictureUri = x.PictureUri,
                Price = x.Price,
                Description = x.Description
            };

        public async Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(PagingModel paging)
        {
            // A ordenação garante páginas estáveis entre uma chamada e outra.
            var result = await _context.DbSet<Product>().AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(ToResponse)
                .ToPagedList(paging.Page(), paging.Size());
            return result;
        }

        public async Task<ProductItemMessageResponse> GetProductById(long id)
            => await _context.DbSet<Product>().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync();
    }
}