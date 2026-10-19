using System;
using System.Collections.Generic;
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

        // A ordenação garante páginas estáveis entre uma chamada e outra.
        public IQueryable<ProductItemMessageResponse> QueryProducts()
            => _context.DbSet<Product>().AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(ToResponse);

        public async Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(PagingModel paging)
            => await QueryProducts().ToPagedList(paging.Page(), paging.Size());

        public async Task<ProductItemMessageResponse> GetProductById(long id)
            => await _context.DbSet<Product>().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync();

        public async Task<IReadOnlyList<ProductItemMessageResponse>> GetProductsByIds(IReadOnlyCollection<long> ids)
            => await _context.DbSet<Product>().AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Id)
                .Select(ToResponse)
                .ToListAsync();
    }
}