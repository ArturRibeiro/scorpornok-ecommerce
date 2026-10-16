// Dados e validação do formulário de checkout. A validação roda antes do
// envio, para que o Orders.Web.Api só receba pedidos completos.

export interface ShippingAddress {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

export interface PaymentDetails {
  cardHolderName: string;
  cardNumber: string;
  expirationMonth: string;
  expirationYear: string;
  cvv: string;
  installments: string;
}

export type CheckoutField = keyof ShippingAddress | keyof PaymentDetails;
export type CheckoutErrors = Partial<Record<CheckoutField, string>>;

export const EMPTY_ADDRESS: ShippingAddress = {
  street: "",
  city: "",
  state: "",
  country: "",
  zipCode: "",
};

export const EMPTY_PAYMENT: PaymentDetails = {
  cardHolderName: "",
  cardNumber: "",
  expirationMonth: "",
  expirationYear: "",
  cvv: "",
  installments: "1",
};

// O número pode ser digitado com espaços ou hífens; só os dígitos vão para a API.
export const onlyDigits = (value: string) => value.replace(/[\s-]/g, "");

// O e-mail não é validado aqui: é o da conta, que o Orders lê do token.
export function validateCheckout(
  address: ShippingAddress,
  payment: PaymentDetails,
  today = new Date()
): CheckoutErrors {
  const errors: CheckoutErrors = {};

  for (const field of Object.keys(address) as (keyof ShippingAddress)[]) {
    if (!address[field].trim()) errors[field] = "Obrigatório";
  }

  return { ...errors, ...validatePayment(payment, today) };
}

// Só o cartão: usada também na nova tentativa de pagamento, em que o endereço não muda.
export function validatePayment(payment: PaymentDetails, today = new Date()): CheckoutErrors {
  const errors: CheckoutErrors = {};

  if (!payment.cardHolderName.trim()) errors.cardHolderName = "Obrigatório";

  if (!/^\d{13,19}$/.test(onlyDigits(payment.cardNumber))) {
    errors.cardNumber = "O número do cartão deve ter de 13 a 19 dígitos";
  }

  if (!/^\d{3,4}$/.test(payment.cvv)) {
    errors.cvv = "O CVV deve ter 3 ou 4 dígitos";
  }

  const month = Number(payment.expirationMonth);
  const year = Number(payment.expirationYear);
  if (!/^\d{1,2}$/.test(payment.expirationMonth) || month < 1 || month > 12) {
    errors.expirationMonth = "Mês inválido";
  } else if (!/^\d{4}$/.test(payment.expirationYear)) {
    errors.expirationYear = "Ano inválido";
  } else {
    // O cartão vale até o fim do mês de validade.
    const currentYear = today.getFullYear();
    const currentMonth = today.getMonth() + 1;
    if (year < currentYear || (year === currentYear && month < currentMonth)) {
      errors.expirationYear = "Cartão vencido";
    }
  }

  return errors;
}
