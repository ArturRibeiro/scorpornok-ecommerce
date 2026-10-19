// Dados e validação do formulário de checkout. A validação roda antes do
// envio, para que o Orders.Web.Api só receba pedidos completos. Comprador e
// endereço seguem as regras do cadastro do cliente (lib/brazil.ts), e a página
// "Meu cadastro" usa as mesmas funções.

import {
  formatCpf,
  formatPhone,
  formatZipCode,
  isValidCpf,
  isValidFullName,
  isValidPhone,
  isValidState,
  isValidZipCode,
} from "./brazil";
import type { CustomerAddress, CustomerProfile } from "./customers";

// Quem compra: obrigatório no pedido. Vai para o Customers na mensagem do pedido,
// que cria o cadastro na primeira compra.
export interface BuyerDetails {
  fullName: string;
  phone: string;
  cpf: string;
}

// Endereço no formato do cadastro, sempre no Brasil; só o complemento é opcional.
export interface ShippingAddress {
  street: string;
  number: string;
  complement: string;
  district: string;
  city: string;
  state: string;
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

export type CheckoutField = keyof BuyerDetails | keyof ShippingAddress | keyof PaymentDetails;
export type CheckoutErrors = Partial<Record<CheckoutField, string>>;

export const EMPTY_BUYER: BuyerDetails = { fullName: "", phone: "", cpf: "" };

export const EMPTY_ADDRESS: ShippingAddress = {
  street: "",
  number: "",
  complement: "",
  district: "",
  city: "",
  state: "",
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
  buyer: BuyerDetails,
  address: ShippingAddress,
  payment: PaymentDetails,
  today = new Date()
): CheckoutErrors {
  return { ...validateBuyer(buyer), ...validateAddress(address), ...validatePayment(payment, today) };
}

export function validateBuyer(buyer: BuyerDetails): CheckoutErrors {
  const errors: CheckoutErrors = {};
  if (!buyer.fullName.trim()) errors.fullName = "Obrigatório";
  else if (!isValidFullName(buyer.fullName)) errors.fullName = "Informe nome e sobrenome";
  if (!buyer.phone.trim()) errors.phone = "Obrigatório";
  else if (!isValidPhone(buyer.phone)) errors.phone = "Informe o DDD e o número, com 10 ou 11 dígitos";
  if (!buyer.cpf.trim()) errors.cpf = "Obrigatório";
  else if (!isValidCpf(buyer.cpf)) errors.cpf = "CPF inválido";
  return errors;
}

export function validateAddress(address: ShippingAddress): CheckoutErrors {
  const errors: CheckoutErrors = {};
  for (const field of ["street", "number", "district", "city", "state", "zipCode"] as const) {
    if (!address[field].trim()) errors[field] = "Obrigatório";
  }
  if (!errors.state && !isValidState(address.state)) errors.state = "UF inválida";
  if (!errors.zipCode && !isValidZipCode(address.zipCode)) errors.zipCode = "CEP inválido: informe 8 dígitos";
  return errors;
}

// Preenche com o cadastro só os campos ainda vazios: o que o cliente já digitou fica.
export function fillEmpty<T extends object>(current: T, incoming: T): T {
  const next = { ...current };
  for (const key of Object.keys(incoming) as (keyof T)[]) {
    if (!String(current[key] ?? "").trim()) next[key] = incoming[key];
  }
  return next;
}

// Do cadastro (só dígitos) para o formulário (com máscara).
export const buyerFromProfile = (profile: CustomerProfile): BuyerDetails => ({
  fullName: profile.fullName ?? "",
  phone: formatPhone(profile.phone ?? ""),
  cpf: formatCpf(profile.cpf ?? ""),
});

export const addressFromProfile = (address: CustomerAddress | null): ShippingAddress =>
  address
    ? {
        street: address.street,
        number: address.number,
        complement: address.complement ?? "",
        district: address.district,
        city: address.city,
        state: address.state,
        zipCode: formatZipCode(address.zipCode),
      }
    : EMPTY_ADDRESS;

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
