import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { ORDERS_API_URL } from "./orders";

// Mensagem que o Orders.Web.Api envia pelo hub quando o pagamento é processado.
interface PaymentStatusChanged {
  orderNumber: string;
  approved: boolean;
}

// Conecta ao hub /hubs/orders, acompanha o pedido e resolve com o resultado do
// pagamento (true = aprovado). Não consulta o pedido: o Orders envia a mensagem
// quando o resultado chega, ou na hora, se o pedido já estiver resolvido.
// A conexão é encerrada ao receber o resultado ou quando o signal é abortado.
export async function watchOrderPayment(
  orderNumber: string,
  signal: AbortSignal
): Promise<boolean> {
  const connection = new HubConnectionBuilder()
    .withUrl(`${ORDERS_API_URL}/hubs/orders`)
    .configureLogging(LogLevel.Warning)
    .build();

  const result = new Promise<boolean>((resolve, reject) => {
    connection.on("PaymentStatusChanged", (message: PaymentStatusChanged) => {
      if (message.orderNumber === orderNumber) resolve(message.approved);
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
