import { userManager, type AuthReturnState } from "@/lib/auth";
import type { User } from "oidc-client-ts";
import React, { createContext, useCallback, useContext, useEffect, useState } from "react";

interface AuthContextProps {
  // Nulo sem sessão ou com a sessão expirada sem renovação.
  user: User | null;
  // Enquanto a sessão salva ainda está sendo lida.
  loading: boolean;
  email: string | null;
  login: (returnTo: string) => Promise<void>;
  register: (returnTo: string) => Promise<void>;
  logout: () => Promise<void>;
  // Token atual, renovado se tiver vencido; nulo se não houver sessão.
  getAccessToken: () => Promise<string | null>;
}

const AuthContext = createContext<AuthContextProps | undefined>(undefined);

const activeUser = (user: User | null) => (user && !user.expired ? user : null);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

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

    const onLoaded = (loaded: User) => setUser(activeUser(loaded));
    const onUnloaded = () => setUser(null);
    userManager.events.addUserLoaded(onLoaded);
    userManager.events.addUserUnloaded(onUnloaded);
    userManager.events.addUserSignedOut(onUnloaded);
    return () => {
      cancelled = true;
      userManager.events.removeUserLoaded(onLoaded);
      userManager.events.removeUserUnloaded(onUnloaded);
      userManager.events.removeUserSignedOut(onUnloaded);
    };
  }, []);

  const login = useCallback(
    (returnTo: string) =>
      userManager.signinRedirect({ state: { returnTo } satisfies AuthReturnState }),
    []
  );

  // prompt=create abre direto a tela de cadastro do Keycloak.
  const register = useCallback(
    (returnTo: string) =>
      userManager.signinRedirect({
        state: { returnTo } satisfies AuthReturnState,
        prompt: "create",
      }),
    []
  );

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
        logout,
        getAccessToken,
      }}
    >
      {children}
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
