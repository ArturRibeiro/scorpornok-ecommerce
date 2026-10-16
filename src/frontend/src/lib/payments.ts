import type { CheckoutErrors, PaymentDetails } from "./checkout";
import { onlyDigits } from "./checkout";

// Cliente do Payments.Web.Api (tokenização do cartão). Como o catalog.ts e o orders.ts, roda no
// navegador: a URL é a porta publicada no host (VITE_PAYMENTS_API_URL, embutida no build) e a API
// precisa liberar a origem da loja no CORS. O cartão (número e CVV) vai só para cá; ao Orders vai
// apenas o token, que não é guardado no navegador nem reaproveitado entre envios.
const PAYMENTS_API_URL =
  import.meta.env.VITE_PAYMENTS_API_URL ?? "http://localhost:5220";

export interface CardToken {
  token: string;
  last4: string;
  expiresAt: string;
}

export type TokenizeCardResult =
  | { ok: true; token: CardToken }
  // 400: erros por campo do cartão, com os nomes do formulário.
  | { ok: false; errors: CheckoutErrors };

// 201 devolve o token e 400 os erros por campo. Falha de rede e outros status viram exceção:
// não há o que corrigir no formulário.
export async function tokenizeCard(payment: PaymentDetails): Promise<TokenizeCardResult> {
  const response = await fetch(`${PAYMENTS_API_URL}/card-tokens`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      cardHolderName: payment.cardHolderName.trim(),
      cardNumber: onlyDigits(payment.cardNumber),
      expirationMonth: payment.expirationMonth.padStart(2, "0"),
      expirationYear: payment.expirationYear,
      cvv: payment.cvv,
    }),
  });

  if (response.status === 201) {
    return { ok: true, token: await response.json() };
  }
  if (response.status === 400) {
    const body: { errors?: CheckoutErrors } = await response.json();
    return { ok: false, errors: body.errors ?? {} };
  }
  throw new Error(`Payments API respondeu ${response.status} em card-tokens`);
}
