import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";
import { Link } from "react-router";

export default function ProductBreadcrumb() {
  return (
    <nav className="mb-8">
      <Button
        variant="ghost"
        asChild
        className="text-muted-foreground hover:text-foreground"
      >
        <Link to="/" className="flex items-center gap-2">
          <ArrowLeft className="h-4 w-4" />
          Return to Shop
        </Link>
      </Button>
    </nav>
  );
}
