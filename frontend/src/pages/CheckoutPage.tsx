import { useState } from "react";
import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { useCartStore } from "../stores/cartStore";
import { apiClient } from "../lib/apiClient";
import { CardIcon } from "../components/CardIcon";
import { AddressManagement } from "../components/AddressManagement";
import { Button, Card, Input, Alert } from "../components/ui";

export const CheckoutPage = () => {
  const { items, clearCart, getTotalPrice } = useCartStore();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [cardNumber, setCardNumber] = useState("");
  const [cardType, setCardType] = useState("Visa");

  const [selectedShippingId, setSelectedShippingId] = useState<string | null>(
    null
  );
  const [selectedBillingId, setSelectedBillingId] = useState<string | null>(
    null
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!selectedShippingId || !selectedBillingId) {
      toast.error("Please select both shipping and billing addresses");
      return;
    }

    if (!cardNumber || cardNumber.length < 13) {
      toast.error("Please enter a valid card number");
      return;
    }

    setLoading(true);

    try {
      const response = await apiClient.post("/orders", {
        items: items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
        shippingAddressId: selectedShippingId,
        billingAddressId: selectedBillingId,
        cardNumber,
        cardType,
      });

      clearCart();
      toast.success("Order placed successfully!");
      navigate(`/orders/${response.data.id}`);
    } catch (err: any) {
      setError(err.response?.data?.error || "checkout failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-3xl font-bold mb-8 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
        Checkout
      </h1>

      <Card className="mb-6" padding="lg">
        <h2 className="text-xl font-bold mb-4 text-gray-900">Order Summary</h2>
        <div className="space-y-2 mb-4">
          {items.map((item) => (
            <div
              key={item.productId}
              className="flex justify-between text-gray-700"
            >
              <span>
                {item.name} × {item.quantity}
              </span>
              <span className="font-medium">
                ${(item.price * item.quantity).toFixed(2)}
              </span>
            </div>
          ))}
        </div>
        <div className="border-t pt-4">
          <div className="flex justify-between text-xl font-bold">
            <span className="text-gray-900">Total:</span>
            <span className="text-blue-600">${getTotalPrice().toFixed(2)}</span>
          </div>
        </div>
      </Card>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card padding="lg">
          <AddressManagement
            showSelector={true}
            selectedShippingId={selectedShippingId}
            selectedBillingId={selectedBillingId}
            onShippingSelect={setSelectedShippingId}
            onBillingSelect={setSelectedBillingId}
          />
        </Card>

        <Card padding="lg">
          <h2 className="text-xl font-bold mb-4 text-gray-900">
            Payment Information
          </h2>

          <div className="space-y-4">
            <Input
              type="text"
              label="Card Number"
              value={cardNumber}
              onChange={(e) => setCardNumber(e.target.value.replace(/\s/g, ""))}
              placeholder="1234567812345678"
              maxLength={16}
              required
            />

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Card Type
              </label>
              <div className="grid grid-cols-4 gap-3">
                {["Visa", "Mastercard", "Amex", "Discover"].map((type) => (
                  <button
                    key={type}
                    type="button"
                    onClick={() => setCardType(type)}
                    className={`flex flex-col items-center justify-center p-3 border-2 rounded-lg transition-all ${
                      cardType === type
                        ? "border-blue-600 bg-blue-50 shadow-md"
                        : "border-gray-200 hover:border-gray-300"
                    }`}
                  >
                    <CardIcon type={type} />
                    <span className="text-xs mt-1 font-medium">{type}</span>
                  </button>
                ))}
              </div>
            </div>
          </div>
        </Card>

        {error && <Alert variant="error">{error}</Alert>}

        <Button type="submit" isLoading={loading} className="w-full" size="lg">
          {loading ? "Placing order..." : "Place Order"}
        </Button>
      </form>
    </div>
  );
};
