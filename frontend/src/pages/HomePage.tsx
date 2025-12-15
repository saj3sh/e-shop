import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Navigate } from "react-router-dom";
import toast from "react-hot-toast";
import { apiClient } from "../lib/apiClient";
import { useCartStore } from "../stores/cartStore";
import { useAuthStore } from "../stores/authStore";
import { Button, Card, Input, Badge } from "../components/ui";
import { LoadingState } from "../components/LoadingState";
import { EmptyState } from "../components/EmptyState";
import { ProductModal } from "../components/ProductModal";

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
  const { role } = useAuthStore();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [selectedProductId, setSelectedProductId] = useState<string | null>(
    null
  );
  const addItem = useCartStore((state) => state.addItem);

  // Redirect admin users to admin dashboard
  if (role === "Admin") {
    return <Navigate to="/admin" replace />;
  }

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
    toast.success(`${product.name} added to cart!`);
  };

  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
          Products
        </h1>
        <Input
          type="text"
          value={search}
          onChange={(e) => handleSearch(e.target.value)}
          placeholder="Search products..."
          className="w-full sm:w-64"
        />
      </div>

      {isLoading ? (
        <LoadingState message="Loading products..." />
      ) : data?.items.length === 0 ? (
        <EmptyState
          title="No products found"
          description={
            search
              ? `No products match "${search}". Try a different search.`
              : "No products available at the moment."
          }
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
                d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"
              />
            </svg>
          }
        />
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {data?.items.map((product) => (
              <Card key={product.id} hover className="flex flex-col">
                <div className="flex-1">
                  <h3 className="text-lg font-semibold mb-2 text-gray-900">
                    {product.name}
                  </h3>
                  <p className="text-2xl font-bold text-blue-600 mb-4">
                    ${product.price.toFixed(2)}
                  </p>
                  <div className="text-sm text-gray-600 space-y-2 mb-4">
                    <div className="flex items-center gap-2">
                      <Badge variant="gray" size="sm">
                        {product.sku}
                      </Badge>
                    </div>
                    <p className="flex items-center gap-1">
                      <svg
                        className="h-4 w-4 text-gray-400"
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
                      {product.manufacturedFrom}
                    </p>
                  </div>
                </div>
                <div className="flex gap-2 mt-auto">
                  <Button
                    onClick={() => setSelectedProductId(product.id)}
                    variant="outline"
                    className="flex-1"
                  >
                    Details
                  </Button>
                  <Button
                    onClick={() => handleAddToCart(product)}
                    className="flex-1"
                  >
                    Add to Cart
                  </Button>
                </div>
              </Card>
            ))}
          </div>

          <ProductModal
            productId={selectedProductId || ""}
            isOpen={!!selectedProductId}
            onClose={() => setSelectedProductId(null)}
          />

          {totalPages > 1 && (
            <div className="flex justify-center items-center gap-2 mt-8">
              <Button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                variant="outline"
                size="sm"
              >
                Previous
              </Button>
              <span className="px-4 py-2 text-gray-700 font-medium">
                Page {page} of {totalPages}
              </span>
              <Button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                variant="outline"
                size="sm"
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
};
