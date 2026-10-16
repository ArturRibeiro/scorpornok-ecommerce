import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { CheckoutErrors, ShippingAddress } from "@/lib/checkout";
import FormField from "./FormField";

interface ShippingAddressFormProps {
  address: ShippingAddress;
  errors: CheckoutErrors;
  onChange: (field: keyof ShippingAddress, value: string) => void;
}

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
      onChange(name, e.target.value),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">Endereço de entrega</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 sm:grid-cols-2">
        <FormField
          {...field("street", "Endereço", "street-address")}
          className="sm:col-span-2"
        />
        <FormField {...field("city", "Cidade", "address-level2")} />
        <FormField {...field("state", "Estado", "address-level1")} />
        <FormField {...field("country", "País", "country-name")} />
        <FormField {...field("zipCode", "CEP", "postal-code")} />
      </CardContent>
    </Card>
  );
}
