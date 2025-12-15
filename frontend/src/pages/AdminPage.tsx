import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";

interface Order {
  id: string;
  customerId: string;
  status: string;
  trackingNumber: string;
  purchaseDate: string;
  total: number;
}

export const AdminPage = () => {
  const queryClient = useQueryClient();

  const { data: orders, isLoading } = useQuery<Order[]>({
    queryKey: ["admin-orders"],
    queryFn: async () => {
      const response = await apiClient.get("/admin/orders");
      return response.data;
    },
  });

  const completeMutation = useMutation({
    mutationFn: async (orderId: string) => {
      await apiClient.post(`/admin/orders/${orderId}/complete`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-orders"] });
    },
  });

  return (
    <div className="max-w-6xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">admin dashboard</h1>

      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-xl font-bold mb-4">incomplete orders</h2>

        {isLoading ? (
          <p>loading orders...</p>
        ) : orders && orders.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-2">tracking #</th>
                  <th className="text-left py-2">status</th>
                  <th className="text-left py-2">date</th>
                  <th className="text-right py-2">total</th>
                  <th className="text-right py-2">actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((order) => (
                  <tr key={order.id} className="border-b">
                    <td className="py-2">{order.trackingNumber}</td>
                    <td className="py-2">{order.status}</td>
                    <td className="py-2">
                      {new Date(order.purchaseDate).toLocaleDateString()}
                    </td>
                    <td className="text-right py-2">
                      ${order.total.toFixed(2)}
                    </td>
                    <td className="text-right py-2">
                      <button
                        onClick={() => completeMutation.mutate(order.id)}
                        disabled={completeMutation.isPending}
                        className="bg-green-600 hover:bg-green-700 text-white px-3 py-1 rounded text-sm disabled:bg-gray-400"
                      >
                        mark complete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-gray-600">no incomplete orders</p>
        )}
      </div>
    </div>
  );
};
