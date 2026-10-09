import AuthModal from "@/components/AuthModal";
import {
  AUTH_CALLBACK_MESSAGE,
  createModalSigninUrl,
  userManager,
  type AuthMode,
} from "@/lib/auth";
import type { User } from "oidc-client-ts";
import React, { createContext, useCallback, useContext, useEffect, useState } from "react";

interface AuthContextProps {
  // Nulo sem sessão ou com a sessão expirada sem renovação.
  user: User | null;
  // Enquanto a sessão salva ainda está sendo lida.
  loading: boolean;
  email: string | null;
  // Abrem o login ou o cadastro do Keycloak num modal, sem sair da página.
  login: () => void;
  register: () => void;
  // Token recusado pela API (401): descarta a sessão local e abre o login.
  expireSession: () => Promise<void>;
  logout: () => Promise<void>;
  // Token atual, renovado se tiver vencido; nulo se não houver sessão.
  getAccessToken: () => Promise<string | null>;
}

const AuthContext = createContext<AuthContextProps | undefined>(undefined);

const activeUser = (user: User | null) => (user && !user.expired ? user : null);

// Modal aberto: o modo e a URL do Keycloak para o iframe (nula enquanto é montada).
interface ModalState {
  mode: AuthMode;
  url: string | null;
  failed: boolean;
}

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState<ModalState | null>(null);

  // Com sessão, o modal não tem mais o que mostrar.
  const applyUser = useCallback((current: User | null) => {
    const active = activeUser(current);
    setUser(active);
    if (active) setModal(null);
  }, []);

  useEffect(() => {
    let cancelled = false;
    userManager
      .getUser()
      .then(async (saved) => {
        // Sessão salva com o access token vencido: tenta renovar antes de desistir.
        if (saved?.expired && saved.refresh_token) {
          return userManager.signinSilent().catch(() => null);
        }
        return saved;
      })
      .then((current) => {
        if (!cancelled) setUser(activeUser(current));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    const onLoaded = (loaded: User) => applyUser(loaded);
    const onUnloaded = () => setUser(null);
    userManager.events.addUserLoaded(onLoaded);
    userManager.events.addUserUnloaded(onUnloaded);
    userManager.events.addUserSignedOut(onUnloaded);

    // Login concluído em outra aba (ex.: o link de confirmação do e-mail): a sessão
    // chega pelo localStorage.
    const onStorage = (event: StorageEvent) => {
      if (!event.key?.startsWith("oidc.user:")) return;
      userManager.getUser().then(applyUser);
    };
    window.addEventListener("storage", onStorage);
    return () => {
      window.removeEventListener("storage", onStorage);
      cancelled = true;
      userManager.events.removeUserLoaded(onLoaded);
      userManager.events.removeUserUnloaded(onUnloaded);
      userManager.events.removeUserSignedOut(onUnloaded);
    };
  }, [applyUser]);

  // O iframe volta para a modal-callback.html, que manda a URL com o code para cá.
  useEffect(() => {
    const onMessage = (event: MessageEvent) => {
      if (event.origin !== window.location.origin) return;
      if (event.data?.type !== AUTH_CALLBACK_MESSAGE) return;
      userManager
        .signinRedirectCallback(event.data.url)
        .then(applyUser)
        .catch(() => setModal((prev) => prev && { ...prev, failed: true }));
    };
    window.addEventListener("message", onMessage);
    return () => window.removeEventListener("message", onMessage);
  }, [applyUser]);

  // O returnTo só é usado se o login terminar fora do modal (link do e-mail numa aba nova).
  const openModal = useCallback((mode: AuthMode) => {
    setModal({ mode, url: null, failed: false });
    const returnTo = window.location.pathname + window.location.search;
    createModalSigninUrl(mode, returnTo)
      .then((url) => setModal((prev) => prev && prev.mode === mode ? { ...prev, url } : prev))
      .catch(() => setModal((prev) => prev && { ...prev, failed: true }));
  }, []);

  const login = useCallback(() => openModal("login"), [openModal]);
  const register = useCallback(() => openModal("register"), [openModal]);
  const closeModal = useCallback(() => setModal(null), []);

  const expireSession = useCallback(async () => {
    await userManager.removeUser();
    setUser(null);
    openModal("login");
  }, [openModal]);

  const logout = useCallback(() => userManager.signoutRedirect(), []);

  const getAccessToken = useCallback(async () => {
    let current = await userManager.getUser();
    if (current?.expired) {
      current = await userManager.signinSilent().catch(() => null);
    }
    setUser(activeUser(current));
    return current && !current.expired ? current.access_token : null;
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        email: (user?.profile.email as string | undefined) ?? null,
        login,
        register,
        expireSession,
        logout,
        getAccessToken,
      }}
    >
      {children}
      {modal && !user && (
        <AuthModal
          mode={modal.mode}
          url={modal.url}
          failed={modal.failed}
          onRetry={() => openModal(modal.mode)}
          onClose={closeModal}
        />
      )}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
