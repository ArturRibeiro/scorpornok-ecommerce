import CheckoutItems from "@/components/checkout/CheckoutItems";
import ContactForm from "@/components/checkout/ContactForm";
import CheckoutSummary from "@/components/checkout/CheckoutSummary";
import OrderConfirmation from "@/components/checkout/OrderConfirmation";
import PaymentForm from "@/components/checkout/PaymentForm";
import ShippingAddressForm from "@/components/checkout/ShippingAddressForm";
import { Button } from "@/components/ui/button";
import { useCart, type CartItem } from "@/context/CartContext";
import {
  EMPTY_ADDRESS,
  EMPTY_PAYMENT,
  onlyDigits,
  validateCheckout,
  type CheckoutErrors,
  type CheckoutField,
  type PaymentDetails,
  type ShippingAddress,
} from "@/lib/checkout";
import { getCustomerId } from "@/lib/customer";
import {
  createOrder,
  type CreatedOrder,
  type CreateOrderRequest,
} from "@/lib/orders";
import { ArrowLeft } from "lucide-react";
import { useState } from "react";
import { Link, Navigate } from "react-router";

const NETWORK_ERROR =
  "The order service is unavailable. Please try again in a moment.";

const toRequest = (
  cart: CartItem[],
  email: string,
  address: ShippingAddress,
  payment: PaymentDetails,
  total: number
): CreateOrderRequest => ({
  userId: getCustomerId(),
  email: email.trim(),
  address,
  items: cart.map((item) => ({
    productId: item.id,
    productName: item.name,
    pictureUrl: item.image,
    unitPrice: item.price,
    discount: 0,
    units: item.quantity,
  })),
  card: {
    orderId: "00000000-0000-0000-0000-000000000000",
    cardHolderName: payment.cardHolderName.trim(),
    cardNumber: onlyDigits(payment.cardNumber),
    expirationMonth: payment.expirationMonth.padStart(2, "0"),
    expirationYear: payment.expirationYear,
    cvv: payment.cvv,
    amount: total,
    installments: Number(payment.installments),
  },
});

export default function CheckoutPage() {
  const { cart, clearCart } = useCart();
  const [email, setEmail] = useState("");
  const [address, setAddress] = useState(EMPTY_ADDRESS);
  const [payment, setPayment] = useState(EMPTY_PAYMENT);
  const [errors, setErrors] = useState<CheckoutErrors>({});
  const [submitErrors, setSubmitErrors] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [placedOrder, setPlacedOrder] = useState<CreatedOrder | null>(null);

  // Vem antes do redirecionamento: o carrinho já foi esvaziado no sucesso.
  if (placedOrder) {
    return <OrderConfirmation order={placedOrder} email={email.trim()} />;
  }

  if (cart.length === 0) {
    return <Navigate to="/cart" replace />;
  }

  const itemCount = cart.reduce((sum, item) => sum + item.quantity, 0);
  const total = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);

  const clearError = (field: CheckoutField) =>
    setErrors((prev) => {
      const next = { ...prev };
      delete next[field];
      return next;
    });

  const updateEmail = (value: string) => {
    setEmail(value);
    clearError("email");
  };

  const updateAddress = (field: keyof ShippingAddress, value: string) => {
    setAddress((prev) => ({ ...prev, [field]: value }));
    clearError(field);
  };

  const updatePayment = (field: keyof PaymentDetails, value: string) => {
    setPayment((prev) => ({ ...prev, [field]: value }));
    clearError(field);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const found = validateCheckout(email, address, payment);
    setErrors(found);
    if (Object.keys(found).length > 0) return;

    setSubmitting(true);
    setSubmitErrors([]);
    try {
      const result = await createOrder(toRequest(cart, email, address, payment, total));
      if (result.ok) {
        clearCart();
        setPlacedOrder(result.order);
      } else {
        setSubmitErrors(result.errors);
      }
    } catch {
      setSubmitErrors([NETWORK_ERROR]);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Checkout</h1>
          <p className="text-muted-foreground mt-2">
            Review your items and enter your shipping and payment details
          </p>
        </div>

        <Button
          variant="ghost"
          asChild
          className="text-muted-foreground hover:text-foreground"
        >
          <Link to="/cart" className="flex items-center gap-2">
            <ArrowLeft className="h-4 w-4" />
            Back to Cart
          </Link>
        </Button>
      </div>

      <form noValidate onSubmit={handleSubmit} className="grid lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">
          <CheckoutItems items={cart} />
          <ContactForm email={email} error={errors.email} onChange={updateEmail} />
          <ShippingAddressForm
            address={address}
            errors={errors}
            onChange={updateAddress}
          />
          <PaymentForm payment={payment} errors={errors} onChange={updatePayment} />
        </div>

        <div className="lg:col-span-1">
          <CheckoutSummary
            itemCount={itemCount}
            total={total}
            submitting={submitting}
            errors={submitErrors}
          />
        </div>
      </form>
    </div>
  );
}
