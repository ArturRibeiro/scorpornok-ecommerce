import ErrorState from "@/components/ErrorState";
import LoadingState from "@/components/LoadingState";
import { Button } from "@/components/ui/button";
import { useRequest } from "@/hooks/useRequest";
import { getProducts } from "@/lib/catalog";
import { useEffect } from "react";
import { Link, useSearchParams } from "react-router";
import ProductCard from "./ProductCard";
import ProductPagination from "./ProductPagination";

// Múltiplo de 2 e 3 para completar as linhas da grade; a API aceita até 20.
const PAGE_SIZE = 18;

export default function ProductList() {
  const [searchParams] = useSearchParams();
  const requestedPage = Number(searchParams.get("page"));
  const page = Number.isInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1;

  const { data, error, loading, retry } = useRequest(`products?page=${page}`, (signal) =>
    getProducts(page, PAGE_SIZE, signal)
  );

  // O React Router não rola a página ao trocar só a query string.
  useEffect(() => {
    window.scrollTo({ top: 0 });
  }, [page]);

  if (loading) return <LoadingState />;
  if (error || !data) return <ErrorState onRetry={retry} />;

  const products = data.items;
  const isPastLastPage = products.length === 0 && data.pageInfo.totalItems > 0;

  return (
    <>
      <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 max-w-7xl mx-auto">
        {products.length > 0 ? (
          products.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))
        ) : (
          <div className="col-span-full flex flex-col items-center justify-center py-16 text-center">
            <div className="text-6xl mb-4">🔍</div>
            <h3 className="text-xl font-semibold text-foreground mb-2">
              No products found
            </h3>
            {isPastLastPage ? (
              <Button asChild>
                <Link to="/">Go to the first page</Link>
              </Button>
            ) : (
              <p className="text-muted-foreground mb-4">
                Try adjusting your filters or search terms
              </p>
            )}
          </div>
        )}
      </div>

      {!isPastLastPage && <ProductPagination pageInfo={data.pageInfo} />}
    </>
  );
}
