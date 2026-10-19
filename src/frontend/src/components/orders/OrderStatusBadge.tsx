import { orderStatusView } from "@/lib/orderStatus";
import { cn } from "@/lib/utils";

// Situação do pedido como selo: "Pago", "Aguardando pagamento" etc.
export default function OrderStatusBadge({ status }: { status: string }) {
  const view = orderStatusView(status);
  return (
    <span
      className={cn("inline-flex items-center rounded-md px-2.5 py-0.5 text-xs font-semibold", view.className)}
      data-testid="order-status"
    >
      {view.label}
    </span>
  );
}
