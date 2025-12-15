import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { apiClient } from "../lib/apiClient";

interface Order {
  id: string;
  status: string;
  trackingNumber: string;
  purchaseDate: string;
  total: number;
}

interface Customer {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

export const UserPage = () => {
  const { data: customer, isLoading: isLoadingCustomer } = useQuery<Customer>({
    queryKey: ["customer-me"],
    queryFn: async () => {
      const response = await apiClient.get("/customers/me");
      return response.data;
    },
  });

  const { data: orders, isLoading: isLoadingOrders } = useQuery<Order[]>({
    queryKey: ["my-orders"],
    queryFn: async () => {
      const response = await apiClient.get("/orders/mine");
      return response.data;
    },
  });

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">My Profile</h1>

      {/* Customer Information */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
        <h2 className="text-xl font-bold mb-4">Personal Information</h2>

        {isLoadingCustomer ? (
          <p>Loading...</p>
        ) : customer ? (
          <div className="space-y-3">
            <div className="flex justify-between">
              <span className="text-gray-600">Name:</span>
              <span className="font-medium">
                {customer.firstName} {customer.lastName}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Email:</span>
              <span className="font-medium">{customer.email}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Phone:</span>
              <span className="font-medium">{customer.phone}</span>
            </div>
          </div>
        ) : (
          <p className="text-red-600">Failed to load customer information</p>
        )}
      </div>

      {/* Orders Section */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-xl font-bold mb-4">My Orders</h2>

        {isLoadingOrders ? (
          <p>Loading orders...</p>
        ) : orders && orders.length > 0 ? (
          <div className="space-y-4">
            {orders.map((order) => (
              <Link
                key={order.id}
                to={`/orders/${order.id}`}
                className="block border rounded-lg p-4 hover:bg-gray-50 transition-colors"
              >
                <div className="flex justify-between items-start">
                  <div>
                    <p className="font-semibold">
                      Order #{order.trackingNumber}
                    </p>
                    <p className="text-sm text-gray-600">
                      {new Date(order.purchaseDate).toLocaleDateString()}
                    </p>
                    <p className="text-sm mt-1">
                      Status:{" "}
                      <span
                        className={`font-medium ${
                          order.status === "Completed"
                            ? "text-green-600"
                            : order.status === "Pending"
                            ? "text-yellow-600"
                            : "text-blue-600"
                        }`}
                      >
                        {order.status}
                      </span>
                    </p>
                  </div>
                  <p className="font-bold">${order.total.toFixed(2)}</p>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <p className="text-gray-600">No orders yet</p>
        )}
      </div>
    </div>
  );
};
