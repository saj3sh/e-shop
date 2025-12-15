import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import toast from "react-hot-toast";
import { apiClient } from "../lib/apiClient";
import { useCartStore } from "../stores/cartStore";
import { Button, Card, Input, Badge } from "../components/ui";
import { LoadingState } from "../components/LoadingState";

interface Product {
  id: string;
  name: string;
  price: number;
  sku: string;
  manufacturedFrom: string;
  shippedFrom: string;
}

export const ProductPage = () => {
  const { id } = useParams<{ id: string }>();
  const [quantity, setQuantity] = useState(1);
  const addItem = useCartStore((state) => state.addItem);

  const { data: product, isLoading } = useQuery<Product>({
    queryKey: ["product", id],
    queryFn: async () => {
      const response = await apiClient.get(`/products/${id}`);
      return response.data;
    },
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
    }
  };

  if (isLoading) {
    return <LoadingState message="Loading product..." />;
  }

  if (!product) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 mb-2">
          Product not found
        </h2>
        <p className="text-gray-600">
          The product you're looking for doesn't exist.
        </p>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto">
      <Card padding="lg">
        <div className="mb-6">
          <h1 className="text-4xl font-bold mb-4 text-gray-900">
            {product.name}
          </h1>
          <p className="text-4xl font-bold text-blue-600 mb-6">
            ${product.price.toFixed(2)}
          </p>
        </div>

        <div className="space-y-4 mb-8 pb-8 border-b">
          <div className="flex items-center gap-3">
            <span className="font-semibold text-gray-700 w-32">SKU:</span>
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
            <label htmlFor="quantity" className="font-semibold text-gray-700">
              Quantity:
            </label>
            <Input
              id="quantity"
              type="number"
              min="1"
              max="99"
              value={quantity}
              onChange={(e) =>
                setQuantity(Math.max(1, parseInt(e.target.value) || 1))
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
      </Card>
    </div>
  );
};
