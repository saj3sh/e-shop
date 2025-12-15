import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";
import { apiClient } from "../lib/apiClient";
import { useState } from "react";
import { AddressManagement } from "../components/AddressManagement";
import { Button, Input, Card, Badge } from "../components/ui";
import { LoadingState } from "../components/LoadingState";

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
      toast.success("Profile updated successfully!");
    },
    onError: () => {
      toast.error("Failed to update profile. Please try again.");
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
      <h1 className="text-3xl font-bold mb-8 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
        My Profile
      </h1>

      {/* Customer Information */}
      <Card className="mb-8" padding="lg">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-bold text-gray-900">
            Personal Information
          </h2>
          {!isEditing && customer && (
            <Button onClick={handleEditClick} variant="primary">
              Edit
            </Button>
          )}
        </div>

        {isLoadingCustomer ? (
          <LoadingState message="Loading..." />
        ) : customer ? (
          isEditing ? (
            <div className="space-y-4">
              <Input
                label="First Name"
                type="text"
                value={editForm.firstName}
                onChange={(e) =>
                  setEditForm({ ...editForm, firstName: e.target.value })
                }
              />
              <Input
                label="Last Name"
                type="text"
                value={editForm.lastName}
                onChange={(e) =>
                  setEditForm({ ...editForm, lastName: e.target.value })
                }
              />
              <Input
                label="Phone"
                type="tel"
                value={editForm.phone}
                onChange={(e) =>
                  setEditForm({ ...editForm, phone: e.target.value })
                }
              />
              <Input
                label="Email"
                type="email"
                value={customer.email}
                disabled
                helperText="Email cannot be changed"
              />
              <div className="flex gap-3">
                <Button
                  onClick={handleSave}
                  isLoading={updateMutation.isPending}
                  variant="primary"
                >
                  {updateMutation.isPending ? "Saving..." : "Save"}
                </Button>
                <Button
                  onClick={handleCancel}
                  disabled={updateMutation.isPending}
                  variant="secondary"
                >
                  Cancel
                </Button>
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
      </Card>

      {/* Orders Section */}
      <Card className="mb-8" padding="lg">
        <h2 className="text-xl font-bold mb-4 text-gray-900">My Orders</h2>

        {isLoadingOrders ? (
          <LoadingState message="Loading orders..." />
        ) : orders && orders.length > 0 ? (
          <div className="space-y-4">
            {orders.map((order) => (
              <Link
                key={order.id}
                to={`/orders/${order.id}`}
                className="block border rounded-lg p-4 hover:bg-gray-50 hover:border-blue-300 transition-all"
              >
                <div className="flex justify-between items-start">
                  <div>
                    <p className="font-semibold text-gray-900">
                      Order #{order.trackingNumber}
                    </p>
                    <p className="text-sm text-gray-600 mt-1">
                      {new Date(order.purchaseDate).toLocaleDateString(
                        "en-US",
                        {
                          year: "numeric",
                          month: "long",
                          day: "numeric",
                        }
                      )}
                    </p>
                    <div className="mt-2">
                      <Badge
                        variant={
                          order.status === "Completed" ||
                          order.status === "Delivered"
                            ? "success"
                            : order.status === "Pending"
                            ? "warning"
                            : "info"
                        }
                      >
                        {order.status}
                      </Badge>
                    </div>
                  </div>
                  <p className="font-bold text-gray-900">
                    ${order.total.toFixed(2)}
                  </p>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <p className="text-gray-600">No orders yet</p>
        )}
      </Card>

      {/* Addresses Section */}
      <Card padding="lg">
        <h2 className="text-xl font-bold mb-4 text-gray-900">My Addresses</h2>
        <AddressManagement />
      </Card>
    </div>
  );
};
