import type { ProductSearchFilters } from "@/lib/catalog";

// A URL da página de resultados é a fonte de verdade da busca: /search?q=nike&brands=Nike,Adidas&min=200&max=500&page=2.
// Assim o endereço pode ser compartilhado, e voltar no navegador refaz a mesma busca.

export interface SearchState {
  filters: ProductSearchFilters;
  page: number;
}

const toPrice = (value: string | null) => {
  if (value === null || value.trim() === "") return undefined;
  const price = Number(value);
  return Number.isFinite(price) && price >= 0 ? price : undefined;
};

export function readSearchParams(params: URLSearchParams): SearchState {
  const term = params.get("q")?.trim() || undefined;
  const brands = (params.get("brands") ?? "").split(",").map((b) => b.trim()).filter(Boolean);
  const page = Number(params.get("page"));
  return {
    filters: { term, brands, minPrice: toPrice(params.get("min")), maxPrice: toPrice(params.get("max")) },
    page: Number.isInteger(page) && page > 0 ? page : 1,
  };
}

// Mudar um filtro sempre volta à primeira página.
export function writeSearchParams({ term, brands, minPrice, maxPrice }: ProductSearchFilters): URLSearchParams {
  const params = new URLSearchParams();
  if (term) params.set("q", term);
  if (brands?.length) params.set("brands", brands.join(","));
  if (minPrice !== undefined) params.set("min", String(minPrice));
  if (maxPrice !== undefined) params.set("max", String(maxPrice));
  return params;
}

export const hasFilters = ({ brands, minPrice, maxPrice }: ProductSearchFilters) =>
  Boolean(brands?.length) || minPrice !== undefined || maxPrice !== undefined;

// Aceita "1.299,99", "399,99" ou "399.99"; vazio é "sem limite" e um valor inválido devolve null.
export function parsePriceInput(value: string): number | undefined | null {
  let text = value.trim().replace(/^R\$\s*/, "");
  if (text === "") return undefined;
  // Com vírgula, ela é a separação dos centavos, e os pontos separam os milhares.
  if (text.includes(",")) text = text.replace(/\./g, "").replace(",", ".");
  const price = Number(text);
  return Number.isFinite(price) && price >= 0 ? price : null;
}
