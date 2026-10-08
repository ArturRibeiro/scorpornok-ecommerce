// Cliente do Orders.Web.Api. Como o catalog.ts, roda no navegador: a URL é a
// porta publicada no host (VITE_ORDERS_API_URL, embutida no build) e a API
// precisa liberar a origem do front no CORS.
const ORDERS_API_URL =
  import.meta.env.VITE_ORDERS_API_URL ?? "http://localhost:5224";

// Formato do CreateCommand no backend.
export interface CreateOrderRequest {
  userId: string;
  address: {
    street: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
  };
  items: {
    productId: number;
    productName: string;
    pictureUrl: string;
    unitPrice: number;
    discount: number;
    units: number;
  }[];
  card: {
    // O servidor ainda não usa o OrderId do pagamento.
    orderId: string;
    cardHolderName: string;
    cardNumber: string;
    expirationMonth: string;
    expirationYear: string;
    cvv: string;
    amount: number;
    installments: number;
  };
}

export interface CreatedOrder {
  orderNumber: string;
  status: string;
  total: number;
}

export type CreateOrderResult =
  | { ok: true; order: CreatedOrder }
  | { ok: false; errors: string[] };

// 201 devolve o pedido e 400 as mensagens de validação. Falha de rede e outros
// status viram exceção, porque não há o que o cliente corrigir no formulário.
export async function createOrder(
  request: CreateOrderRequest
): Promise<CreateOrderResult> {
  const response = await fetch(`${ORDERS_API_URL}/createOrder`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (response.status === 201) {
    return { ok: true, order: await response.json() };
  }
  if (response.status === 400) {
    const body: { errors?: string[] } = await response.json();
    return { ok: false, errors: body.errors ?? [] };
  }
  throw new Error(`Orders API respondeu ${response.status} em createOrder`);
}
