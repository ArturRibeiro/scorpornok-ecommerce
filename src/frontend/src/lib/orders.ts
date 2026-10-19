// Cliente do Orders.Web.Api. Como o catalog.ts, roda no navegador: a URL é a
// porta publicada no host (VITE_ORDERS_API_URL, embutida no build) e a API
// precisa liberar a origem do front no CORS.
export const ORDERS_API_URL =
  import.meta.env.VITE_ORDERS_API_URL ?? "http://localhost:5224";

// Formato do CreateCommand no backend. O cliente e o e-mail (para onde vai o
// resultado do pagamento) não vão no corpo: o Orders os lê do token. O cartão também
// não: vai só o token gerado pelo Payments (tokenizeCard, em payments.ts).
export interface CreateOrderRequest {
  // Sempre no Brasil: o Orders fixa o país.
  address: {
    street: string;
    number: string;
    complement: string;
    district: string;
    city: string;
    state: string;
    zipCode: string;
  };
  // Comprador obrigatório; não fica no pedido, segue para o Customers.
  customer: {
    fullName: string;
    phone: string;
    cpf: string;
  };
  items: {
    productId: number;
    productName: string;
    pictureUrl: string;
    unitPrice: number;
    discount: number;
    units: number;
  }[];
  payment: {
    cardToken: string;
    installments: number;
  };
}

export interface CreatedOrder {
  orderNumber: string;
  status: string;
  total: number;
  // Tentativa de pagamento atual e o máximo por pedido: a tela de recusa só oferece
  // "Tentar com outro cartão" enquanto attempt < maxAttempts.
  attempt: number;
  maxAttempts: number;
}

export type CreateOrderResult =
  | { ok: true; order: CreatedOrder }
  | { ok: false; errors: string[] };

// 401: a sessão venceu (ou o token foi recusado) e o cliente precisa entrar de novo.
export class SessionExpiredError extends Error {
  constructor() {
    super("Sessão expirada");
  }
}

// 201 devolve o pedido e 400 as mensagens de validação. 401 vira SessionExpiredError;
// falha de rede e outros status viram exceção, porque não há o que corrigir no formulário.
export async function createOrder(
  request: CreateOrderRequest,
  accessToken: string
): Promise<CreateOrderResult> {
  const response = await fetch(`${ORDERS_API_URL}/createOrder`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify(request),
  });

  if (response.status === 401) {
    throw new SessionExpiredError();
  }

  if (response.status === 201) {
    return { ok: true, order: await response.json() };
  }
  if (response.status === 400) {
    const body: { errors?: string[] } = await response.json();
    return { ok: false, errors: body.errors ?? [] };
  }
  throw new Error(`Orders API respondeu ${response.status} em createOrder`);
}

export type RetryOrderPaymentResult =
  | { ok: true; attempt: number }
  | { ok: false; errors: string[] };

// Nova tentativa de pagamento de um pedido recusado, com outro cartão (token do Payments).
// 202 devolve a nova tentativa; 400 (sem cartão), 404 (pedido não encontrado) e 409 (pedido não
// recusado ou tentativas esgotadas) devolvem as mensagens. 401 vira SessionExpiredError; falha de
// rede e outros status viram exceção.
export async function retryOrderPayment(
  orderNumber: string,
  payment: { cardToken: string; installments: number },
  accessToken: string
): Promise<RetryOrderPaymentResult> {
  const response = await fetch(
    `${ORDERS_API_URL}/orders/${encodeURIComponent(orderNumber)}/payment`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify(payment),
    }
  );

  if (response.status === 401) {
    throw new SessionExpiredError();
  }
  if (response.status === 202) {
    const body: { attempt: number } = await response.json();
    return { ok: true, attempt: body.attempt };
  }
  if ([400, 404, 409].includes(response.status)) {
    const body: { errors?: string[] } = await response.json();
    return { ok: false, errors: body.errors ?? [] };
  }
  throw new Error(`Orders API respondeu ${response.status} em orders/payment`);
}
