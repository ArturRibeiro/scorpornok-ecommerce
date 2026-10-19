// Cliente mínimo do gateway GraphQL (Gateway.Web.Api), o ponto de entrada único da loja. Roda no
// navegador: a URL é a porta publicada no host (VITE_GRAPHQL_URL, embutida no build), e o gateway
// precisa liberar a origem da loja no CORS.
export const GRAPHQL_URL =
  import.meta.env.VITE_GRAPHQL_URL ?? "http://localhost:5200/graphql";

interface GraphQLResponse<T> {
  data?: T | null;
  errors?: { message: string; path?: (string | number)[] }[];
}

// Uma consulta ao gateway. Lança erro quando a resposta não é 200 ou traz errors (por exemplo, com
// o Catalog fora do ar o gateway responde 200 com o erro no campo): a tela mostra o ErrorState.
export async function graphqlRequest<T>(
  query: string,
  variables?: Record<string, unknown>,
  signal?: AbortSignal
): Promise<T> {
  const response = await fetch(GRAPHQL_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({ query, variables }),
    signal,
  });
  if (!response.ok) {
    throw new Error(`Gateway GraphQL respondeu ${response.status}`);
  }

  const body: GraphQLResponse<T> = await response.json();
  if (body.errors?.length || !body.data) {
    throw new Error(`Gateway GraphQL: ${body.errors?.map((e) => e.message).join("; ") ?? "sem dados"}`);
  }
  return body.data;
}
