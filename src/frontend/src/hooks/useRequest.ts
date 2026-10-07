import { useEffect, useEffectEvent, useState } from "react";

type Settled<T> =
  | { tag: string; ok: true; data: T }
  | { tag: string; ok: false; error: unknown };

// Executa load ao montar e sempre que key mudar, cancelando a requisição
// anterior. O resultado guarda a chave e a tentativa que o geraram: se não
// baterem com as atuais, a nova requisição ainda está em andamento.
// Em desenvolvimento, o StrictMode monta o componente duas vezes, então a
// primeira chamada aparece como "canceled" no Network.
export function useRequest<T>(
  key: string,
  load: (signal: AbortSignal) => Promise<T>
) {
  const [attempt, setAttempt] = useState(0);
  const [settled, setSettled] = useState<Settled<T> | null>(null);
  const tag = `${key}#${attempt}`;
  const run = useEffectEvent(load);

  useEffect(() => {
    const controller = new AbortController();
    run(controller.signal).then(
      (data) => setSettled({ tag, ok: true, data }),
      (error) => {
        if (!controller.signal.aborted) setSettled({ tag, ok: false, error });
      }
    );
    return () => controller.abort();
  }, [tag]);

  const current = settled?.tag === tag ? settled : null;
  return {
    loading: current === null,
    data: current?.ok ? current.data : undefined,
    error: current && !current.ok ? current.error : undefined,
    retry: () => setAttempt((prev) => prev + 1),
  };
}
