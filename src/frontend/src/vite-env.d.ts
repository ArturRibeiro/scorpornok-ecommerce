/// <reference types="vite/client" />

interface ImportMetaEnv {
  // Gateway GraphQL (catálogo e cadastro vêm por ele).
  readonly VITE_GRAPHQL_URL?: string;
  readonly VITE_ORDERS_API_URL?: string;
  readonly VITE_KEYCLOAK_URL?: string;
  readonly VITE_PAYMENTS_API_URL?: string;
}
