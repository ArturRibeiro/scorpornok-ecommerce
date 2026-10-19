import { SessionExpiredError } from "./session";

// Cliente mínimo do gateway GraphQL (Gateway.Web.Api), o ponto de entrada único da loja. Roda no
// navegador: a URL é a porta publicada no host (VITE_GRAPHQL_URL, embutida no build), e o gateway
// precisa liberar a origem da loja no CORS.
export const GRAPHQL_URL =
  import.meta.env.VITE_GRAPHQL_URL ?? "http://localhost:5200/graphql";

interface GraphQLResponse<T> {
  data?: T | null;
  errors?: { message: string; path?: (string | number)[]; extensions?: { code?: string } }[];
}

export interface GraphQLRequestOptions {
  signal?: AbortSignal;
  // Token do Keycloak, para as consultas que exigem login (cadastro, pedidos); o catálogo não precisa.
  accessToken?: string;
}

// Uma consulta ao gateway. Token recusado (401) ou consulta que exige login sem sessão
// (AUTH_NOT_AUTHENTICATED) viram SessionExpiredError. Outros erros, com status diferente de 200 ou
// errors na resposta (por exemplo, com o Catalog fora do ar o gateway responde 200 com o erro no
// campo), lançam Error: a tela mostra o ErrorState.
export async function graphqlRequest<T>(
  query: string,
  variables?: Record<string, unknown>,
  options: GraphQLRequestOptions | AbortSignal = {}
): Promise<T> {
  const { signal, accessToken } = options instanceof AbortSignal ? { signal: options, accessToken: undefined } : options;
  const headers: Record<string, string> = { "Content-Type": "application/json", Accept: "application/json" };
  if (accessToken) headers.Authorization = `Bearer ${accessToken}`;

  const response = await fetch(GRAPHQL_URL, {
    method: "POST",
    headers,
    body: JSON.stringify({ query, variables }),
    signal,
  });
  if (response.status === 401) throw new SessionExpiredError();
  if (!response.ok) {
    throw new Error(`Gateway GraphQL respondeu ${response.status}`);
  }

  const body: GraphQLResponse<T> = await response.json();
  if (body.errors?.some((e) => e.extensions?.code === "AUTH_NOT_AUTHENTICATED")) {
    throw new SessionExpiredError();
  }
  if (body.errors?.length || !body.data) {
    throw new Error(`Gateway GraphQL: ${body.errors?.map((e) => e.message).join("; ") ?? "sem dados"}`);
  }
  return body.data;
}
