import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { BRAZILIAN_STATES, formatZipCode } from "@/lib/brazil";
import type { CheckoutErrors, ShippingAddress } from "@/lib/checkout";
import { cn } from "@/lib/utils";
import FormField from "./FormField";

interface ShippingAddressFormProps {
  address: ShippingAddress;
  errors: CheckoutErrors;
  onChange: (field: keyof ShippingAddress, value: string) => void;
}

// Endereço no formato do cadastro (checkout e "Meu cadastro"). A entrega é só no Brasil:
// não há campo de país, e a UF vem de uma lista.
export default function ShippingAddressForm({
  address,
  errors,
  onChange,
}: ShippingAddressFormProps) {
  const field = (name: keyof ShippingAddress, label: string, autoComplete: string) => ({
    id: name,
    label,
    autoComplete,
    value: address[name],
    error: errors[name],
    onChange: (e: React.ChangeEvent<HTMLInputElement>) =>
      onChange(name, name === "zipCode" ? formatZipCode(e.target.value) : e.target.value),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">Endereço de entrega</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 sm:grid-cols-6">
        <FormField {...field("zipCode", "CEP", "postal-code")} inputMode="numeric"
          placeholder="00000-000" className="sm:col-span-2" />
        <FormField {...field("street", "Rua", "address-line1")} className="sm:col-span-4" />
        <FormField {...field("number", "Número", "address-line2")} className="sm:col-span-2" />
        <FormField {...field("complement", "Complemento (opcional)", "address-line3")}
          className="sm:col-span-4" />
        <FormField {...field("district", "Bairro", "address-level3")} className="sm:col-span-2" />
        <FormField {...field("city", "Cidade", "address-level2")} className="sm:col-span-3" />
        <div className="space-y-1.5 sm:col-span-1">
          <label id="state-label" className="text-sm font-medium text-foreground">
            UF
          </label>
          <Select value={address.state} onValueChange={(value) => onChange("state", value)}>
            <SelectTrigger
              aria-labelledby="state-label"
              aria-invalid={!!errors.state}
              aria-describedby={errors.state ? "state-error" : undefined}
              className={cn("w-full", errors.state && "border-destructive")}
            >
              <SelectValue placeholder="UF" />
            </SelectTrigger>
            <SelectContent>
              {BRAZILIAN_STATES.map((uf) => (
                <SelectItem key={uf} value={uf}>
                  {uf}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {errors.state && (
            <p id="state-error" className="text-xs text-destructive">
              {errors.state}
            </p>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
