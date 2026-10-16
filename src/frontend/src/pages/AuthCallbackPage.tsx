import LoadingState from "@/components/LoadingState";
import { userManager, type AuthReturnState } from "@/lib/auth";
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router";
import { Button } from "@/components/ui/button";

// O code só pode ser trocado uma vez; o StrictMode monta a página duas vezes em dev.
let callback: ReturnType<typeof userManager.signinRedirectCallback> | null = null;

// Volta do Keycloak (login, cadastro ou confirmação do e-mail): troca o code pelo
// token e segue para a página de onde o cliente saiu.
export default function AuthCallbackPage() {
  const navigate = useNavigate();
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    callback ??= userManager.signinRedirectCallback();
    callback
      .then((user) => {
        const state = user.state as AuthReturnState | undefined;
        navigate(state?.returnTo ?? "/", { replace: true });
      })
      .catch(() => setFailed(true))
      .finally(() => {
        callback = null;
      });
  }, [navigate]);

  if (failed) {
    return (
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-16 text-center">
        <h1 className="text-2xl font-bold text-foreground mb-2">
          Não conseguimos fazer o seu login
        </h1>
        <p className="text-muted-foreground mb-6">Tente de novo.</p>
        <Button asChild>
          <Link to="/">Voltar para a loja</Link>
        </Button>
      </div>
    );
  }
  return <LoadingState />;
}
