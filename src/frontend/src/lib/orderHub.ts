import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { ORDERS_API_URL } from "./orders";

// Mensagem que o Orders.Web.Api envia pelo hub quando o pagamento tem desfecho.
// status é o status do pedido (Confirmed, Failed ou NotProcessed); approved é o
// campo antigo, mantido pelo Orders por compatibilidade.
interface PaymentStatusChanged {
  orderNumber: string;
  approved: boolean;
  status?: string;
  // Tentativa a que o desfecho pertence; mensagens antigas, sem o campo, valem como 1.
  attempt?: number;
}

// Desfecho do pagamento: "notProcessed" é diferente de "declined": nada foi
// cobrado, porque o pagamento não chegou a ser processado.
export type PaymentOutcome = "approved" | "declined" | "notProcessed";

const toOutcome = (message: PaymentStatusChanged): PaymentOutcome =>
  message.status === "NotProcessed" ? "notProcessed" : message.approved ? "approved" : "declined";

// Conecta ao hub /hubs/orders, acompanha o pedido e resolve com o desfecho do
// pagamento. Não consulta o pedido: o Orders envia a mensagem
// quando o resultado chega, ou na hora, se o pedido já estiver resolvido.
// O hub exige o token do cliente (o SignalR o manda em access_token na query do
// WebSocket) e só aceita o dono do pedido; sem token, a conexão é recusada.
// A conexão é encerrada ao receber o resultado ou quando o signal é abortado.
// attempt é a tentativa acompanhada: o desfecho de uma tentativa anterior (que pode chegar
// atrasado, ou na hora, pela consulta do WatchOrder) é ignorado.
export async function watchOrderPayment(
  orderNumber: string,
  getAccessToken: () => Promise<string | null>,
  signal: AbortSignal,
  attempt = 1
): Promise<PaymentOutcome> {
  const connection = new HubConnectionBuilder()
    .withUrl(`${ORDERS_API_URL}/hubs/orders`, {
      accessTokenFactory: async () => (await getAccessToken()) ?? "",
    })
    .configureLogging(LogLevel.Warning)
    .build();

  const result = new Promise<PaymentOutcome>((resolve, reject) => {
    connection.on("PaymentStatusChanged", (message: PaymentStatusChanged) => {
      if (message.orderNumber === orderNumber && (message.attempt ?? 1) >= attempt)
        resolve(toOutcome(message));
    });
    signal.addEventListener("abort", () => reject(signal.reason), { once: true });
  });
  // Se start/invoke falharem, ninguém espera result; evita rejeição sem tratamento.
  result.catch(() => undefined);

  try {
    await connection.start();
    await connection.invoke("WatchOrder", orderNumber);
    return await result;
  } finally {
    await connection.stop();
  }
}
