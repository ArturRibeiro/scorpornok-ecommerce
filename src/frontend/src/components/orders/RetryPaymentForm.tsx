import PaymentForm from "@/components/checkout/PaymentForm";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/context/AuthContext";
import {
  EMPTY_PAYMENT,
  validatePayment,
  type CheckoutErrors,
  type PaymentDetails,
} from "@/lib/checkout";
import { retryOrderPayment, SessionExpiredError } from "@/lib/orders";
import { tokenizeCard } from "@/lib/payments";
import { Loader2, RotateCcw } from "lucide-react";
import { useState } from "react";

const RETRY_ERROR =
  "Não foi possível enviar a nova tentativa agora. Tente de novo em instantes.";

// Nova tentativa do mesmo pedido com outro cartão: o cartão é tokenizado no Payments e só o token
// vai ao Orders, como no checkout. O endereço e os itens não mudam.
// Usado na confirmação da compra e no detalhe do pedido ("Meus pedidos"); secondary é o botão ao
// lado de "Tentar com outro cartão" (a confirmação oferece voltar ao carrinho).
export default function RetryPaymentForm({
  orderNumber,
  onRetried,
  secondary,
}: {
  orderNumber: string;
  onRetried: (attempt: number) => void;
  secondary?: React.ReactNode;
}) {
  const { expireSession, getAccessToken } = useAuth();
  const [payment, setPayment] = useState<PaymentDetails>(EMPTY_PAYMENT);
  const [errors, setErrors] = useState<CheckoutErrors>({});
  const [submitErrors, setSubmitErrors] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  const updatePayment = (field: keyof PaymentDetails, value: string) => {
    setPayment((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => {
      const next = { ...prev };
      delete next[field];
      return next;
    });
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const found = validatePayment(payment);
    setErrors(found);
    if (Object.keys(found).length > 0) return;

    setSubmitting(true);
    setSubmitErrors([]);
    try {
      const token = await getAccessToken();
      if (!token) throw new SessionExpiredError();

      const tokenized = await tokenizeCard(payment);
      if (!tokenized.ok) {
        setErrors(tokenized.errors);
        return;
      }

      const result = await retryOrderPayment(
        orderNumber,
        { cardToken: tokenized.token.token, installments: Number(payment.installments) },
        token
      );
      if (result.ok) onRetried(result.attempt);
      else setSubmitErrors(result.errors);
    } catch (error) {
      if (error instanceof SessionExpiredError) {
        await expireSession();
        return;
      }
      setSubmitErrors([RETRY_ERROR]);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form noValidate onSubmit={handleSubmit} className="space-y-4 text-left">
      <PaymentForm payment={payment} errors={errors} onChange={updatePayment} />
      {submitErrors.length > 0 && (
        <ul role="alert" className="list-disc pl-5 text-sm text-destructive space-y-1">
          {submitErrors.map((error) => (
            <li key={error}>{error}</li>
          ))}
        </ul>
      )}
      <div className="flex flex-col sm:flex-row gap-3 justify-center">
        <Button type="submit" disabled={submitting}>
          {submitting ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <RotateCcw className="h-4 w-4" />
          )}
          Tentar com outro cartão
        </Button>
        {secondary}
      </div>
    </form>
  );
}
