import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import FormField from "./FormField";

interface ContactFormProps {
  email: string;
  error?: string;
  onChange: (value: string) => void;
}

// O resultado do pagamento é enviado para este e-mail.
export default function ContactForm({ email, error, onChange }: ContactFormProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">Contact</CardTitle>
      </CardHeader>
      <CardContent className="space-y-2">
        <FormField
          id="email"
          label="Email"
          type="email"
          autoComplete="email"
          value={email}
          error={error}
          onChange={(e) => onChange(e.target.value)}
        />
        <p className="text-xs text-muted-foreground">
          We'll send the payment result to this address.
        </p>
      </CardContent>
    </Card>
  );
}
