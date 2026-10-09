import LoadingState from "@/components/LoadingState";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/context/AuthContext";
import { useEffect, useRef } from "react";

// Mostra o conteúdo só com sessão; sem ela, abre o login no modal, sobre a mesma
// página (o carrinho continua no localStorage). Fechando o modal, o cliente pode
// abri-lo de novo por aqui.
export default function RequireAuth({ children }: { children: React.ReactNode }) {
  const { user, loading, login, register } = useAuth();
  // O StrictMode roda o efeito duas vezes em dev; abrir uma vez basta.
  const opened = useRef(false);

  useEffect(() => {
    if (loading || user || opened.current) return;
    opened.current = true;
    login();
  }, [loading, user, login]);

  if (user) return children;
  if (loading) return <LoadingState />;
  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16 text-center">
      <h1 className="text-2xl font-bold text-foreground mb-2">Sign in to continue</h1>
      <p className="text-muted-foreground mb-6">
        You need an account to check out. Your cart is saved.
      </p>
      <div className="flex justify-center gap-3">
        <Button onClick={login}>Sign In</Button>
        <Button variant="outline" onClick={register}>
          Sign Up
        </Button>
      </div>
    </div>
  );
}
