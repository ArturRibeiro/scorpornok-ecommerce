import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import type { CartItem } from "@/context/CartContext";
import { Pencil } from "lucide-react";
import { Link } from "react-router";

// Revisão dos itens, só leitura: as quantidades mudam na página do carrinho.
export default function CheckoutItems({ items }: { items: CartItem[] }) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle className="text-lg font-semibold">Review items</CardTitle>
        <Button variant="ghost" size="sm" asChild>
          <Link to="/cart" className="flex items-center gap-2">
            <Pencil className="h-4 w-4" />
            Edit cart
          </Link>
        </Button>
      </CardHeader>

      <CardContent className="space-y-4">
        {items.map((item, index) => (
          <div key={item.id} data-testid="checkout-item">
            <div className="flex items-center gap-4">
              <img
                src={item.image}
                alt={item.name}
                loading="lazy"
                className="h-16 w-16 rounded-lg object-cover bg-muted shrink-0"
              />
              <div className="flex-1 min-w-0">
                <p className="font-medium text-foreground line-clamp-1">
                  {item.name}
                </p>
                <p className="text-sm text-muted-foreground">
                  {item.quantity} × ${item.price.toFixed(2)}
                </p>
              </div>
              <p className="font-semibold text-foreground">
                ${(item.price * item.quantity).toFixed(2)}
              </p>
            </div>
            {index < items.length - 1 && <Separator className="mt-4" />}
          </div>
        ))}
      </CardContent>
    </Card>
  );
}
