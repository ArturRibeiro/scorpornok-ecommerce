import ErrorState from "@/components/ErrorState";
import LoadingState from "@/components/LoadingState";
import OrderStatusBadge from "@/components/orders/OrderStatusBadge";
import RetryPaymentForm from "@/components/orders/RetryPaymentForm";
import RequireAuth from "@/components/RequireAuth";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { useAuth } from "@/context/AuthContext";
import { useRequest } from "@/hooks/useRequest";
import { formatZipCode } from "@/lib/brazil";
import { watchOrderPayment } from "@/lib/orderHub";
import { formatOrderDate } from "@/lib/orderStatus";
import { getMyOrder, SessionExpiredError, type OrderDetail } from "@/lib/orders";
import { formatPrice } from "@/lib/utils";
import { ArrowLeft, Package } from "lucide-react";
import { useEffect, useEffectEvent, useState } from "react";
import { Link, useParams } from "react-router";

const LOAD_ERROR = "Não foi possível carregar o pedido agora. Tente de novo em instantes.";

// Um pedido de "Meus pedidos", como foi comprado: itens, total, endereço e situação do pagamento.
export default function OrderDetailPage() {
  return (
    <RequireAuth message="Entre para ver os seus pedidos.">
      <OrderDetailLoader />
    </RequireAuth>
  );
}

function OrderDetailLoader() {
  const { orderNumber = "" } = useParams();
  const { user, getAccessToken, expireSession } = useAuth();
  const order = useRequest(`${user?.profile.sub ?? ""}:${orderNumber}`, async (signal) => {
    const token = await getAccessToken();
    if (!token) throw new SessionExpiredError();
    return getMyOrder(orderNumber, token, signal);
  });

  if (order.error instanceof SessionExpiredError) {
    void expireSession();
    return <LoadingState />;
  }
  if (order.error) return <ErrorState onRetry={order.retry} message={LOAD_ERROR} />;
  if (order.loading || order.data === undefined) return <LoadingState />;
  // Pedido de outro cliente ou inexistente: a API não diz qual dos dois.
  if (order.data === null) return <OrderNotFound />;
  // Um resultado de pagamento ou uma nova tentativa recarregam o pedido: situação, tentativas e
  // "pode pagar de novo" vêm sempre da API.
  return <OrderView order={order.data} onChanged={order.retry} />;
}

function OrderView({ order, onChanged }: { order: OrderDetail; onChanged: () => void }) {
  const { getAccessToken } = useAuth();
  const [watchFailed, setWatchFailed] = useState(false);
  const pending = order.status === "Pending";
  // O onChanged muda a cada renderização; o efeito do hub não deve reconectar por isso.
  const onOutcome = useEffectEvent(onChanged);

  // Pagamento em andamento: acompanha a tentativa atual pelo hub, sem consultar o pedido, enquanto a
  // página estiver aberta. Sem conexão, o resultado chega por e-mail.
  useEffect(() => {
    if (!pending) return;
    const controller = new AbortController();
    watchOrderPayment(order.orderNumber, getAccessToken, controller.signal, order.attempt).then(
      () => onOutcome(),
      () => {
        if (!controller.signal.aborted) setWatchFailed(true);
      }
    );
    return () => controller.abort();
  }, [pending, order.orderNumber, order.attempt, getAccessToken]);

  const { street, number, complement, district, city, state, zipCode } = order.address;

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8 max-w-3xl space-y-6">
      <Link
        to="/account/orders"
        className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="h-4 w-4" />
        Meus pedidos
      </Link>

      <div className="flex flex-wrap items-center gap-3">
        <h1 className="text-3xl font-bold text-foreground" data-testid="order-number">
          Pedido {order.orderNumber}
        </h1>
        <OrderStatusBadge status={order.status} />
      </div>
      <p className="text-muted-foreground -mt-4">Feito em {formatOrderDate(order.orderDate)}</p>

      <Card>
        <CardContent className="pt-6 space-y-4">
          <h2 className="text-lg font-semibold text-foreground">Pagamento</h2>
          <PaymentSituation order={order} watchFailed={watchFailed} />
          {order.canRetryPayment && (
            <RetryPaymentForm orderNumber={order.orderNumber} onRetried={onChanged} />
          )}
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6 space-y-4">
          <h2 className="text-lg font-semibold text-foreground">Itens</h2>
          <ul className="space-y-4">
            {order.items.map((item) => (
              <li key={item.productId} className="flex gap-4">
                <div className="h-16 w-16 shrink-0 overflow-hidden rounded-lg bg-muted flex items-center justify-center">
                  {item.pictureUrl ? (
                    <img src={item.pictureUrl} alt={item.productName} className="h-full w-full object-cover" />
                  ) : (
                    <Package className="h-6 w-6 text-muted-foreground" />
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <p className="font-medium text-foreground">{item.productName}</p>
                  <p className="text-sm text-muted-foreground">
                    {item.quantity} × {formatPrice(item.unitPrice)}
                    {item.discount > 0 && ` · Desconto ${formatPrice(item.discount)}`}
                  </p>
                </div>
                <span className="font-semibold text-foreground whitespace-nowrap">
                  {formatPrice(item.subtotal)}
                </span>
              </li>
            ))}
          </ul>
          <Separator />
          <div className="flex justify-between text-lg font-semibold text-foreground">
            <span>Total</span>
            <span>{formatPrice(order.total)}</span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6 space-y-2">
          <h2 className="text-lg font-semibold text-foreground">Endereço de entrega</h2>
          <address className="not-italic text-muted-foreground">
            {street}, {number}
            {complement ? ` – ${complement}` : ""}
            <br />
            {district} – {city}/{state}
            <br />
            CEP {formatZipCode(zipCode)}
          </address>
        </CardContent>
      </Card>
    </div>
  );
}

// O que a situação do pedido significa para o cliente e o que ele pode fazer.
function PaymentSituation({ order, watchFailed }: { order: OrderDetail; watchFailed: boolean }) {
  const remaining = Math.max(0, order.maxAttempts - order.attempt);
  switch (order.status) {
    case "Pending":
      return (
        <p className="text-muted-foreground">
          {watchFailed
            ? "O pagamento está em andamento. Não foi possível acompanhar o resultado agora; ele será enviado para o seu e-mail."
            : "O pagamento está em andamento. Esta página será atualizada assim que o resultado chegar."}
        </p>
      );
    case "Confirmed":
      return <p className="text-muted-foreground">Pagamento aprovado.</p>;
    case "Failed":
      return (
        <p className="text-muted-foreground" data-testid="payment-detail">
          {order.canRetryPayment
            ? `Seu cartão foi recusado e nada foi cobrado. Você pode pagar com outro cartão (${remaining} ${remaining === 1 ? "tentativa restante" : "tentativas restantes"}).`
            : "Seu cartão foi recusado e as tentativas de pagamento deste pedido acabaram. Para comprar estes produtos, refaça a compra."}
        </p>
      );
    case "NotProcessed":
      return (
        <p className="text-muted-foreground">
          Não conseguimos processar o pagamento deste pedido e nada foi cobrado. Para comprar estes
          produtos, refaça a compra.
        </p>
      );
    default:
      return null;
  }
}

function OrderNotFound() {
  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <div className="text-center">
        <div className="text-6xl mb-4">📦</div>
        <h1 className="text-2xl font-bold text-foreground mb-2">Pedido não encontrado</h1>
        <p className="text-muted-foreground mb-6">Não encontramos este pedido entre os seus.</p>
        <Button asChild>
          <Link to="/account/orders">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Ver meus pedidos
          </Link>
        </Button>
      </div>
    </div>
  );
}
