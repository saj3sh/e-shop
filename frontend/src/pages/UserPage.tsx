import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { apiClient } from "../lib/apiClient";
import { useState } from "react";
import { AddressManagement } from "../components/AddressManagement";

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

interface UpdateCustomerRequest {
  firstName: string;
  lastName: string;
  phone: string;
}

export const UserPage = () => {
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState<UpdateCustomerRequest>({
    firstName: "",
    lastName: "",
    phone: "",
  });

  const { data: customer, isLoading: isLoadingCustomer } = useQuery<Customer>({
    queryKey: ["customer-me"],
    queryFn: async () => {
      const response = await apiClient.get("/customers/me");
      return response.data;
    },
  });

  const updateMutation = useMutation({
    mutationFn: async (data: UpdateCustomerRequest) => {
      await apiClient.put("/customers/me", data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["customer-me"] });
      setIsEditing(false);
    },
  });

  const handleEditClick = () => {
    if (customer) {
      setEditForm({
        firstName: customer.firstName,
        lastName: customer.lastName,
        phone: customer.phone,
      });
      setIsEditing(true);
    }
  };

  const handleSave = () => {
    updateMutation.mutate(editForm);
  };

  const handleCancel = () => {
    setIsEditing(false);
  };

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
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-bold">Personal Information</h2>
          {!isEditing && customer && (
            <button
              onClick={handleEditClick}
              className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Edit
            </button>
          )}
        </div>

        {isLoadingCustomer ? (
          <p>Loading...</p>
        ) : customer ? (
          isEditing ? (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  First Name
                </label>
                <input
                  type="text"
                  value={editForm.firstName}
                  onChange={(e) =>
                    setEditForm({ ...editForm, firstName: e.target.value })
                  }
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Last Name
                </label>
                <input
                  type="text"
                  value={editForm.lastName}
                  onChange={(e) =>
                    setEditForm({ ...editForm, lastName: e.target.value })
                  }
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Phone
                </label>
                <input
                  type="tel"
                  value={editForm.phone}
                  onChange={(e) =>
                    setEditForm({ ...editForm, phone: e.target.value })
                  }
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email
                </label>
                <input
                  type="email"
                  value={customer.email}
                  disabled
                  className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100 text-gray-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Email cannot be changed
                </p>
              </div>
              {updateMutation.isError && (
                <p className="text-red-600 text-sm">
                  Failed to update profile. Please try again.
                </p>
              )}
              <div className="flex gap-3">
                <button
                  onClick={handleSave}
                  disabled={updateMutation.isPending}
                  className="px-6 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:bg-gray-400"
                >
                  {updateMutation.isPending ? "Saving..." : "Save"}
                </button>
                <button
                  onClick={handleCancel}
                  disabled={updateMutation.isPending}
                  className="px-6 py-2 bg-gray-300 text-gray-700 rounded hover:bg-gray-400 disabled:bg-gray-200"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
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
          )
        ) : (
          <p className="text-red-600">Failed to load customer information</p>
        )}
      </div>

      {/* Orders Section */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
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

      {/* Addresses Section */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-xl font-bold mb-4">My Addresses</h2>
        <AddressManagement />
      </div>
    </div>
  );
};
