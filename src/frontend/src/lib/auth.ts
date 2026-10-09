import { UserManager, WebStorageStateStore } from "oidc-client-ts";

// Login e cadastro pelo formulário do Keycloak (Authorization Code + PKCE, client
// público). Como as APIs, a URL é a que o navegador alcança (VITE_KEYCLOAK_URL,
// embutida no build). A sessão fica no localStorage, compartilhada entre abas
// como o carrinho, e é renovada em silêncio pelo refresh token.
export const KEYCLOAK_URL =
  import.meta.env.VITE_KEYCLOAK_URL ?? "http://localhost:8080";
const REALM = "scorponok";
const CLIENT_ID = "scorponok-web";

export const AUTH_CALLBACK_PATH = "/auth/callback";

export const userManager = new UserManager({
  authority: `${KEYCLOAK_URL}/realms/${REALM}`,
  client_id: CLIENT_ID,
  redirect_uri: `${window.location.origin}${AUTH_CALLBACK_PATH}`,
  post_logout_redirect_uri: window.location.origin,
  scope: "openid email",
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  automaticSilentRenew: true,
});

// Para onde voltar depois do login, guardado no state do redirecionamento.
export interface AuthReturnState {
  returnTo: string;
}
