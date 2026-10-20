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

        public async Task<IPagedList<ProductItemMessageResponse>> SearchProducts(ProductSearch search, PagingModel paging)
        {
            var words = search.Words();

            // Cada palavra precisa estar no nome, na marca ou na descrição, em qualquer ordem, sem diferenciar
            // maiúsculas e acentos.
            var products = _context.DbSet<Product>().AsNoTracking()
                .Where(ProductSearchExpressions.Filter(words));

            var brands = search.SelectedBrands();
            if (brands.Count > 0)
                products = products.Where(x => brands.Contains(x.Brand));

            if (search.IsEmptyRange())
                products = products.Where(x => false);
            if (search.MinPrice is { } min)
                products = products.Where(x => x.Price >= min);
            if (search.MaxPrice is { } max)
                products = products.Where(x => x.Price <= max);

            // Primeiro quem tem todas as palavras no nome, depois quem precisa da marca, por último quem
            // só é encontrado com a descrição; no mesmo grupo, a ordem do catálogo (páginas estáveis).
            return await products
                .OrderBy(ProductSearchExpressions.Rank(words))
                .ThenBy(x => x.Id)
                .Select(ToResponse)
                .ToPagedList(paging.Page(), paging.Size());
        }

        public async Task<IReadOnlyList<string>> GetBrands()
            => await _context.DbSet<Product>().AsNoTracking()
                .Select(x => x.Brand)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

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