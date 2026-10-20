import { searchProducts } from "@/lib/catalog";
import { cn, formatPrice } from "@/lib/utils";
import type { Product } from "@/types/product";
import { Search } from "lucide-react";
import { useEffect, useId, useState } from "react";
import { useNavigate } from "react-router";

// A partir de quantos caracteres sugerir, quantas sugestões e a espera entre as teclas.
const MIN_TERM_LENGTH = 2;
const SUGGESTIONS = 5;
const DEBOUNCE_MS = 250;

type Suggestions = { term: string; ok: true; items: Product[]; total: number } | { term: string; ok: false };

interface SearchBoxProps {
  variant: "desktop" | "mobile";
  autoFocus?: boolean;
  // Chamado ao navegar para um produto ou para os resultados (o painel do celular fecha).
  onNavigate?: () => void;
}

// Campo de busca do cabeçalho, no padrão combobox: sugere produtos enquanto o cliente digita (os 5
// primeiros da mesma busca da página de resultados, no nome, na marca e na descrição), leva ao produto
// escolhido ou, ao confirmar, à página de resultados. Uma falha só esconde as sugestões.
export default function SearchBox({ variant, autoFocus, onNavigate }: SearchBoxProps) {
  const navigate = useNavigate();
  const listId = useId();
  const [query, setQuery] = useState("");
  const [open, setOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const [suggestions, setSuggestions] = useState<Suggestions | null>(null);
  const term = query.trim();

  useEffect(() => {
    if (term.length < MIN_TERM_LENGTH) return;
    const controller = new AbortController();
    const timer = setTimeout(() => {
      searchProducts({ term }, 1, SUGGESTIONS, controller.signal).then(
        (page) => setSuggestions({ term, ok: true, items: page.items, total: page.pageInfo.totalItems }),
        () => {
          if (!controller.signal.aborted) setSuggestions({ term, ok: false });
        }
      );
    }, DEBOUNCE_MS);
    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [term]);

  // Só vale a resposta do termo atual; com menos de 2 caracteres ou com erro, nada é mostrado.
  const current = term.length >= MIN_TERM_LENGTH && suggestions?.term === term && suggestions.ok ? suggestions : null;
  const items = current?.items ?? [];
  // As opções são os produtos e, quando há algum, "Ver todos os resultados" no fim.
  const optionCount = items.length > 0 ? items.length + 1 : 0;
  const expanded = open && current !== null;

  const finish = (path: string) => {
    setOpen(false);
    setActiveIndex(-1);
    onNavigate?.();
    navigate(path);
  };

  const showResults = () => {
    if (term) finish(`/search?q=${encodeURIComponent(term)}`);
  };

  const choose = (index: number) => {
    if (index >= 0 && index < items.length) finish(`/product/${items[index].id}`);
    else showResults();
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "ArrowDown" && optionCount > 0) {
      e.preventDefault();
      setOpen(true);
      setActiveIndex((prev) => (prev + 1) % optionCount);
    } else if (e.key === "ArrowUp" && optionCount > 0) {
      e.preventDefault();
      setOpen(true);
      setActiveIndex((prev) => (prev <= 0 ? optionCount - 1 : prev - 1));
    } else if (e.key === "Escape") {
      setOpen(false);
      setActiveIndex(-1);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (expanded && activeIndex >= 0) choose(activeIndex);
    else showResults();
  };

  const optionId = (index: number) => `${listId}-option-${index}`;

  return (
    <form className="relative w-full" role="search" onSubmit={handleSubmit}>
      <input
        type="search"
        placeholder="Buscar produtos..."
        value={query}
        onChange={(e) => {
          setQuery(e.target.value);
          setOpen(true);
          setActiveIndex(-1);
        }}
        onFocus={() => setOpen(true)}
        onBlur={() => setOpen(false)}
        onKeyDown={handleKeyDown}
        className={cn(
          "w-full pl-10 pr-4 text-sm border border-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-900 focus:border-transparent",
          variant === "desktop" ? "py-2 rounded-full transition-all" : "py-3 rounded-lg"
        )}
        aria-label="Buscar produtos"
        role="combobox"
        aria-autocomplete="list"
        aria-expanded={expanded}
        aria-controls={listId}
        aria-activedescendant={expanded && activeIndex >= 0 ? optionId(activeIndex) : undefined}
        autoFocus={autoFocus}
      />
      <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />

      {expanded && (
        <ul
          id={listId}
          role="listbox"
          aria-label="Sugestões de produtos"
          className="absolute left-0 right-0 top-full mt-2 z-50 overflow-hidden rounded-lg border border-gray-200 bg-white shadow-lg"
          // Evita o blur do campo antes do clique na opção.
          onMouseDown={(e) => e.preventDefault()}
        >
          {items.length === 0 && (
            <li role="presentation" className="px-4 py-3 text-sm text-muted-foreground">
              Nenhum produto encontrado
            </li>
          )}
          {items.map((product, index) => (
            <li
              key={product.id}
              id={optionId(index)}
              role="option"
              aria-selected={activeIndex === index}
              onClick={() => choose(index)}
              onMouseEnter={() => setActiveIndex(index)}
              className={cn("flex cursor-pointer items-center gap-3 px-3 py-2", activeIndex === index && "bg-gray-100")}
            >
              <img src={product.image} alt="" className="h-10 w-10 rounded object-cover" />
              <span className="flex-1 truncate text-sm text-gray-900">{product.name}</span>
              <span className="text-sm font-medium text-gray-900">{formatPrice(product.price)}</span>
            </li>
          ))}
          {items.length > 0 && (
            <li
              id={optionId(items.length)}
              role="option"
              aria-selected={activeIndex === items.length}
              onClick={() => choose(items.length)}
              onMouseEnter={() => setActiveIndex(items.length)}
              className={cn(
                "cursor-pointer border-t border-gray-200 px-4 py-2 text-sm font-medium text-primary",
                activeIndex === items.length && "bg-gray-100"
            )}
          >
            Ver todos os resultados ({current?.total})
          </li>
          )}
        </ul>
      )}
    </form>
  );
}
