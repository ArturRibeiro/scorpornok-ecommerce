import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useRequest } from "@/hooks/useRequest";
import { getBrands, type ProductSearchFilters } from "@/lib/catalog";
import { hasFilters, parsePriceInput } from "@/lib/searchParams";
import { useState } from "react";

interface SearchFiltersProps {
  filters: ProductSearchFilters;
  onChange: (changes: Partial<ProductSearchFilters>) => void;
  onClear: () => void;
}

// Filtros da página de resultados: marcas (aplicadas ao marcar) e faixa de preço (aplicada em "Aplicar").
export default function SearchFilters({ filters, onChange, onClear }: SearchFiltersProps) {
  const brands = useRequest("brands", getBrands);
  const selected = filters.brands ?? [];

  const toggleBrand = (brand: string) =>
    onChange({ brands: selected.includes(brand) ? selected.filter((b) => b !== brand) : [...selected, brand] });

  return (
    <div className="space-y-6">
      <fieldset>
        <legend className="mb-3 text-sm font-semibold text-foreground">Marca</legend>
        {brands.loading && <p className="text-sm text-muted-foreground">Carregando marcas...</p>}
        {brands.error !== undefined && (
          <p className="text-sm text-muted-foreground">
            Não foi possível carregar as marcas.{" "}
            <button type="button" className="underline" onClick={brands.retry}>
              Tentar de novo
            </button>
          </p>
        )}
        <div className="space-y-2">
          {brands.data?.map((brand) => (
            <label key={brand} className="flex cursor-pointer items-center gap-2 text-sm text-foreground">
              <input
                type="checkbox"
                className="h-4 w-4 accent-primary"
                checked={selected.includes(brand)}
                onChange={() => toggleBrand(brand)}
              />
              {brand}
            </label>
          ))}
        </div>
      </fieldset>

      {/* A chave recria o formulário quando a faixa da URL muda (ex.: "Limpar filtros"). */}
      <PriceFilter
        key={`${filters.minPrice ?? ""}-${filters.maxPrice ?? ""}`}
        minPrice={filters.minPrice}
        maxPrice={filters.maxPrice}
        onApply={(minPrice, maxPrice) => onChange({ minPrice, maxPrice })}
      />

      {hasFilters(filters) && (
        <Button type="button" variant="outline" className="w-full" onClick={onClear}>
          Limpar filtros
        </Button>
      )}
    </div>
  );
}

const formatInput = (value?: number) => (value === undefined ? "" : String(value).replace(".", ","));

function PriceFilter({
  minPrice,
  maxPrice,
  onApply,
}: {
  minPrice?: number;
  maxPrice?: number;
  onApply: (minPrice?: number, maxPrice?: number) => void;
}) {
  const [min, setMin] = useState(formatInput(minPrice));
  const [max, setMax] = useState(formatInput(maxPrice));
  const [error, setError] = useState<string | null>(null);

  const apply = (e: React.FormEvent) => {
    e.preventDefault();
    const parsedMin = parsePriceInput(min);
    const parsedMax = parsePriceInput(max);
    if (parsedMin === null || parsedMax === null) {
      setError("Informe um valor válido, como 199,90.");
    } else if (parsedMin !== undefined && parsedMax !== undefined && parsedMin > parsedMax) {
      setError("A faixa é inválida: o preço mínimo é maior que o máximo.");
    } else {
      setError(null);
      onApply(parsedMin, parsedMax);
    }
  };

  return (
    <form onSubmit={apply} noValidate>
      <fieldset>
        <legend className="mb-3 text-sm font-semibold text-foreground">Preço (R$)</legend>
        <div className="flex items-center gap-2">
          <Input
            aria-label="Preço mínimo"
            inputMode="decimal"
            placeholder="Mínimo"
            value={min}
            onChange={(e) => setMin(e.target.value)}
            aria-invalid={error !== null}
          />
          <span className="text-muted-foreground">até</span>
          <Input
            aria-label="Preço máximo"
            inputMode="decimal"
            placeholder="Máximo"
            value={max}
            onChange={(e) => setMax(e.target.value)}
            aria-invalid={error !== null}
          />
        </div>
        {error && (
          <p className="mt-2 text-sm text-destructive" role="alert">
            {error}
          </p>
        )}
        <Button type="submit" variant="secondary" className="mt-3 w-full">
          Aplicar
        </Button>
      </fieldset>
    </form>
  );
}
