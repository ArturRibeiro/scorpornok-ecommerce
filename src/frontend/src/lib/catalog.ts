import type { PagedList, Product } from "@/types/product";

// Cliente do Catalog.Web.Api. Roda no navegador, então as chamadas aparecem
// na aba Network do DevTools e a API precisa liberar a origem do front no
// CORS. A URL é a que o navegador enxerga (porta publicada no host), não o
// nome do container; o Vite a embute no build a partir de VITE_CATALOG_API_URL.
const CATALOG_API_URL =
  import.meta.env.VITE_CATALOG_API_URL ?? "http://localhost:5064";

// Formato de ProductItemMessageResponse no backend.
interface ProductResponse {
  id: number;
  name: string;
  sku: string;
  pictureUri: string;
  description: string;
  price: number;
}

const toProduct = ({ pictureUri, ...product }: ProductResponse): Product => ({
  ...product,
  image: pictureUri,
});

const request = (path: string, signal?: AbortSignal) =>
  fetch(`${CATALOG_API_URL}${path}`, { signal });

// A API limita pageSize a 20.
export async function getProducts(
  pageNumber = 1,
  pageSize = 20,
  signal?: AbortSignal
): Promise<PagedList<Product>> {
  const response = await request(
    `/Products?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    signal
  );
  if (!response.ok) {
    throw new Error(`Catalog API respondeu ${response.status} em Products`);
  }

  const data: PagedList<ProductResponse> = await response.json();
  return { pageInfo: data.pageInfo, items: data.items.map(toProduct) };
}

export async function getProduct(
  id: number,
  signal?: AbortSignal
): Promise<Product | null> {
  const response = await request(`/GetProductById/${id}`, signal);
  if (response.status === 404) return null;
  if (!response.ok) {
    throw new Error(`Catalog API respondeu ${response.status} em GetProductById`);
  }

  return toProduct(await response.json());
}
