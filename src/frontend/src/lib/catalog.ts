import { graphqlRequest } from "@/lib/graphql";
import type { PagedList, Product } from "@/types/product";

// Catálogo pelo gateway GraphQL (subgraph do Catalog). Roda no navegador, então as chamadas
// aparecem na aba Network como POST /graphql. As regras são as do GET /Products do Catalog:
// página a partir de 1 e até 20 produtos por página.

// Formato do tipo Product do schema do Catalog.
interface ProductResponse {
  id: number;
  name: string;
  sku: string;
  pictureUri: string;
  description: string;
  price: number;
}

const PRODUCT_FIELDS = "id name sku pictureUri price description";

const toProduct = ({ pictureUri, ...product }: ProductResponse): Product => ({
  ...product,
  image: pictureUri,
});

const PRODUCTS_QUERY = `query Products($page: Int, $pageSize: Int) {
  products(page: $page, pageSize: $pageSize) {
    pageInfo { currentPage pageSize totalItems totalPages hasPreviousPage hasNextPage }
    items { ${PRODUCT_FIELDS} }
  }
}`;

// O gateway limita pageSize a 20.
export async function getProducts(
  pageNumber = 1,
  pageSize = 20,
  signal?: AbortSignal
): Promise<PagedList<Product>> {
  const { products } = await graphqlRequest<{ products: PagedList<ProductResponse> }>(
    PRODUCTS_QUERY,
    { page: pageNumber, pageSize },
    signal
  );
  return { pageInfo: products.pageInfo, items: products.items.map(toProduct) };
}

const PRODUCT_QUERY = `query Product($id: Long!) {
  productById(id: $id) { ${PRODUCT_FIELDS} }
}`;

// null quando o produto não existe (o gateway devolve productById nulo, sem erro).
export async function getProduct(
  id: number,
  signal?: AbortSignal
): Promise<Product | null> {
  const { productById } = await graphqlRequest<{ productById: ProductResponse | null }>(
    PRODUCT_QUERY,
    { id },
    signal
  );
  return productById ? toProduct(productById) : null;
}

// Filtros da busca, todos opcionais e combináveis. O termo procura todas as palavras no nome, na marca e
// na descrição, sem diferenciar maiúsculas e acentos, com os do nome primeiro; a faixa de preço inclui
// os limites.
export interface ProductSearchFilters {
  term?: string;
  brands?: string[];
  minPrice?: number;
  maxPrice?: number;
}

const SEARCH_QUERY = `query SearchProducts($term: String, $brands: [String!], $minPrice: Decimal, $maxPrice: Decimal, $page: Int, $pageSize: Int) {
  searchProducts(term: $term, brands: $brands, minPrice: $minPrice, maxPrice: $maxPrice, page: $page, pageSize: $pageSize) {
    pageInfo { currentPage pageSize totalItems totalPages hasPreviousPage hasNextPage }
    items { ${PRODUCT_FIELDS} }
  }
}`;

// Mesmas páginas do getProducts (até 20 por página). As sugestões do cabeçalho usam pageSize 5.
export async function searchProducts(
  filters: ProductSearchFilters,
  pageNumber = 1,
  pageSize = 20,
  signal?: AbortSignal
): Promise<PagedList<Product>> {
  const { searchProducts } = await graphqlRequest<{ searchProducts: PagedList<ProductResponse> }>(
    SEARCH_QUERY,
    {
      term: filters.term || null,
      brands: filters.brands?.length ? filters.brands : null,
      minPrice: filters.minPrice ?? null,
      maxPrice: filters.maxPrice ?? null,
      page: pageNumber,
      pageSize,
    },
    signal
  );
  return { pageInfo: searchProducts.pageInfo, items: searchProducts.items.map(toProduct) };
}

// Marcas do catálogo, em ordem alfabética, para o filtro da busca.
export async function getBrands(signal?: AbortSignal): Promise<string[]> {
  const { productBrands } = await graphqlRequest<{ productBrands: string[] }>(
    "query ProductBrands { productBrands }",
    undefined,
    signal
  );
  return productBrands;
}
