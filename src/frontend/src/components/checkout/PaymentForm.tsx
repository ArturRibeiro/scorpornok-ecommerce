import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { CheckoutErrors, PaymentDetails } from "@/lib/checkout";
import FormField from "./FormField";

const INSTALLMENTS = Array.from({ length: 12 }, (_, i) => String(i + 1));

interface PaymentFormProps {
  payment: PaymentDetails;
  errors: CheckoutErrors;
  onChange: (field: keyof PaymentDetails, value: string) => void;
}

export default function PaymentForm({
  payment,
  errors,
  onChange,
}: PaymentFormProps) {
  const field = (name: keyof PaymentDetails, label: string, autoComplete: string) => ({
    id: name,
    label,
    autoComplete,
    value: payment[name],
    error: errors[name],
    onChange: (e: React.ChangeEvent<HTMLInputElement>) =>
      onChange(name, e.target.value),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">Payment</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 sm:grid-cols-2">
        <FormField
          {...field("cardHolderName", "Name on card", "cc-name")}
          className="sm:col-span-2"
        />
        <FormField
          {...field("cardNumber", "Card number", "cc-number")}
          inputMode="numeric"
          placeholder="4111 1111 1111 1111"
          className="sm:col-span-2"
        />
        <div className="grid grid-cols-2 gap-4">
          <FormField
            {...field("expirationMonth", "Exp. month", "cc-exp-month")}
            inputMode="numeric"
            placeholder="MM"
            maxLength={2}
          />
          <FormField
            {...field("expirationYear", "Exp. year", "cc-exp-year")}
            inputMode="numeric"
            placeholder="YYYY"
            maxLength={4}
          />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <FormField
            {...field("cvv", "CVV", "cc-csc")}
            inputMode="numeric"
            maxLength={4}
          />
          <div className="space-y-1.5">
            <label
              id="installments-label"
              className="text-sm font-medium text-foreground"
            >
              Installments
            </label>
            <Select
              value={payment.installments}
              onValueChange={(value) => onChange("installments", value)}
            >
              <SelectTrigger aria-labelledby="installments-label">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {INSTALLMENTS.map((n) => (
                  <SelectItem key={n} value={n}>
                    {n}x
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
