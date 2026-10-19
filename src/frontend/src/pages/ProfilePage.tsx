import AccountEmail from "@/components/checkout/AccountEmail";
import BuyerForm from "@/components/checkout/BuyerForm";
import ShippingAddressForm from "@/components/checkout/ShippingAddressForm";
import ErrorState from "@/components/ErrorState";
import LoadingState from "@/components/LoadingState";
import RequireAuth from "@/components/RequireAuth";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/context/AuthContext";
import { useRequest } from "@/hooks/useRequest";
import {
  addressFromProfile,
  buyerFromProfile,
  validateAddress,
  validateBuyer,
  type BuyerDetails,
  type CheckoutErrors,
  type CheckoutField,
  type ShippingAddress,
} from "@/lib/checkout";
import { getProfile, saveProfile, type CustomerProfile } from "@/lib/customers";
import { SessionExpiredError } from "@/lib/orders";
import { AlertCircle, CheckCircle2, Loader2 } from "lucide-react";
import { useState } from "react";

const SAVE_ERROR = "Não foi possível salvar o cadastro agora. Tente de novo em instantes.";

// Erros da API vêm nas chaves do JSON ("cpf", "address.zipCode"); o formulário usa o nome do campo.
const toFieldErrors = (errors: Record<string, string[]>): CheckoutErrors =>
  Object.fromEntries(
    Object.entries(errors).map(([key, messages]) => [
      key === "address" ? "street" : key.replace(/^address\./, ""),
      messages[0],
    ])
  );

// "Meu cadastro": o cliente vê e edita nome, telefone, CPF e endereço. O e-mail é o da conta.
export default function ProfilePage() {
  return (
    <RequireAuth message="Entre para ver e editar o seu cadastro.">
      <ProfileLoader />
    </RequireAuth>
  );
}

function ProfileLoader() {
  const { user, getAccessToken, expireSession } = useAuth();
  const profile = useRequest(user?.profile.sub ?? "", async (signal) => {
    const token = await getAccessToken();
    if (!token) throw new SessionExpiredError();
    return getProfile(token, signal);
  });

  if (profile.error instanceof SessionExpiredError) {
    void expireSession();
    return <LoadingState />;
  }
  if (profile.error) {
    return (
      <ErrorState
        onRetry={profile.retry}
        message="Não conseguimos carregar o seu cadastro agora. Tente de novo em instantes."
      />
    );
  }
  if (profile.loading || !profile.data) return <LoadingState />;
  return <ProfileForm initial={profile.data} />;
}

function ProfileForm({ initial }: { initial: CustomerProfile }) {
  const { getAccessToken, expireSession } = useAuth();
  const [buyer, setBuyer] = useState<BuyerDetails>(() => buyerFromProfile(initial));
  const [address, setAddress] = useState<ShippingAddress>(() => addressFromProfile(initial.address));
  const [errors, setErrors] = useState<CheckoutErrors>({});
  const [saving, setSaving] = useState(false);
  const [status, setStatus] = useState<"saved" | "failed" | null>(null);

  const clearError = (field: CheckoutField) => {
    setStatus(null);
    setErrors((prev) => {
      const next = { ...prev };
      delete next[field];
      return next;
    });
  };

  const updateBuyer = (field: keyof BuyerDetails, value: string) => {
    setBuyer((prev) => ({ ...prev, [field]: value }));
    clearError(field);
  };

  const updateAddress = (field: keyof ShippingAddress, value: string) => {
    setAddress((prev) => ({ ...prev, [field]: value }));
    clearError(field);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setStatus(null);

    const found = { ...validateBuyer(buyer), ...validateAddress(address) };
    setErrors(found);
    if (Object.keys(found).length > 0) return;

    setSaving(true);
    try {
      const token = await getAccessToken();
      if (!token) throw new SessionExpiredError();
      const result = await saveProfile(
        { ...buyer, address: { ...address, complement: address.complement.trim() || null } },
        token
      );
      if (result.ok) {
        setStatus("saved");
      } else {
        setErrors(toFieldErrors(result.errors));
      }
    } catch (error) {
      if (error instanceof SessionExpiredError) {
        await expireSession();
        return;
      }
      // Os campos continuam com o que o cliente digitou.
      setStatus("failed");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8 max-w-3xl">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-foreground">Meu cadastro</h1>
        <p className="text-muted-foreground mt-2">
          Seus dados e o endereço de entrega, usados para agilizar o checkout
        </p>
      </div>

      <form noValidate onSubmit={handleSubmit} className="space-y-6">
        <AccountEmail email={initial.email} description="E-mail da sua conta; não pode ser alterado aqui." />
        <BuyerForm title="Dados pessoais" buyer={buyer} errors={errors} onChange={updateBuyer} />
        <ShippingAddressForm address={address} errors={errors} onChange={updateAddress} />

        {status === "saved" && (
          <div role="status" className="flex items-center gap-2 rounded-lg border border-green-300 bg-green-50 p-3 text-sm text-green-800">
            <CheckCircle2 className="h-4 w-4" />
            Cadastro salvo.
          </div>
        )}
        {status === "failed" && (
          <div role="alert" className="flex items-center gap-2 rounded-lg border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
            <AlertCircle className="h-4 w-4" />
            {SAVE_ERROR}
          </div>
        )}

        <div className="flex justify-end">
          <Button type="submit" disabled={saving}>
            {saving && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
            {saving ? "Salvando..." : "Salvar cadastro"}
          </Button>
        </div>
      </form>
    </div>
  );
}
