// Regras do cadastro do cliente, as mesmas do backend (Shared.Code/Validation): a loja valida
// antes de enviar, e a API valida de novo. As máscaras só formatam o que o cliente digita;
// a API aceita a pontuação e guarda só os dígitos.

export const BRAZILIAN_STATES = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
  "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO",
] as const;

export const digits = (value: string) => value.replace(/\D/g, "");

// Aplica a máscara aos dígitos já digitados, sem completar o que falta.
function mask(value: string, pattern: string) {
  const d = digits(value);
  let out = "";
  let i = 0;
  for (const ch of pattern) {
    if (i >= d.length) break;
    if (ch === "0") out += d[i++];
    else out += ch;
  }
  return out;
}

export const formatCpf = (value: string) => mask(value, "000.000.000-00");
export const formatZipCode = (value: string) => mask(value, "00000-000");
// Fixo com 10 dígitos, celular com 11.
export const formatPhone = (value: string) =>
  mask(value, digits(value).length > 10 ? "(00) 00000-0000" : "(00) 0000-0000");

export function isValidCpf(value: string) {
  const d = digits(value);
  if (!/^\d{11}$/.test(value.replace(/[.\-\s]/g, "")) || /^(\d)\1{10}$/.test(d)) return false;
  const check = (length: number) => {
    let sum = 0;
    for (let i = 0; i < length; i++) sum += Number(d[i]) * (length + 1 - i);
    const rest = sum % 11;
    return rest < 2 ? 0 : 11 - rest;
  };
  return check(9) === Number(d[9]) && check(10) === Number(d[10]);
}

export const isValidPhone = (value: string) =>
  /^[1-9]{2}\d{8,9}$/.test(value.replace(/[()\-\s]/g, ""));

export const isValidZipCode = (value: string) => /^\d{8}$/.test(value.replace(/[.\-\s]/g, ""));

export const isValidState = (value: string) =>
  (BRAZILIAN_STATES as readonly string[]).includes(value.trim().toUpperCase());

// Nome e sobrenome, até 120 caracteres.
export function isValidFullName(value: string) {
  const name = value.trim().replace(/\s+/g, " ");
  return name.length <= 120 && name.split(" ").filter((word) => /\p{L}/u.test(word)).length >= 2;
}
