import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
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

interface SearchResult {
  items: Product[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const HomePage = () => {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const addItem = useCartStore((state) => state.addItem);

  const { data, isLoading } = useQuery<SearchResult>({
    queryKey: ["products", debouncedSearch, page],
    queryFn: async () => {
      const response = await apiClient.get("/products", {
        params: { search: debouncedSearch || undefined, page, pageSize: 25 },
      });
      return response.data;
    },
  });

  const handleSearch = (value: string) => {
    setSearch(value);
    setTimeout(() => setDebouncedSearch(value), 300);
    setPage(1);
  };

  const handleAddToCart = (product: Product) => {
    addItem({
      productId: product.id,
      name: product.name,
      price: product.price,
    });
  };

  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">Products</h1>
        <input
          type="text"
          value={search}
          onChange={(e) => handleSearch(e.target.value)}
          placeholder="search products..."
          className="px-4 py-2 border border-gray-300 rounded-md w-64"
        />
      </div>

      {isLoading ? (
        <div className="text-center py-12">loading...</div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {data?.items.map((product) => (
              <div
                key={product.id}
                className="bg-white rounded-lg shadow-md p-6"
              >
                <h3 className="text-lg font-semibold mb-2">{product.name}</h3>
                <p className="text-2xl font-bold text-blue-600 mb-4">
                  ${product.price.toFixed(2)}
                </p>
                <div className="text-sm text-gray-600 space-y-1 mb-4">
                  <p>SKU: {product.sku}</p>
                  <p>From: {product.manufacturedFrom}</p>
                </div>
                <div className="flex gap-2">
                  <Link
                    to={`/products/${product.id}`}
                    className="flex-1 text-center bg-gray-100 hover:bg-gray-200 text-gray-800 py-2 px-4 rounded-md"
                  >
                    details
                  </Link>
                  <button
                    onClick={() => handleAddToCart(product)}
                    className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2 px-4 rounded-md"
                  >
                    add to cart
                  </button>
                </div>
              </div>
            ))}
          </div>

          {totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-8">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 border rounded-md disabled:opacity-50 disabled:cursor-not-allowed"
              >
                previous
              </button>
              <span className="px-4 py-2">
                page {page} of {totalPages}
              </span>
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="px-4 py-2 border rounded-md disabled:opacity-50 disabled:cursor-not-allowed"
              >
                next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};
