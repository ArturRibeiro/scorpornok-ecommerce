import ProductList from "@/components/home/ProductList";

export default function Home() {
  return (
    <div className="bg-background px-4 py-8 sm:py-12 lg:py-16 lg:px-8 min-h-screen">
      <div className="text-center mx-auto mb-18 space-y-3">
        <h1 className="text-primary leading-tighter text-4xl font-semibold tracking-tight text-balance lg:leading-[1.1] lg:font-semibold xl:text-5xl xl:tracking-tighter">
          Step Into Style
        </h1>
        <p className="text-foreground text-base max-w-3xl mx-auto text-balance sm:text-lg">
          Discover our latest collection of premium sneakers — comfort, design,
          and performance in every pair.
        </p>
      </div>
      <ProductList />
    </div>
  );
}
