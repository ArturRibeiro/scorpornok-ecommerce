import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { AlertCircle, Loader2, Lock } from "lucide-react";

interface CheckoutSummaryProps {
  itemCount: number;
  total: number;
  submitting: boolean;
  // Mensagens do Orders (400) ou o aviso de falha de rede.
  errors: string[];
}

// Fica dentro do <form> da página: o botão é o submit do checkout.
export default function CheckoutSummary({
  itemCount,
  total,
  submitting,
  errors,
}: CheckoutSummaryProps) {
  return (
    <Card className="sticky top-4">
      <CardHeader>
        <CardTitle className="text-lg font-semibold">Order Summary</CardTitle>
      </CardHeader>

      <CardContent className="space-y-4">
        <div className="space-y-3">
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">
              Subtotal ({itemCount} {itemCount === 1 ? "item" : "items"})
            </span>
            <span className="font-medium">${total.toFixed(2)}</span>
          </div>

          <Separator />

          <div className="flex justify-between">
            <span className="text-lg font-semibold">Total</span>
            <span className="text-lg font-bold text-primary">
              ${total.toFixed(2)}
            </span>
          </div>
        </div>

        {errors.length > 0 && (
          <div
            role="alert"
            className="rounded-lg border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive"
          >
            <div className="flex items-center gap-2 font-medium">
              <AlertCircle className="h-4 w-4" />
              We couldn&apos;t place your order
            </div>
            <ul className="mt-2 list-disc pl-5 space-y-1">
              {errors.map((error) => (
                <li key={error}>{error}</li>
              ))}
            </ul>
          </div>
        )}

        <Button
          type="submit"
          size="lg"
          disabled={submitting}
          className="w-full bg-primary text-primary-foreground hover:bg-primary/90"
        >
          {submitting ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              Placing order...
            </>
          ) : (
            <>
              <Lock className="h-4 w-4" />
              Place order
            </>
          )}
        </Button>
      </CardContent>
    </Card>
  );
}
