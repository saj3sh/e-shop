import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";
import { CardIcon } from "../components/CardIcon";
import { Card, Badge } from "../components/ui";
import { LoadingState } from "../components/LoadingState";

interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

interface Order {
  id: string;
  status: string;
  trackingNumber: string;
  purchaseDate: string;
  estimatedDelivery: string;
  total: number;
  paymentCard?: string;
  items: OrderItem[];
}

export const OrderDetailsPage = () => {
  const { id } = useParams<{ id: string }>();

  const { data: order, isLoading } = useQuery({
    queryKey: ["order", id],
    queryFn: async (): Promise<Order> => {
      const response = await apiClient.get(`/orders/${id}`);
      return response.data;
    },
  });

  const getStatusVariant = (
    status: string
  ): "success" | "warning" | "info" | "purple" | "gray" => {
    switch (status.toLowerCase()) {
      case "delivered":
      case "completed":
        return "success";
      case "pending":
        return "warning";
      case "processing":
        return "info";
      case "shipped":
        return "purple";
      default:
        return "gray";
    }
  };

  if (isLoading) {
    return <LoadingState message="Loading order details..." />;
  }

  if (!order) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 mb-2">
          Order not found
        </h2>
        <p className="text-gray-600">
          The order you're looking for doesn't exist.
        </p>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold mb-8 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
        Order Details
      </h1>

      <Card className="mb-6" padding="lg">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
          <div>
            <p className="text-sm text-gray-600 mb-1">Tracking Number</p>
            <p className="font-semibold text-lg text-gray-900">
              {order.trackingNumber}
            </p>
          </div>
          <div>
            <p className="text-sm text-gray-600 mb-1">Status</p>
            <Badge variant={getStatusVariant(order.status)} size="lg">
              {order.status}
            </Badge>
          </div>
          <div>
            <p className="text-sm text-gray-600 mb-1">Order Date</p>
            <p className="font-semibold text-gray-900">
              {new Date(order.purchaseDate).toLocaleDateString("en-US", {
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </p>
          </div>
          <div>
            <p className="text-sm text-gray-600 mb-1">Estimated Delivery</p>
            <p className="font-semibold text-gray-900">
              {new Date(order.estimatedDelivery).toLocaleDateString("en-US", {
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </p>
          </div>
          {order.paymentCard && (
            <div className="md:col-span-2">
              <p className="text-sm text-gray-600 mb-1">Payment Method</p>
              <div className="flex items-center gap-2 mt-1">
                <CardIcon type={order.paymentCard.split(" ")[0]} />
                <span className="font-semibold text-gray-900">
                  {order.paymentCard}
                </span>
              </div>
            </div>
          )}
        </div>

        <div className="border-t pt-6">
          <h3 className="font-bold text-lg mb-4 text-gray-900">Order Items</h3>
          <div className="space-y-3">
            {order.items.map((item, index) => (
              <div
                key={index}
                className="flex justify-between items-center py-2"
              >
                <div className="flex-1">
                  <p className="font-medium text-gray-900">
                    {item.productName}
                  </p>
                  <p className="text-sm text-gray-600">
                    ${item.unitPrice.toFixed(2)} × {item.quantity}
                  </p>
                </div>
                <span className="font-semibold text-gray-900">
                  ${item.totalPrice.toFixed(2)}
                </span>
              </div>
            ))}
          </div>
        </div>

        <div className="border-t mt-6 pt-6">
          <div className="flex justify-between items-center">
            <span className="text-xl font-bold text-gray-900">Total:</span>
            <span className="text-3xl font-bold text-blue-600">
              ${order.total.toFixed(2)}
            </span>
          </div>
        </div>
      </Card>
    </div>
  );
};
