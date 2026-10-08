import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import type { CreatedOrder } from "@/lib/orders";
import { CheckCircle2 } from "lucide-react";
import { Link } from "react-router";

export default function OrderConfirmation({ order }: { order: CreatedOrder }) {
  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <Card className="max-w-lg mx-auto">
        <CardContent className="pt-8 text-center space-y-4">
          <CheckCircle2 className="h-16 w-16 text-green-500 mx-auto" />
          <h1 className="text-2xl font-bold text-foreground">
            Thank you for your order!
          </h1>
          <dl className="grid grid-cols-2 gap-2 text-left text-sm rounded-lg bg-muted p-4">
            <dt className="text-muted-foreground">Order number</dt>
            <dd className="font-semibold" data-testid="order-number">
              {order.orderNumber}
            </dd>
            <dt className="text-muted-foreground">Status</dt>
            <dd className="font-semibold" data-testid="order-status">
              {order.status}
            </dd>
            <dt className="text-muted-foreground">Total</dt>
            <dd className="font-semibold">${order.total.toFixed(2)}</dd>
          </dl>
          <Button asChild>
            <Link to="/">Continue Shopping</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
