import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

const currency = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" })

// Preços em reais, no formato brasileiro: R$ 1.299,99.
export function formatPrice(value: number) {
  return currency.format(value)
}
