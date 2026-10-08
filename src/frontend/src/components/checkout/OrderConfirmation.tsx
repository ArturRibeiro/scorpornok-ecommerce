import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { watchOrderPayment } from "@/lib/orderHub";
import type { CreatedOrder } from "@/lib/orders";
import { CheckCircle2, Clock, Loader2, XCircle } from "lucide-react";
import { useEffect, useState } from "react";
import { Link } from "react-router";

// Sem mensagem do hub nesse prazo, a tela deixa de esperar.
const PAYMENT_TIMEOUT_MS = 30_000;

type PaymentState = "processing" | "approved" | "declined" | "pending";

const PAYMENT_VIEW: Record<
  PaymentState,
  { icon: React.ReactNode; title: string; detail: string }
> = {
  processing: {
    icon: <Loader2 className="h-16 w-16 text-primary mx-auto animate-spin" />,
    title: "Processing your payment…",
    detail: "Your order was placed. We'll update this page as soon as the payment is processed.",
  },
  approved: {
    icon: <CheckCircle2 className="h-16 w-16 text-green-500 mx-auto" />,
    title: "Thank you for your order!",
    detail: "Your payment was approved.",
  },
  declined: {
    icon: <XCircle className="h-16 w-16 text-destructive mx-auto" />,
    title: "Payment declined",
    detail: "Your order was placed, but the card was declined.",
  },
  pending: {
    icon: <Clock className="h-16 w-16 text-muted-foreground mx-auto" />,
    title: "Payment still processing",
    detail: "Your order was placed, but the payment is still being processed.",
  },
};

const PAYMENT_LABEL: Record<PaymentState, string> = {
  processing: "Processing",
  approved: "Approved",
  declined: "Declined",
  pending: "Still processing",
};

export default function OrderConfirmation({ order }: { order: CreatedOrder }) {
  const [payment, setPayment] = useState<PaymentState>("processing");

  // Recebe o resultado pelo hub do Orders, sem consultar o pedido. Falha de conexão
  // ou prazo esgotado viram "pending": o pedido existe, só o resultado não chegou.
  useEffect(() => {
    const controller = new AbortController();
    let unmounted = false;
    const timer = setTimeout(() => controller.abort(), PAYMENT_TIMEOUT_MS);

    watchOrderPayment(order.orderNumber, controller.signal)
      .then((approved) => setPayment(approved ? "approved" : "declined"))
      .catch(() => {
        if (!unmounted) setPayment("pending");
      })
      .finally(() => clearTimeout(timer));

    return () => {
      unmounted = true;
      clearTimeout(timer);
      controller.abort();
    };
  }, [order.orderNumber]);

  const view = PAYMENT_VIEW[payment];

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <Card className="max-w-lg mx-auto">
        <CardContent className="pt-8 text-center space-y-4">
          {view.icon}
          <h1 className="text-2xl font-bold text-foreground">{view.title}</h1>
          <p className="text-muted-foreground">{view.detail}</p>
          <dl className="grid grid-cols-2 gap-2 text-left text-sm rounded-lg bg-muted p-4">
            <dt className="text-muted-foreground">Order number</dt>
            <dd className="font-semibold" data-testid="order-number">
              {order.orderNumber}
            </dd>
            <dt className="text-muted-foreground">Payment</dt>
            <dd className="font-semibold" data-testid="payment-status">
              {PAYMENT_LABEL[payment]}
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
