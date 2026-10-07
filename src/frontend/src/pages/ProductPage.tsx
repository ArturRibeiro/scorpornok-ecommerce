import ErrorState from "@/components/ErrorState";
import LoadingState from "@/components/LoadingState";
import Features from "@/components/product/Features";
import ProductBreadcrumb from "@/components/product/ProductBreadcrumb";
import ProductDetails from "@/components/product/ProductDetails";
import ProductNotFound from "@/components/product/ProductNotFound";
import RelatedProducts from "@/components/product/RelatedProducts";
import { useRequest } from "@/hooks/useRequest";
import { getProduct, getProducts } from "@/lib/catalog";
import { useParams } from "react-router";

export default function ProductPage() {
  const { productId = "" } = useParams();
  const isValidId = /^\d+$/.test(productId);

  const { data, error, loading, retry } = useRequest(
    `product/${productId}`,
    async (signal) => {
      if (!isValidId) return null;
      const [product, { items }] = await Promise.all([
        getProduct(Number(productId), signal),
        getProducts(1, 20, signal),
      ]);
      return { product, items };
    }
  );

  if (!isValidId) return <ProductNotFound />;
  if (loading) return <LoadingState />;
  if (error) return <ErrorState onRetry={retry} />;
  if (!data?.product) return <ProductNotFound />;

  const { product, items } = data;

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <ProductBreadcrumb />

      <ProductDetails product={product} />

      <Features />

      <RelatedProducts
        products={items.filter((p) => p.id !== product.id).slice(0, 4)}
      />
    </div>
  );
}
