import type { CheckoutErrors, PaymentDetails } from "./checkout";
import { onlyDigits } from "./checkout";
import { graphqlRequest } from "./graphql";

// Tokenização do cartão pelo gateway GraphQL (subgraph do Payments). O cartão (número e CVV) passa pelo
// gateway, que não o registra, até o Payments; ao Orders vai apenas o token, que não é guardado no navegador
// nem reaproveitado entre envios. Sem login, como antes: o token sozinho não cobra nada.

export interface CardToken {
  token: string;
  last4: string;
  expiresAt: string;
}

export type TokenizeCardResult =
  | { ok: true; token: CardToken }
  // Erros por campo do cartão, com os nomes do formulário.
  | { ok: false; errors: CheckoutErrors };

const TOKENIZE_CARD = `mutation TokenizeCard($input: TokenizeCardInput!) {
  tokenizeCard(input: $input) {
    token { token last4 expiresAt }
    errors { field message }
  }
}`;

// O token, ou os erros por campo. Falha de rede e outros erros viram exceção: não há o que corrigir no
// formulário.
export async function tokenizeCard(payment: PaymentDetails): Promise<TokenizeCardResult> {
  const { tokenizeCard: result } = await graphqlRequest<{
    tokenizeCard: { token: CardToken | null; errors: { field: string; message: string }[] };
  }>(TOKENIZE_CARD, {
    input: {
      cardHolderName: payment.cardHolderName.trim(),
      cardNumber: onlyDigits(payment.cardNumber),
      expirationMonth: payment.expirationMonth.padStart(2, "0"),
      expirationYear: payment.expirationYear,
      cvv: payment.cvv,
    },
  });

  if (result.token) return { ok: true, token: result.token };
  return {
    ok: false,
    errors: Object.fromEntries(result.errors.map((e) => [e.field, e.message])) as CheckoutErrors,
  };
}
