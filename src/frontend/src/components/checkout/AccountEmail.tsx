import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

// O e-mail da conta, para onde o Orders envia o resultado do pagamento. Não é
// editável: vem do token do Keycloak.
export default function AccountEmail({ email }: { email: string }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">Contato</CardTitle>
      </CardHeader>
      <CardContent className="space-y-1">
        <p className="text-sm font-medium text-foreground" data-testid="account-email">
          {email}
        </p>
        <p className="text-xs text-muted-foreground">
          Vamos enviar o resultado do pagamento para este e-mail.
        </p>
      </CardContent>
    </Card>
  );
}
