import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";
import { CardIcon } from "../components/CardIcon";

interface OrderItem {
  productId: string;
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

  if (isLoading) {
    return <div className="text-center py-12">loading order details...</div>;
  }

  if (!order) {
    return <div className="text-center py-12">order not found</div>;
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">order details</h1>

      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <div className="grid grid-cols-2 gap-4 mb-6">
          <div>
            <p className="text-sm text-gray-600">tracking number</p>
            <p className="font-semibold">{order.trackingNumber}</p>
          </div>
          <div>
            <p className="text-sm text-gray-600">status</p>
            <p className="font-semibold">{order.status}</p>
          </div>
          <div>
            <p className="text-sm text-gray-600">order date</p>
            <p className="font-semibold">
              {new Date(order.purchaseDate).toLocaleDateString()}
            </p>
          </div>
          <div>
            <p className="text-sm text-gray-600">estimated delivery</p>
            <p className="font-semibold">
              {new Date(order.estimatedDelivery).toLocaleDateString()}
            </p>
          </div>
          {order.paymentCard && (
            <div>
              <p className="text-sm text-gray-600">payment method</p>
              <div className="flex items-center gap-2 mt-1">
                <CardIcon type={order.paymentCard.split(" ")[0]} />
                <span className="font-semibold">{order.paymentCard}</span>
              </div>
            </div>
          )}
        </div>

        <div className="border-t pt-4">
          <h3 className="font-bold mb-4">items</h3>
          <div className="space-y-2">
            {order.items.map((item, index) => (
              <div key={index} className="flex justify-between">
                <span>
                  Item {index + 1} × {item.quantity}
                </span>
                <span>${item.totalPrice.toFixed(2)}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="border-t mt-4 pt-4">
          <div className="flex justify-between text-xl font-bold">
            <span>Total:</span>
            <span>${order.total.toFixed(2)}</span>
          </div>
        </div>
      </div>
    </div>
  );
};
