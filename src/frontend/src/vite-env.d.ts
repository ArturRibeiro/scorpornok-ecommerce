/// <reference types="vite/client" />

interface ImportMetaEnv {
  // Gateway GraphQL (catálogo, cadastro, pedidos e tokenização do cartão vêm por ele).
  readonly VITE_GRAPHQL_URL?: string;
  readonly VITE_ORDERS_API_URL?: string;
  readonly VITE_KEYCLOAK_URL?: string;
}
