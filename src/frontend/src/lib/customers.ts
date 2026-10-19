import { SessionExpiredError } from "./orders";

// Cliente do Customers.Web.Api (cadastro do cliente). Como o orders.ts, roda no navegador: a URL é a
// porta publicada no host (VITE_CUSTOMERS_API_URL, embutida no build) e a API precisa liberar a
// origem da loja no CORS. O cliente vem do token: não há id na rota.
const CUSTOMERS_API_URL =
  import.meta.env.VITE_CUSTOMERS_API_URL ?? "http://localhost:5230";

export interface CustomerAddress {
  street: string;
  number: string;
  complement: string | null;
  district: string;
  city: string;
  state: string;
  zipCode: string;
}

// Telefone, CPF e CEP voltam só com dígitos; a loja formata para exibir. Sem cadastro salvo,
// só o e-mail vem preenchido.
export interface CustomerProfile {
  email: string;
  fullName: string | null;
  phone: string | null;
  cpf: string | null;
  address: CustomerAddress | null;
}

export type SaveProfileRequest = Omit<CustomerProfile, "email">;

export type SaveProfileResult =
  | { ok: true; profile: CustomerProfile }
  // 400: erros por campo, nas chaves do JSON ("cpf", "address.zipCode").
  | { ok: false; errors: Record<string, string[]> };

// 401 vira SessionExpiredError; falha de rede e outros status viram exceção.
export async function getProfile(accessToken: string, signal?: AbortSignal): Promise<CustomerProfile> {
  const response = await fetch(`${CUSTOMERS_API_URL}/customers/me`, {
    headers: { Authorization: `Bearer ${accessToken}` },
    signal,
  });
  if (response.status === 401) throw new SessionExpiredError();
  if (!response.ok) throw new Error(`Customers API respondeu ${response.status} em customers/me`);
  return response.json();
}

// 200 devolve o cadastro salvo e 400 os erros por campo, sem nada gravado.
export async function saveProfile(
  profile: SaveProfileRequest,
  accessToken: string
): Promise<SaveProfileResult> {
  const response = await fetch(`${CUSTOMERS_API_URL}/customers/me`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify(profile),
  });
  if (response.status === 401) throw new SessionExpiredError();
  if (response.status === 200) return { ok: true, profile: await response.json() };
  if (response.status === 400) {
    const body: { errors?: Record<string, string[]> } = await response.json();
    return { ok: false, errors: body.errors ?? {} };
  }
  throw new Error(`Customers API respondeu ${response.status} em customers/me`);
}
