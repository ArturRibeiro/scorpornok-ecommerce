import LoadingState from "@/components/LoadingState";
import { useAuth } from "@/context/AuthContext";
import { useEffect, useRef } from "react";
import { useLocation } from "react-router";

// Mostra o conteúdo só com sessão; sem ela, leva ao login do Keycloak, que volta
// para a mesma página (o carrinho continua no localStorage).
export default function RequireAuth({ children }: { children: React.ReactNode }) {
  const { user, loading, login } = useAuth();
  const { pathname, search } = useLocation();
  // O StrictMode roda o efeito duas vezes em dev; um redirecionamento basta.
  const redirecting = useRef(false);

  useEffect(() => {
    if (loading || user || redirecting.current) return;
    redirecting.current = true;
    login(pathname + search);
  }, [loading, user, login, pathname, search]);

  return user ? children : <LoadingState />;
}
