import { graphqlRequest } from "./graphql";
import { SessionExpiredError } from "./session";

// Pedidos pelo gateway GraphQL (subgraph do Orders): registro, nova tentativa e histórico. O
// acompanhamento do pagamento continua no hub SignalR do Orders (orderHub.ts), que usa
// ORDERS_API_URL (a porta publicada no host, VITE_ORDERS_API_URL, embutida no build).
export const ORDERS_API_URL =
  import.meta.env.VITE_ORDERS_API_URL ?? "http://localhost:5224";

// Dados do checkout para o placeOrder. O cliente e o e-mail (para onde vai o
// resultado do pagamento) não vão: o Orders os lê do token. O cartão também
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

// Mantido aqui por compatibilidade com quem já importa de orders.ts.
export { SessionExpiredError };

const PLACE_ORDER = `mutation PlaceOrder($input: PlaceOrderInput!) {
  placeOrder(input: $input) {
    order { orderNumber status total attempt maxAttempts }
    errors { code message }
  }
}`;

interface UserError {
  code: string;
  message: string;
}

// O pedido registrado, ou as mensagens de validação (VALIDATION). Catálogo fora (CATALOG_UNAVAILABLE),
// falha de rede e outros erros viram exceção, porque não há o que corrigir no formulário; sem sessão
// válida, SessionExpiredError. Preço, nome e foto dos itens vêm do Catalog: vão só o id e a quantidade.
export async function createOrder(
  request: CreateOrderRequest,
  accessToken: string
): Promise<CreateOrderResult> {
  const { placeOrder } = await graphqlRequest<{
    placeOrder: { order: CreatedOrder | null; errors: UserError[] };
  }>(
    PLACE_ORDER,
    {
      input: {
        address: request.address,
        customer: request.customer,
        items: request.items.map((item) => ({ productId: item.productId, units: item.units })),
        payment: request.payment,
      },
    },
    { accessToken }
  );

  if (placeOrder.order) return { ok: true, order: placeOrder.order };
  if (placeOrder.errors.some((e) => e.code !== "VALIDATION")) {
    throw new Error(`Orders: ${placeOrder.errors.map((e) => e.message).join("; ")}`);
  }
  return { ok: false, errors: placeOrder.errors.map((e) => e.message) };
}

export type RetryOrderPaymentResult =
  | { ok: true; attempt: number }
  | { ok: false; errors: string[] };

const RETRY_PAYMENT = `mutation RetryOrderPayment($input: RetryOrderPaymentInput!) {
  retryOrderPayment(input: $input) { orderNumber status attempt errors { code message } }
}`;

// Nova tentativa de pagamento de um pedido recusado, com outro cartão (token do Payments). Aceita
// devolve a nova tentativa; sem cartão (VALIDATION), pedido não encontrado (NOT_FOUND) e pedido não
// recusado ou tentativas esgotadas (CONFLICT) devolvem as mensagens. Sem sessão válida,
// SessionExpiredError; falha de rede e outros erros viram exceção.
export async function retryOrderPayment(
  orderNumber: string,
  payment: { cardToken: string; installments: number },
  accessToken: string
): Promise<RetryOrderPaymentResult> {
  const { retryOrderPayment: result } = await graphqlRequest<{
    retryOrderPayment: { attempt: number | null; errors: UserError[] };
  }>(RETRY_PAYMENT, { input: { orderNumber, ...payment } }, { accessToken });

  if (result.attempt !== null && result.errors.length === 0) return { ok: true, attempt: result.attempt };
  return { ok: false, errors: result.errors.map((e) => e.message) };
}

// Histórico do cliente (GET /orders e GET /orders/{orderNumber}): o pedido como foi comprado.
// status é o nome do status do pedido (Pending, Confirmed, Failed, NotProcessed); o texto
// para o cliente vem de orderStatus.ts.
export interface OrderSummary {
  orderNumber: string;
  orderDate: string;
  status: string;
  total: number;
  // Soma das quantidades dos itens.
  itemCount: number;
  pictureUrl: string | null;
}

export interface OrderPage {
  items: OrderSummary[];
  page: number;
  pageSize: number;
  hasMore: boolean;
}

export interface OrderDetail {
  orderNumber: string;
  orderDate: string;
  status: string;
  total: number;
  attempt: number;
  maxAttempts: number;
  // Pagamento recusado com tentativas restantes: o cliente pode pagar de novo com outro cartão.
  canRetryPayment: boolean;
  address: {
    street: string;
    number: string;
    complement: string | null;
    district: string;
    city: string;
    state: string;
    zipCode: string;
  };
  items: {
    productId: number;
    productName: string;
    pictureUrl: string | null;
    unitPrice: number;
    discount: number;
    quantity: number;
    subtotal: number;
  }[];
}

// Pedidos por página em "Meus pedidos".
export const ORDERS_PAGE_SIZE = 10;

const MY_ORDERS = `query MyOrders($page: Int, $pageSize: Int) {
  myOrders(page: $page, pageSize: $pageSize) {
    items { orderNumber orderDate status total itemCount pictureUrl }
    page pageSize hasMore
  }
}`;

// Uma página dos pedidos do cliente do token, do mais recente para o mais antigo.
// Sem sessão válida, SessionExpiredError; falha de rede e outros erros viram exceção.
export async function getMyOrders(
  page: number,
  accessToken: string,
  signal?: AbortSignal
): Promise<OrderPage> {
  const { myOrders } = await graphqlRequest<{ myOrders: OrderPage }>(
    MY_ORDERS,
    { page, pageSize: ORDERS_PAGE_SIZE },
    { accessToken, signal }
  );
  return myOrders;
}

const MY_ORDER = `query MyOrder($orderNumber: String!) {
  myOrder(orderNumber: $orderNumber) {
    orderNumber orderDate status total attempt maxAttempts canRetryPayment
    address { street number complement district city state zipCode }
    items { productId productName pictureUrl unitPrice discount quantity subtotal }
  }
}`;

// Um pedido do cliente; null quando não é dele ou não existe (o gateway não distingue).
export async function getMyOrder(
  orderNumber: string,
  accessToken: string,
  signal?: AbortSignal
): Promise<OrderDetail | null> {
  const { myOrder } = await graphqlRequest<{ myOrder: OrderDetail | null }>(
    MY_ORDER,
    { orderNumber },
    { accessToken, signal }
  );
  return myOrder;
}
