import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCartStore } from "../stores/cartStore";
import { apiClient } from "../lib/apiClient";
import { CardIcon } from "../components/CardIcon";

export const CheckoutPage = () => {
  const { items, clearCart, getTotalPrice } = useCartStore();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [shippingAddress, setShippingAddress] = useState("");
  const [shippingCity, setShippingCity] = useState("");
  const [shippingCountry, setShippingCountry] = useState("");
  const [cardNumber, setCardNumber] = useState("");
  const [cardType, setCardType] = useState("Visa");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      // create temp address
      const addressResponse = await apiClient.post("/addresses", {
        line1: shippingAddress,
        city: shippingCity,
        country: shippingCountry,
        type: "Both",
      });

      const addressId = addressResponse.data.id;

      const response = await apiClient.post("/orders", {
        items: items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
        shippingAddressId: addressId,
        billingAddressId: addressId,
        shippingCountry,
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
        className="bg-white rounded-lg shadow-md p-6 space-y-4"
      >
        <h2 className="text-xl font-bold mb-4">shipping information</h2>

        <div>
          <label className="block text-sm font-medium mb-1">address</label>
          <input
            type="text"
            value={shippingAddress}
            onChange={(e) => setShippingAddress(e.target.value)}
            required
            className="w-full px-4 py-2 border rounded-md"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">city</label>
          <input
            type="text"
            value={shippingCity}
            onChange={(e) => setShippingCity(e.target.value)}
            required
            className="w-full px-4 py-2 border rounded-md"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">
            country (2-letter code)
          </label>
          <input
            type="text"
            value={shippingCountry}
            onChange={(e) => setShippingCountry(e.target.value.toUpperCase())}
            maxLength={2}
            required
            className="w-full px-4 py-2 border rounded-md"
          />
        </div>

        <h2 className="text-xl font-bold mb-4 mt-6">payment information</h2>

        <div>
          <label className="block text-sm font-medium mb-1">card number</label>
          <input
            type="text"
            value={cardNumber}
            onChange={(e) => setCardNumber(e.target.value.replace(/\s/g, ""))}
            placeholder="1234567812345678"
            maxLength={16}
            className="w-full px-4 py-2 border rounded-md"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">card type</label>
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

        {error && (
          <div className="bg-red-50 text-red-600 p-3 rounded-md">{error}</div>
        )}

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 px-6 rounded-md font-semibold disabled:bg-gray-400"
        >
          {loading ? "placing order..." : "place order"}
        </button>
      </form>
    </div>
  );
};
