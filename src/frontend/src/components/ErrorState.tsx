import { Button } from "@/components/ui/button";
import { RotateCcw } from "lucide-react";

// Mostrada quando a chamada ao Catalog.Web.Api falha, por exemplo com a API
// fora do ar ou bloqueada pelo CORS.
export default function ErrorState({ onRetry }: { onRetry: () => void }) {
  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <div className="text-center">
        <div className="text-6xl mb-4">🛠️</div>
        <h1 className="text-2xl font-bold text-foreground mb-2">
          Something went wrong
        </h1>
        <p className="text-muted-foreground mb-6">
          We couldn&apos;t load the products right now. Please try again in a
          moment.
        </p>
        <Button onClick={onRetry}>
          <RotateCcw className="h-4 w-4 mr-2" />
          Try again
        </Button>
      </div>
    </div>
  );
}
