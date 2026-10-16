import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";
import { Link } from "react-router";

export default function NotFoundPage() {
  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <div className="text-center">
        <div className="text-6xl mb-4">🧭</div>
        <h1 className="text-2xl font-bold text-foreground mb-2">
          Página não encontrada
        </h1>
        <p className="text-muted-foreground mb-6">
          A página que você procura não existe.
        </p>
        <Button asChild>
          <Link to="/">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Voltar para a loja
          </Link>
        </Button>
      </div>
    </div>
  );
}
