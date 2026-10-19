/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_CATALOG_API_URL?: string;
  readonly VITE_ORDERS_API_URL?: string;
  readonly VITE_KEYCLOAK_URL?: string;
  readonly VITE_PAYMENTS_API_URL?: string;
  readonly VITE_CUSTOMERS_API_URL?: string;
}
