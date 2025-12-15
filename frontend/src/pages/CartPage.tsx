import { Link, useNavigate } from "react-router-dom";
import { useCartStore } from "../stores/cartStore";
import { useAuthStore } from "../stores/authStore";

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

  if (items.length === 0) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold mb-4">your cart is empty</h2>
        <Link to="/" className="text-blue-600 hover:underline">
          continue shopping
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">shopping cart</h1>

      <div className="space-y-4 mb-8">
        {items.map((item) => (
          <div
            key={item.productId}
            className="bg-white rounded-lg shadow-md p-6"
          >
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <h3 className="text-lg font-semibold mb-2">{item.name}</h3>
                <p className="text-gray-600">${item.price.toFixed(2)} each</p>
              </div>

              <div className="flex items-center gap-4">
                <input
                  type="number"
                  min="1"
                  value={item.quantity}
                  onChange={(e) =>
                    updateQuantity(
                      item.productId,
                      parseInt(e.target.value) || 1
                    )
                  }
                  className="w-20 px-3 py-1 border border-gray-300 rounded-md"
                />
                <p className="font-bold w-24 text-right">
                  ${(item.price * item.quantity).toFixed(2)}
                </p>
                <button
                  onClick={() => removeItem(item.productId)}
                  className="text-red-600 hover:text-red-800"
                >
                  remove
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex justify-between items-center mb-6">
          <span className="text-xl font-bold">Total:</span>
          <span className="text-2xl font-bold text-blue-600">
            ${getTotalPrice().toFixed(2)}
          </span>
        </div>

        <button
          onClick={handleCheckout}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 px-6 rounded-md text-lg font-semibold"
        >
          proceed to checkout
        </button>
      </div>
    </div>
  );
};
