namespace Catalog.Queries.Products.Queries
{
    public interface IProductQueries
    {
        // Todos os produtos, em ordem estável (por id), sem paginar: a base da página do REST e do
        // GraphQL, para as duas APIs devolverem os mesmos produtos na mesma ordem.
        IQueryable<ProductItemMessageResponse> QueryProducts();

        Task<IPagedList<ProductItemMessageResponse>> GetAllProducts(PagingModel paging);

        // Busca no nome, na marca e na descrição (todas as palavras, sem diferenciar maiúsculas e acentos),
        // com marcas e faixa de preço; os do nome primeiro, depois os da marca e os da descrição, cada grupo
        // na ordem do catálogo, com as mesmas regras de página do GetAllProducts.
        Task<IPagedList<ProductItemMessageResponse>> SearchProducts(ProductSearch search, PagingModel paging);

        // As marcas do catálogo, sem repetição, em ordem alfabética.
        Task<IReadOnlyList<string>> GetBrands();

        Task<ProductItemMessageResponse> GetProductById(long id);

        // Só os produtos encontrados; os ids inexistentes não aparecem no resultado.
        Task<IReadOnlyList<ProductItemMessageResponse>> GetProductsByIds(IReadOnlyCollection<long> ids);
    }
}
