import ErrorState from "@/components/ErrorState";
import LoadingState from "@/components/LoadingState";
import { useRequest } from "@/hooks/useRequest";
import { getProducts } from "@/lib/catalog";
import ProductCard from "./ProductCard";

export default function ProductList() {
  const { data, error, loading, retry } = useRequest("products", (signal) =>
    getProducts(1, 20, signal)
  );

  if (loading) return <LoadingState />;
  if (error || !data) return <ErrorState onRetry={retry} />;

  const products = data.items;

  return (
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
          <p className="text-muted-foreground mb-4">
            Try adjusting your filters or search terms
          </p>
        </div>
      )}
    </div>
  );
}
