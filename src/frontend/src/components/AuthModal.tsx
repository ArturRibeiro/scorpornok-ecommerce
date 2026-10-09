import LoadingState from "@/components/LoadingState";
import { Button } from "@/components/ui/button";
import { KEYCLOAK_URL, type AuthMode } from "@/lib/auth";
import { X } from "lucide-react";
import { useEffect, useState } from "react";

interface AuthModalProps {
  mode: AuthMode;
  // URL do Keycloak para o iframe; nula enquanto é montada.
  url: string | null;
  failed: boolean;
  onRetry: () => void;
  onClose: () => void;
}

// Mensagem do tema do Keycloak (js/embedded.js) com a altura da página no iframe.
const HEIGHT_MESSAGE = "scorponok:kc-height";
const KEYCLOAK_ORIGIN = new URL(KEYCLOAK_URL).origin;
const DEFAULT_HEIGHT = 560;

// Login e cadastro do Keycloak num iframe, sobre a página atual. O modal fecha
// sozinho quando a sessão chega (o AuthProvider deixa de renderizá-lo).
export default function AuthModal({ mode, url, failed, onRetry, onClose }: AuthModalProps) {
  const [loaded, setLoaded] = useState(false);
  const [height, setHeight] = useState(DEFAULT_HEIGHT);

  useEffect(() => {
    const onMessage = (event: MessageEvent) => {
      if (event.origin !== KEYCLOAK_ORIGIN || event.data?.type !== HEIGHT_MESSAGE) return;
      if (typeof event.data.height === "number") setHeight(Math.ceil(event.data.height));
    };
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    window.addEventListener("message", onMessage);
    window.addEventListener("keydown", onKeyDown);
    // Sem rolar a página por trás do modal.
    const overflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      window.removeEventListener("message", onMessage);
      window.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = overflow;
    };
  }, [onClose]);

  const title = mode === "register" ? "Sign up" : "Sign in";

  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 p-0 sm:p-4"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label={title}
        data-testid="auth-modal"
        className="relative flex h-full w-full flex-col overflow-hidden bg-card shadow-2xl sm:h-auto sm:max-h-[90vh] sm:max-w-md sm:rounded-2xl"
        onClick={(event) => event.stopPropagation()}
      >
        <button
          type="button"
          onClick={onClose}
          aria-label="Close"
          className="absolute right-3 top-3 z-10 rounded-full p-2 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
        >
          <X className="h-5 w-5" />
        </button>

        {failed ? (
          <div className="px-8 py-16 text-center">
            <h2 className="mb-2 text-xl font-bold text-foreground">
              We couldn&apos;t sign you in
            </h2>
            <p className="mb-6 text-muted-foreground">Please try again.</p>
            <Button onClick={onRetry}>Try again</Button>
          </div>
        ) : (
          <>
            {!loaded && <LoadingState />}
            {url && (
              <iframe
                key={url}
                src={url}
                title={title}
                onLoad={() => setLoaded(true)}
                className={loaded ? "block w-full flex-1 border-0 sm:flex-none" : "hidden"}
                style={{ height }}
              />
            )}
          </>
        )}
      </div>
    </div>
  );
}
