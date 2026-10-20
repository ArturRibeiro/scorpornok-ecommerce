import ErrorState from "@/components/ErrorState";
import LoadingState from "@/components/LoadingState";
import ProductCard from "@/components/home/ProductCard";
import ProductPagination from "@/components/home/ProductPagination";
import SearchFilters from "@/components/search/SearchFilters";
import { Button } from "@/components/ui/button";
import { useRequest } from "@/hooks/useRequest";
import { searchProducts, type ProductSearchFilters } from "@/lib/catalog";
import { hasFilters, readSearchParams, writeSearchParams } from "@/lib/searchParams";
import { SlidersHorizontal } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router";

// A mesma grade da home: múltiplo de 2 e 3, e a API aceita até 20.
const PAGE_SIZE = 18;

// Resultados da busca: o termo, os filtros e a página vêm da URL (/search?q=&brands=&min=&max=&page=).
export default function SearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { filters, page } = readSearchParams(searchParams);
  const [filtersOpen, setFiltersOpen] = useState(false);

  const { data, error, loading, retry } = useRequest(`search?${searchParams.toString()}`, (signal) =>
    searchProducts(filters, page, PAGE_SIZE, signal)
  );

  // O React Router não rola a página ao trocar só a query string.
  useEffect(() => {
    window.scrollTo({ top: 0 });
  }, [searchParams]);

  const changeFilters = (changes: Partial<ProductSearchFilters>) =>
    setSearchParams(writeSearchParams({ ...filters, ...changes }));
  // Limpar os filtros mantém o termo buscado.
  const clearFilters = () => setSearchParams(writeSearchParams({ term: filters.term }));

  const title = filters.term ? `Resultados para “${filters.term}”` : "Todos os produtos";

  return (
    <div className="bg-background px-4 py-8 sm:py-12 lg:px-8 min-h-screen">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">{title}</h1>
        {data && (
          <p className="mt-1 text-sm text-muted-foreground" aria-live="polite">
            {data.pageInfo.totalItems === 1
              ? "1 produto encontrado"
              : `${data.pageInfo.totalItems} produtos encontrados`}
          </p>
        )}

        <div className="mt-8 grid gap-8 lg:grid-cols-[240px_1fr]">
          <aside aria-label="Filtros">
            <Button
              type="button"
              variant="outline"
              className="w-full lg:hidden"
              aria-expanded={filtersOpen}
              aria-controls="search-filters"
              onClick={() => setFiltersOpen((open) => !open)}
            >
              <SlidersHorizontal className="h-4 w-4" />
              {filtersOpen ? "Esconder filtros" : "Filtros"}
            </Button>
            <div id="search-filters" className={filtersOpen ? "mt-4 lg:mt-0" : "hidden lg:block"}>
              <SearchFilters filters={filters} onChange={changeFilters} onClear={clearFilters} />
            </div>
          </aside>

          <section aria-label="Resultados">
            {loading ? (
              <LoadingState />
            ) : error || !data ? (
              <ErrorState onRetry={retry} />
            ) : data.items.length > 0 ? (
              <>
                <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 xl:grid-cols-3">
                  {data.items.map((product) => (
                    <ProductCard key={product.id} product={product} />
                  ))}
                </div>
                <ProductPagination pageInfo={data.pageInfo} />
              </>
            ) : (
              <NoResults
                pastLastPage={data.pageInfo.totalItems > 0}
                filtered={hasFilters(filters)}
                onClearFilters={clearFilters}
                firstPage={`?${writeSearchParams(filters).toString()}`}
              />
            )}
          </section>
        </div>
      </div>
    </div>
  );
}

function NoResults({
  pastLastPage,
  filtered,
  onClearFilters,
  firstPage,
}: {
  pastLastPage: boolean;
  filtered: boolean;
  onClearFilters: () => void;
  firstPage: string;
}) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <div className="text-6xl mb-4">🔍</div>
      <h2 className="text-xl font-semibold text-foreground mb-2">Nenhum produto encontrado</h2>
      {pastLastPage ? (
        <Button asChild>
          <Link to={{ search: firstPage }}>Ir para a primeira página</Link>
        </Button>
      ) : (
        <>
          <p className="text-muted-foreground mb-6">Confira o termo buscado ou tente outros filtros.</p>
          <div className="flex flex-wrap justify-center gap-3">
            {filtered && (
              <Button type="button" variant="outline" onClick={onClearFilters}>
                Limpar filtros
              </Button>
            )}
            <Button asChild>
              <Link to="/">Voltar à loja</Link>
            </Button>
          </div>
        </>
      )}
    </div>
  );
}
