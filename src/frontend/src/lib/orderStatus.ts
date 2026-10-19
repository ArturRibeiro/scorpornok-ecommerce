// Situação do pedido para o cliente, a partir do nome do status que o Orders devolve.
// Um status desconhecido aparece como veio, em cinza.
export interface OrderStatusView {
  label: string;
  // Classes do selo (fundo e texto).
  className: string;
}

const STATUS: Record<string, OrderStatusView> = {
  Pending: { label: "Aguardando pagamento", className: "bg-amber-100 text-amber-800" },
  Confirmed: { label: "Pago", className: "bg-green-100 text-green-800" },
  Failed: { label: "Pagamento recusado", className: "bg-red-100 text-red-800" },
  NotProcessed: { label: "Pagamento não processado", className: "bg-gray-200 text-gray-800" },
};

export function orderStatusView(status: string): OrderStatusView {
  return STATUS[status] ?? { label: status, className: "bg-muted text-muted-foreground" };
}

const dateFormat = new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" });

// Data do pedido no fuso do navegador: 19/10/2026, 14:32.
export function formatOrderDate(value: string) {
  return dateFormat.format(new Date(value));
}
