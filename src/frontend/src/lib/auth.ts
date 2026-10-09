import {
  OidcClient,
  UserManager,
  WebStorageStateStore,
  type UserManagerSettings,
} from "oidc-client-ts";

// Login e cadastro pelo formulário do Keycloak (Authorization Code + PKCE, client
// público), mostrado num modal da loja (iframe). Como as APIs, a URL é a que o navegador alcança (VITE_KEYCLOAK_URL,
// embutida no build). A sessão fica no localStorage, compartilhada entre abas
// como o carrinho, e é renovada em silêncio pelo refresh token.
export const KEYCLOAK_URL =
  import.meta.env.VITE_KEYCLOAK_URL ?? "http://localhost:8080";
const REALM = "scorponok";
const CLIENT_ID = "scorponok-web";

export const AUTH_CALLBACK_PATH = "/auth/callback";
// Retorno do Keycloak dentro do modal: página estática (public/) que entrega a URL
// à janela da loja. Fora do iframe (link de confirmação aberto em outra aba), ela
// repassa para o AUTH_CALLBACK_PATH.
export const AUTH_MODAL_CALLBACK_PATH = "/auth/modal-callback.html";
export const AUTH_CALLBACK_MESSAGE = "scorponok:auth-callback";

const settings: UserManagerSettings = {
  authority: `${KEYCLOAK_URL}/realms/${REALM}`,
  client_id: CLIENT_ID,
  redirect_uri: `${window.location.origin}${AUTH_CALLBACK_PATH}`,
  post_logout_redirect_uri: window.location.origin,
  scope: "openid email",
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  automaticSilentRenew: true,
};

export const userManager = new UserManager(settings);

// Mesmas configurações e o mesmo armazenamento do state (localStorage) do
// userManager: ele monta a URL de autorização sem navegar, e o userManager
// conclui o login com a URL de retorno.
const client = new OidcClient(settings);

// Para onde voltar depois do login, guardado no state do redirecionamento.
export interface AuthReturnState {
  returnTo: string;
}

export type AuthMode = "login" | "register";

// URL do Keycloak para o iframe do modal; prompt=create abre direto o cadastro.
export async function createModalSigninUrl(mode: AuthMode, returnTo: string) {
  const request = await client.createSigninRequest({
    request_type: "si:r",
    redirect_uri: `${window.location.origin}${AUTH_MODAL_CALLBACK_PATH}`,
    state: { returnTo } satisfies AuthReturnState,
    prompt: mode === "register" ? "create" : undefined,
  });
  return request.url;
}
