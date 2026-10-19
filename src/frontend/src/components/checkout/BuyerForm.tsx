import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCpf, formatPhone } from "@/lib/brazil";
import type { BuyerDetails, CheckoutErrors } from "@/lib/checkout";
import FormField from "./FormField";

interface BuyerFormProps {
  title: string;
  buyer: BuyerDetails;
  errors: CheckoutErrors;
  onChange: (field: keyof BuyerDetails, value: string) => void;
}

const MASKS: Partial<Record<keyof BuyerDetails, (value: string) => string>> = {
  phone: formatPhone,
  cpf: formatCpf,
};

// Nome, telefone e CPF (checkout e "Meu cadastro"), com máscara no telefone e no CPF.
export default function BuyerForm({ title, buyer, errors, onChange }: BuyerFormProps) {
  const field = (name: keyof BuyerDetails, label: string, autoComplete: string) => ({
    id: name,
    label,
    autoComplete,
    value: buyer[name],
    error: errors[name],
    onChange: (e: React.ChangeEvent<HTMLInputElement>) =>
      onChange(name, MASKS[name]?.(e.target.value) ?? e.target.value),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">{title}</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 sm:grid-cols-2">
        <FormField {...field("fullName", "Nome completo", "name")} className="sm:col-span-2" />
        <FormField {...field("phone", "Telefone", "tel-national")} inputMode="tel"
          placeholder="(21) 99999-8888" />
        <FormField {...field("cpf", "CPF", "off")} inputMode="numeric" placeholder="000.000.000-00" />
      </CardContent>
    </Card>
  );
}
