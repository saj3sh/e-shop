import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useCartStore } from "../stores/cartStore";
import { apiClient } from "../lib/apiClient";
import { CardIcon } from "../components/CardIcon";
import { AddressSelector } from "../components/AddressSelector";
import { AddressForm } from "../components/AddressForm";

interface Address {
  id: string;
  line1: string;
  city: string;
  country: string;
  type: string;
}

interface AddressData {
  addresses: Address[];
  defaultShippingAddressId: string | null;
  defaultBillingAddressId: string | null;
}

export const CheckoutPage = () => {
  const { items, clearCart, getTotalPrice } = useCartStore();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [cardNumber, setCardNumber] = useState("");
  const [cardType, setCardType] = useState("Visa");

  const [addressData, setAddressData] = useState<AddressData | null>(null);
  const [loadingAddresses, setLoadingAddresses] = useState(true);
  const [selectedShippingId, setSelectedShippingId] = useState<string | null>(
    null
  );
  const [selectedBillingId, setSelectedBillingId] = useState<string | null>(
    null
  );
  const [showShippingForm, setShowShippingForm] = useState(false);
  const [showBillingForm, setShowBillingForm] = useState(false);

  const fetchAddresses = useCallback(async (setDefaults = false) => {
    try {
      const response = await apiClient.get("/addresses");
      setAddressData(response.data);

      // Only set defaults on initial load
      if (setDefaults) {
        setSelectedShippingId(response.data.defaultShippingAddressId);
        setSelectedBillingId(response.data.defaultBillingAddressId);
      }
    } catch (err) {
      console.error("Failed to fetch addresses", err);
    } finally {
      setLoadingAddresses(false);
    }
  }, []);

  useEffect(() => {
    fetchAddresses(true);
  }, []);

  const handleCreateAddress = async (address: {
    line1: string;
    city: string;
    country: string;
    type: "Shipping" | "Billing" | "Both";
  }) => {
    const response = await apiClient.post("/addresses", address);

    // Auto-select the newly created address before fetching
    if (address.type === "Shipping" || address.type === "Both") {
      setSelectedShippingId(response.data.id);
      setShowShippingForm(false);
    }
    if (address.type === "Billing" || address.type === "Both") {
      setSelectedBillingId(response.data.id);
      setShowBillingForm(false);
    }

    // Refresh addresses list without resetting selections
    await fetchAddresses(false);
  };

  const handleSetDefaultAddress = async (
    addressId: string,
    addressType: "Shipping" | "Billing"
  ) => {
    try {
      await apiClient.put(`/addresses/${addressId}/set-default`, {
        addressType,
      });
      await fetchAddresses(false);
    } catch (err) {
      console.error("Failed to set default address", err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!selectedShippingId || !selectedBillingId) {
      setError("Please select both shipping and billing addresses");
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
      navigate(`/orders/${response.data.id}`);
    } catch (err: any) {
      setError(err.response?.data?.error || "checkout failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">checkout</h1>

      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <h2 className="text-xl font-bold mb-4">order summary</h2>
        <div className="space-y-2 mb-4">
          {items.map((item) => (
            <div key={item.productId} className="flex justify-between">
              <span>
                {item.name} × {item.quantity}
              </span>
              <span>${(item.price * item.quantity).toFixed(2)}</span>
            </div>
          ))}
        </div>
        <div className="border-t pt-4">
          <div className="flex justify-between text-xl font-bold">
            <span>Total:</span>
            <span>${getTotalPrice().toFixed(2)}</span>
          </div>
        </div>
      </div>

      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-lg shadow-md p-6 space-y-6"
      >
        {loadingAddresses ? (
          <div className="text-center py-8">Loading addresses...</div>
        ) : (
          <>
            {/* Shipping Address Section */}
            <div>
              <h2 className="text-xl font-bold mb-4">shipping address</h2>
              {showShippingForm ? (
                <AddressForm
                  onSubmit={handleCreateAddress}
                  onCancel={() => setShowShippingForm(false)}
                  addressType="Shipping"
                />
              ) : (
                <AddressSelector
                  addresses={addressData?.addresses || []}
                  selectedAddressId={selectedShippingId}
                  onAddressSelect={setSelectedShippingId}
                  onCreateNew={() => setShowShippingForm(true)}
                  onSetDefault={(id) => handleSetDefaultAddress(id, "Shipping")}
                  label="Select shipping address"
                  addressType="shipping"
                  defaultAddressId={
                    addressData?.defaultShippingAddressId || null
                  }
                />
              )}
            </div>

            {/* Billing Address Section */}
            <div>
              <h2 className="text-xl font-bold mb-4">billing address</h2>
              {showBillingForm ? (
                <AddressForm
                  onSubmit={handleCreateAddress}
                  onCancel={() => setShowBillingForm(false)}
                  addressType="Billing"
                />
              ) : (
                <AddressSelector
                  addresses={addressData?.addresses || []}
                  selectedAddressId={selectedBillingId}
                  onAddressSelect={setSelectedBillingId}
                  onCreateNew={() => setShowBillingForm(true)}
                  onSetDefault={(id) => handleSetDefaultAddress(id, "Billing")}
                  label="Select billing address"
                  addressType="billing"
                  defaultAddressId={
                    addressData?.defaultBillingAddressId || null
                  }
                />
              )}
            </div>

            {/* Payment Information */}
            <div>
              <h2 className="text-xl font-bold mb-4">payment information</h2>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">
                    card number
                  </label>
                  <input
                    type="text"
                    value={cardNumber}
                    onChange={(e) =>
                      setCardNumber(e.target.value.replace(/\s/g, ""))
                    }
                    placeholder="1234567812345678"
                    maxLength={16}
                    className="w-full px-4 py-2 border rounded-md"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    card type
                  </label>
                  <div className="grid grid-cols-4 gap-3">
                    {["Visa", "Mastercard", "Amex", "Discover"].map((type) => (
                      <button
                        key={type}
                        type="button"
                        onClick={() => setCardType(type)}
                        className={`flex flex-col items-center justify-center p-3 border-2 rounded-lg transition-all ${
                          cardType === type
                            ? "border-blue-600 bg-blue-50"
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
            </div>
          </>
        )}

        {error && (
          <div className="bg-red-50 text-red-600 p-3 rounded-md">{error}</div>
        )}

        <button
          type="submit"
          disabled={loading || loadingAddresses}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 px-6 rounded-md font-semibold disabled:bg-gray-400"
        >
          {loading ? "placing order..." : "place order"}
        </button>
      </form>
    </div>
  );
};
