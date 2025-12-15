import { Link, useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import { useCartStore } from "../stores/cartStore";
import { useAuthStore } from "../stores/authStore";
import { Button, Card, Input } from "../components/ui";
import { EmptyState } from "../components/EmptyState";

export const CartPage = () => {
  const { items, updateQuantity, removeItem, getTotalPrice } = useCartStore();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const navigate = useNavigate();

  const handleCheckout = () => {
    if (!isAuthenticated) {
      navigate("/login");
    } else {
      navigate("/checkout");
    }
  };

  const handleRemoveItem = (productId: string, name: string) => {
    removeItem(productId);
    toast.success(`${name} removed from cart`);
  };

  if (items.length === 0) {
    return (
      <EmptyState
        title="Your cart is empty"
        description="Start shopping to add items to your cart"
        icon={
          <svg
            className="h-16 w-16"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
            />
          </svg>
        }
        action={{
          label: "Continue Shopping",
          onClick: () => navigate("/"),
        }}
      />
    );
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold mb-8 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
        Shopping Cart
      </h1>

      <div className="space-y-4 mb-8">
        {items.map((item) => (
          <Card key={item.productId} hover>
            <div className="flex flex-col sm:flex-row justify-between items-start gap-4">
              <div className="flex-1">
                <h3 className="text-lg font-semibold mb-2 text-gray-900">
                  {item.name}
                </h3>
                <p className="text-gray-600 font-medium">
                  ${item.price.toFixed(2)} each
                </p>
              </div>

              <div className="flex items-center gap-4 w-full sm:w-auto">
                <div className="flex-1 sm:flex-none">
                  <Input
                    type="number"
                    min="1"
                    value={item.quantity}
                    onChange={(e) =>
                      updateQuantity(
                        item.productId,
                        parseInt(e.target.value) || 1
                      )
                    }
                    className="w-20"
                  />
                </div>
                <p className="font-bold w-24 text-right text-gray-900">
                  ${(item.price * item.quantity).toFixed(2)}
                </p>
                <button
                  onClick={() => handleRemoveItem(item.productId, item.name)}
                  className="p-2 text-red-600 hover:text-red-800 hover:bg-red-50 rounded-lg transition-colors"
                  aria-label="Remove item"
                >
                  <svg
                    className="h-5 w-5"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                    />
                  </svg>
                </button>
              </div>
            </div>
          </Card>
        ))}
      </div>

      <Card padding="lg">
        <div className="flex justify-between items-center mb-6">
          <span className="text-xl font-bold text-gray-900">Total:</span>
          <span className="text-3xl font-bold text-blue-600">
            ${getTotalPrice().toFixed(2)}
          </span>
        </div>

        <Button onClick={handleCheckout} className="w-full" size="lg">
          {isAuthenticated ? "Proceed to Checkout" : "Login to Checkout"}
        </Button>
      </Card>
    </div>
  );
};
