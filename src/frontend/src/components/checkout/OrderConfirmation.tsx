import RetryPaymentForm from "@/components/orders/RetryPaymentForm";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/context/AuthContext";
import { useCart } from "@/context/CartContext";
import { watchOrderPayment } from "@/lib/orderHub";
import { type CreatedOrder } from "@/lib/orders";
import { formatPrice } from "@/lib/utils";
import { AlertCircle, CheckCircle2, Clock, Loader2, XCircle } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Link } from "react-router";

// Sem mensagem do hub nesse prazo, a tela deixa de esperar: o resultado vai por e-mail.
const PAYMENT_TIMEOUT_MS = 10_000;

type PaymentState = "processing" | "approved" | "declined" | "notProcessed" | "pending";

const PAYMENT_ICON: Record<PaymentState, React.ReactNode> = {
  processing: <Loader2 className="h-16 w-16 text-primary mx-auto animate-spin" />,
  approved: <CheckCircle2 className="h-16 w-16 text-green-500 mx-auto" />,
  declined: <XCircle className="h-16 w-16 text-destructive mx-auto" />,
  notProcessed: <AlertCircle className="h-16 w-16 text-amber-500 mx-auto" />,
  pending: <Clock className="h-16 w-16 text-muted-foreground mx-auto" />,
};

const paymentView = (
  state: PaymentState,
  email: string,
  remainingAttempts: number
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
        detail:
          remainingAttempts > 0
            ? `Seu cartão foi recusado e nada foi cobrado. Você pode tentar com outro cartão (${remainingAttempts} ${remainingAttempts === 1 ? "tentativa restante" : "tentativas restantes"}). Enviamos os detalhes para ${email}.`
            : `Seu cartão foi recusado e as tentativas de pagamento deste pedido acabaram. Para comprar, faça um novo pedido; seu carrinho continua salvo. Enviamos os detalhes para ${email}.`,
      };
    case "notProcessed":
      return {
        title: "Pagamento não processado",
        detail: `Não conseguimos processar o pagamento do seu pedido e nada foi cobrado. Você pode refazer a compra. Enviamos os detalhes para ${email}.`,
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
  notProcessed: "Não processado",
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
  // Tentativa acompanhada: muda a cada "Tentar com outro cartão".
  const [attempt, setAttempt] = useState(order.attempt ?? 1);
  const maxAttempts = order.maxAttempts ?? 1;
  const { getAccessToken } = useAuth();
  const { clearCart } = useCart();
  const cartCleared = useRef(false);

  // Recebe o resultado pelo hub do Orders, sem consultar o pedido. Falha de conexão
  // ou prazo esgotado viram "pending": o pedido existe e o resultado chega por e-mail.
  // Refaz a conexão a cada nova tentativa, ignorando o desfecho das anteriores.
  useEffect(() => {
    const controller = new AbortController();
    let unmounted = false;
    const timer = setTimeout(() => controller.abort(), PAYMENT_TIMEOUT_MS);

    watchOrderPayment(order.orderNumber, getAccessToken, controller.signal, attempt)
      .then((outcome) => setPayment(outcome))
      .catch(() => {
        if (!unmounted) setPayment("pending");
      })
      .finally(() => clearTimeout(timer));

    return () => {
      unmounted = true;
      clearTimeout(timer);
      controller.abort();
    };
  }, [order.orderNumber, getAccessToken, attempt]);

  // O carrinho só é esvaziado com o pedido aprovado ou pendente (o pedido existe e ainda pode ser
  // aprovado: manter o carrinho levaria a comprar duas vezes). Na recusa, ele fica para outro cartão
  // ou para um novo pedido. Uma vez só: o clearCart muda a cada renderização.
  useEffect(() => {
    if (cartCleared.current || (payment !== "approved" && payment !== "pending")) return;
    cartCleared.current = true;
    clearCart();
  }, [payment, clearCart]);

  const remainingAttempts = Math.max(0, maxAttempts - attempt);
  const view = paymentView(payment, email, remainingAttempts);

  const handleRetried = (nextAttempt: number) => {
    setAttempt(nextAttempt);
    setPayment("processing");
  };

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
          {payment === "declined" && remainingAttempts > 0 ? (
            <RetryPaymentForm
              orderNumber={order.orderNumber}
              onRetried={handleRetried}
              secondary={
                <Button variant="outline" asChild>
                  <Link to="/cart">Voltar ao carrinho</Link>
                </Button>
              }
            />
          ) : (
            <div className="flex flex-col sm:flex-row gap-3 justify-center">
              <Button asChild>
                <Link to="/">Continuar comprando</Link>
              </Button>
              <Button variant="outline" asChild>
                <Link to="/account/orders">Ver meus pedidos</Link>
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
