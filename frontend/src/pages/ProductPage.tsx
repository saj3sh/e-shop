import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { apiClient } from "../lib/apiClient";
import { useCartStore } from "../stores/cartStore";

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
    }
  };

  if (isLoading) {
    return <div className="text-center py-12">loading...</div>;
  }

  if (!product) {
    return <div className="text-center py-12">product not found</div>;
  }

  return (
    <div className="max-w-4xl mx-auto">
      <div className="bg-white rounded-lg shadow-md p-8">
        <h1 className="text-3xl font-bold mb-4">{product.name}</h1>
        <p className="text-4xl font-bold text-blue-600 mb-6">
          ${product.price.toFixed(2)}
        </p>

        <div className="space-y-3 mb-8">
          <div className="flex">
            <span className="font-semibold w-32">SKU:</span>
            <span>{product.sku}</span>
          </div>
          <div className="flex">
            <span className="font-semibold w-32">Manufactured:</span>
            <span>{product.manufacturedFrom}</span>
          </div>
          <div className="flex">
            <span className="font-semibold w-32">Shipped from:</span>
            <span>{product.shippedFrom}</span>
          </div>
        </div>

        <div className="flex gap-4 items-center">
          <div className="flex items-center gap-2">
            <label htmlFor="quantity" className="font-semibold">
              Quantity:
            </label>
            <input
              id="quantity"
              type="number"
              min="1"
              max="99"
              value={quantity}
              onChange={(e) =>
                setQuantity(Math.max(1, parseInt(e.target.value) || 1))
              }
              className="w-20 px-3 py-2 border border-gray-300 rounded-md"
            />
          </div>
          <button
            onClick={handleAddToCart}
            className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-md"
          >
            add to cart
          </button>
        </div>
      </div>
    </div>
  );
};
