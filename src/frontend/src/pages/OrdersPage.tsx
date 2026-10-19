import ErrorState from "@/components/ErrorState";
import LoadingState from "@/components/LoadingState";
import OrderStatusBadge from "@/components/orders/OrderStatusBadge";
import RequireAuth from "@/components/RequireAuth";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "@/context/AuthContext";
import { useRequest } from "@/hooks/useRequest";
import { formatOrderDate } from "@/lib/orderStatus";
import { getMyOrders, SessionExpiredError, type OrderPage, type OrderSummary } from "@/lib/orders";
import { formatPrice } from "@/lib/utils";
import { ChevronRight, Loader2, Package, ShoppingBag } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router";

const LOAD_ERROR = "Não foi possível carregar os seus pedidos agora. Tente de novo em instantes.";

// "Meus pedidos": os pedidos do cliente, do mais recente para o mais antigo, em páginas.
export default function OrdersPage() {
  return (
    <RequireAuth message="Entre para ver os seus pedidos.">
      <OrdersLoader />
    </RequireAuth>
  );
}

function OrdersLoader() {
  const { user, getAccessToken, expireSession } = useAuth();
  const first = useRequest(user?.profile.sub ?? "", async (signal) => {
    const token = await getAccessToken();
    if (!token) throw new SessionExpiredError();
    return getMyOrders(1, token, signal);
  });

  if (first.error instanceof SessionExpiredError) {
    void expireSession();
    return <LoadingState />;
  }
  if (first.error) return <ErrorState onRetry={first.retry} message={LOAD_ERROR} />;
  if (first.loading || !first.data) return <LoadingState />;
  return <OrderList first={first.data} />;
}

function OrderList({ first }: { first: OrderPage }) {
  const { getAccessToken, expireSession } = useAuth();
  const [orders, setOrders] = useState<OrderSummary[]>(first.items);
  const [page, setPage] = useState(first.page);
  const [hasMore, setHasMore] = useState(first.hasMore);
  const [loadingMore, setLoadingMore] = useState(false);
  const [moreError, setMoreError] = useState(false);

  // Um pedido novo, registrado em outra aba, empurra a lista: o que já está na tela não se repete.
  const loadMore = async () => {
    setLoadingMore(true);
    setMoreError(false);
    try {
      const token = await getAccessToken();
      if (!token) throw new SessionExpiredError();
      const next = await getMyOrders(page + 1, token);
      setOrders((prev) => {
        const shown = new Set(prev.map((o) => o.orderNumber));
        return [...prev, ...next.items.filter((o) => !shown.has(o.orderNumber))];
      });
      setPage(next.page);
      setHasMore(next.hasMore);
    } catch (error) {
      if (error instanceof SessionExpiredError) {
        await expireSession();
        return;
      }
      setMoreError(true);
    } finally {
      setLoadingMore(false);
    }
  };

  if (orders.length === 0) return <NoOrders />;

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8 max-w-3xl">
      <h1 className="text-3xl font-bold text-foreground mb-6">Meus pedidos</h1>
      <ul className="space-y-3">
        {orders.map((order) => (
          <li key={order.orderNumber}>
            <OrderRow order={order} />
          </li>
        ))}
      </ul>
      <div className="mt-6 text-center space-y-2">
        {moreError && (
          <p role="alert" className="text-sm text-destructive">
            {LOAD_ERROR}
          </p>
        )}
        {hasMore && (
          <Button variant="outline" onClick={loadMore} disabled={loadingMore}>
            {loadingMore && <Loader2 className="h-4 w-4 animate-spin" />}
            Carregar mais
          </Button>
        )}
      </div>
    </div>
  );
}

function OrderRow({ order }: { order: OrderSummary }) {
  return (
    <Link
      to={`/account/orders/${encodeURIComponent(order.orderNumber)}`}
      className="block rounded-xl focus:outline-none focus-visible:ring-2 focus-visible:ring-ring"
    >
      <Card className="hover:bg-muted/50 transition-colors">
        <CardContent className="flex items-center gap-4 py-4">
          <div className="h-16 w-16 shrink-0 overflow-hidden rounded-lg bg-muted flex items-center justify-center">
            {order.pictureUrl ? (
              <img src={order.pictureUrl} alt="" className="h-full w-full object-cover" />
            ) : (
              <Package className="h-6 w-6 text-muted-foreground" />
            )}
          </div>
          <div className="min-w-0 flex-1 space-y-1">
            <div className="flex flex-wrap items-center gap-2">
              <span className="font-semibold text-foreground">Pedido {order.orderNumber}</span>
              <OrderStatusBadge status={order.status} />
            </div>
            <p className="text-sm text-muted-foreground">
              {formatOrderDate(order.orderDate)} · {order.itemCount}{" "}
              {order.itemCount === 1 ? "item" : "itens"}
            </p>
          </div>
          <span className="font-semibold text-foreground whitespace-nowrap">{formatPrice(order.total)}</span>
          <ChevronRight className="h-5 w-5 text-muted-foreground shrink-0" />
        </CardContent>
      </Card>
    </Link>
  );
}

function NoOrders() {
  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-32">
      <div className="max-w-2xl mx-auto text-center">
        <ShoppingBag className="h-24 w-24 text-muted-foreground mx-auto mb-4" />
        <h1 className="text-3xl font-bold text-foreground mb-4">Você ainda não fez nenhuma compra</h1>
        <p className="text-muted-foreground text-lg mb-8">Os pedidos que você fizer aparecem aqui.</p>
        <Button asChild size="lg">
          <Link to="/">Ir para a loja</Link>
        </Button>
      </div>
    </div>
  );
}
