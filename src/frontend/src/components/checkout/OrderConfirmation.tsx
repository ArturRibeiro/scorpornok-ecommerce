import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/context/AuthContext";
import { watchOrderPayment } from "@/lib/orderHub";
import type { CreatedOrder } from "@/lib/orders";
import { formatPrice } from "@/lib/utils";
import { CheckCircle2, Clock, Loader2, XCircle } from "lucide-react";
import { useEffect, useState } from "react";
import { Link } from "react-router";

// Sem mensagem do hub nesse prazo, a tela deixa de esperar: o resultado vai por e-mail.
const PAYMENT_TIMEOUT_MS = 10_000;

type PaymentState = "processing" | "approved" | "declined" | "pending";

const PAYMENT_ICON: Record<PaymentState, React.ReactNode> = {
  processing: <Loader2 className="h-16 w-16 text-primary mx-auto animate-spin" />,
  approved: <CheckCircle2 className="h-16 w-16 text-green-500 mx-auto" />,
  declined: <XCircle className="h-16 w-16 text-destructive mx-auto" />,
  pending: <Clock className="h-16 w-16 text-muted-foreground mx-auto" />,
};

const paymentView = (
  state: PaymentState,
  email: string
): { title: string; detail: string } => {
  switch (state) {
    case "processing":
      return {
        title: "Processando o seu pagamento…",
        detail: "Seu pedido foi registrado. Esta página será atualizada assim que o pagamento for processado.",
      };
    case "approved":
      return {
        title: "Obrigado pela sua compra!",
        detail: `Seu pagamento foi aprovado. Enviamos a confirmação para ${email}.`,
      };
    case "declined":
      return {
        title: "Pagamento recusado",
        detail: `Seu cartão foi recusado. Enviamos os detalhes para ${email}.`,
      };
    case "pending":
      return {
        title: "Pagamento pendente",
        detail: `Seu pedido foi registrado e o pagamento está pendente. Vamos enviar o resultado para ${email}.`,
      };
  }
};

const PAYMENT_LABEL: Record<PaymentState, string> = {
  processing: "Processando",
  approved: "Aprovado",
  declined: "Recusado",
  pending: "Pendente",
};

export default function OrderConfirmation({
  order,
  email,
}: {
  order: CreatedOrder;
  email: string;
}) {
  const [payment, setPayment] = useState<PaymentState>("processing");
  const { getAccessToken } = useAuth();

  // Recebe o resultado pelo hub do Orders, sem consultar o pedido. Falha de conexão
  // ou prazo esgotado viram "pending": o pedido existe e o resultado chega por e-mail.
  useEffect(() => {
    const controller = new AbortController();
    let unmounted = false;
    const timer = setTimeout(() => controller.abort(), PAYMENT_TIMEOUT_MS);

    watchOrderPayment(order.orderNumber, getAccessToken, controller.signal)
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
  }, [order.orderNumber, getAccessToken]);

  const view = paymentView(payment, email);

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <Card className="max-w-lg mx-auto">
        <CardContent className="pt-8 text-center space-y-4">
          {PAYMENT_ICON[payment]}
          <h1 className="text-2xl font-bold text-foreground">{view.title}</h1>
          <p className="text-muted-foreground">{view.detail}</p>
          <dl className="grid grid-cols-2 gap-2 text-left text-sm rounded-lg bg-muted p-4">
            <dt className="text-muted-foreground">Número do pedido</dt>
            <dd className="font-semibold" data-testid="order-number">
              {order.orderNumber}
            </dd>
            <dt className="text-muted-foreground">Pagamento</dt>
            <dd className="font-semibold" data-testid="payment-status">
              {PAYMENT_LABEL[payment]}
            </dd>
            <dt className="text-muted-foreground">Total</dt>
            <dd className="font-semibold">{formatPrice(order.total)}</dd>
          </dl>
          <Button asChild>
            <Link to="/">Continuar comprando</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
