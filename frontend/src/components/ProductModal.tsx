import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { apiClient } from "../lib/apiClient";
import { useCartStore } from "../stores/cartStore";
import { Button, Input, Badge } from "./ui";
import { LoadingState } from "./LoadingState";

interface Product {
  id: string;
  name: string;
  price: number;
  sku: string;
  manufacturedFrom: string;
  shippedFrom: string;
}

interface ProductModalProps {
  productId: string;
  isOpen: boolean;
  onClose: () => void;
}

export const ProductModal = ({
  productId,
  isOpen,
  onClose,
}: ProductModalProps) => {
  const [quantity, setQuantity] = useState(1);
  const addItem = useCartStore((state) => state.addItem);

  const { data: product, isLoading } = useQuery<Product>({
    queryKey: ["product", productId],
    queryFn: async () => {
      const response = await apiClient.get(`/products/${productId}`);
      return response.data;
    },
    enabled: isOpen,
  });

  const handleAddToCart = () => {
    if (product) {
      addItem({
        productId: product.id,
        name: product.name,
        price: product.price,
        quantity,
      });
      toast.success(`${quantity} × ${product.name} added to cart!`);
      onClose();
    }
  };

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="fixed inset-0 z-50 overflow-y-auto" onClick={onClose}>
        <div className="flex min-h-full items-center justify-center p-4">
          <div
            className="relative bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Close button */}
            <button
              onClick={onClose}
              className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 transition-colors cursor-pointer"
              aria-label="Close modal"
            >
              <svg
                className="h-6 w-6"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>

            {/* Content */}
            <div className="p-6">
              {isLoading ? (
                <LoadingState message="Loading product..." />
              ) : product ? (
                <>
                  <div className="mb-6">
                    <h2 className="text-3xl font-bold mb-4 text-gray-900 pr-8">
                      {product.name}
                    </h2>
                    <p className="text-4xl font-bold text-blue-600 mb-6">
                      ${product.price.toFixed(2)}
                    </p>
                  </div>

                  <div className="space-y-4 mb-8 pb-8 border-b">
                    <div className="flex items-center gap-3">
                      <span className="font-semibold text-gray-700 w-32">
                        SKU:
                      </span>
                      <Badge variant="gray">{product.sku}</Badge>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="font-semibold text-gray-700 w-32">
                        Manufactured:
                      </span>
                      <div className="flex items-center gap-2 text-gray-900">
                        <svg
                          className="h-5 w-5 text-gray-400"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                          />
                        </svg>
                        {product.manufacturedFrom}
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="font-semibold text-gray-700 w-32">
                        Shipped from:
                      </span>
                      <div className="flex items-center gap-2 text-gray-900">
                        <svg
                          className="h-5 w-5 text-gray-400"
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                          />
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                          />
                        </svg>
                        {product.shippedFrom}
                      </div>
                    </div>
                  </div>

                  <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center">
                    <div className="flex items-center gap-3">
                      <label
                        htmlFor="quantity"
                        className="font-semibold text-gray-700"
                      >
                        Quantity:
                      </label>
                      <Input
                        id="quantity"
                        type="number"
                        min="1"
                        max="99"
                        value={quantity}
                        onChange={(e) =>
                          setQuantity(
                            Math.max(1, parseInt(e.target.value) || 1)
                          )
                        }
                        className="w-20"
                      />
                    </div>
                    <Button
                      onClick={handleAddToCart}
                      size="lg"
                      className="flex-1 sm:flex-none"
                    >
                      Add to Cart
                    </Button>
                  </div>
                </>
              ) : (
                <div className="text-center py-8">
                  <p className="text-gray-600">Product not found</p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
