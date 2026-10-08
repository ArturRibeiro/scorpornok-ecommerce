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

export type CheckoutField = "email" | keyof ShippingAddress | keyof PaymentDetails;
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

// Só a forma básica (algo@algo.dominio); o Orders valida de novo.
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validateCheckout(
  email: string,
  address: ShippingAddress,
  payment: PaymentDetails,
  today = new Date()
): CheckoutErrors {
  const errors: CheckoutErrors = {};

  if (!email.trim()) errors.email = "Required";
  else if (!EMAIL_PATTERN.test(email.trim())) errors.email = "Invalid email";

  for (const field of Object.keys(address) as (keyof ShippingAddress)[]) {
    if (!address[field].trim()) errors[field] = "Required";
  }

  if (!payment.cardHolderName.trim()) errors.cardHolderName = "Required";

  if (!/^\d{13,19}$/.test(onlyDigits(payment.cardNumber))) {
    errors.cardNumber = "Card number must have 13 to 19 digits";
  }

  if (!/^\d{3,4}$/.test(payment.cvv)) {
    errors.cvv = "CVV must have 3 or 4 digits";
  }

  const month = Number(payment.expirationMonth);
  const year = Number(payment.expirationYear);
  if (!/^\d{1,2}$/.test(payment.expirationMonth) || month < 1 || month > 12) {
    errors.expirationMonth = "Invalid month";
  } else if (!/^\d{4}$/.test(payment.expirationYear)) {
    errors.expirationYear = "Invalid year";
  } else {
    // O cartão vale até o fim do mês de validade.
    const currentYear = today.getFullYear();
    const currentMonth = today.getMonth() + 1;
    if (year < currentYear || (year === currentYear && month < currentMonth)) {
      errors.expirationYear = "Card is expired";
    }
  }

  return errors;
}
