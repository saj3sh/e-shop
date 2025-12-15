import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";
import { useState } from "react";
import { OrderActionsMenu } from "../components/OrderActionsMenu";
import { Card, Badge, Button, Input, Select } from "../components/ui";
import { LoadingState } from "../components/LoadingState";

interface Order {
  id: string;
  customerId: string;
  status: string;
  trackingNumber: string;
  purchaseDate: string;
  total: number;
}

interface OrderStatistics {
  totalOrders: number;
  pendingOrders: number;
  processingOrders: number;
  shippedOrders: number;
  deliveredOrders: number;
  completedOrders: number;
  cancelledOrders: number;
}

interface OrdersResponse {
  orders: Order[];
  statistics: OrderStatistics;
}

export const AdminPage = () => {
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 25;

  const { data, isLoading } = useQuery<OrdersResponse>({
    queryKey: ["admin-orders", statusFilter, searchQuery],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (statusFilter) params.append("status", statusFilter);
      if (searchQuery) params.append("trackingNumber", searchQuery);

      const response = await apiClient.get(
        `/admin/orders${params.toString() ? `?${params.toString()}` : ""}`
      );
      return response.data;
    },
  });

  // Pagination
  const orders = data?.orders || [];
  const totalPages = Math.ceil(orders.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const currentOrders = orders.slice(startIndex, endIndex);

  // Reset to page 1 when filters change
  const handleFilterChange = (filter: string) => {
    setStatusFilter(filter);
    setCurrentPage(1);
  };

  const handleSearchChange = (search: string) => {
    setSearchQuery(search);
    setCurrentPage(1);
  };

  const getStatusVariant = (
    status: string
  ): "success" | "warning" | "danger" | "info" | "purple" | "gray" => {
    switch (status.toLowerCase()) {
      case "pending":
        return "warning";
      case "processing":
        return "info";
      case "shipped":
        return "purple";
      case "delivered":
        return "success";
      case "completed":
        return "gray";
      case "cancelled":
        return "danger";
      default:
        return "gray";
    }
  };

  return (
    <div className="max-w-7xl mx-auto">
      <h1 className="text-3xl font-bold mb-8 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
        Admin Dashboard
      </h1>

      {/* Statistics Cards */}
      {data?.statistics && (
        <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-4 mb-8">
          <Card hover className="text-center cursor-default" padding="sm">
            <div className="text-sm text-gray-600 mb-1">Total Orders</div>
            <div className="text-2xl font-bold text-gray-900">
              {data.statistics.totalOrders}
            </div>
          </Card>
          {[
            {
              label: "Pending",
              value: data.statistics.pendingOrders,
              color: "bg-yellow-50",
              textColor: "text-yellow-900",
              subText: "text-yellow-700",
              status: "Pending",
            },
            {
              label: "Processing",
              value: data.statistics.processingOrders,
              color: "bg-blue-50",
              textColor: "text-blue-900",
              subText: "text-blue-700",
              status: "Processing",
            },
            {
              label: "Shipped",
              value: data.statistics.shippedOrders,
              color: "bg-purple-50",
              textColor: "text-purple-900",
              subText: "text-purple-700",
              status: "Shipped",
            },
            {
              label: "Delivered",
              value: data.statistics.deliveredOrders,
              color: "bg-green-50",
              textColor: "text-green-900",
              subText: "text-green-700",
              status: "Delivered",
            },
            {
              label: "Completed",
              value: data.statistics.completedOrders,
              color: "bg-gray-50",
              textColor: "text-gray-900",
              subText: "text-gray-700",
              status: "Completed",
            },
            {
              label: "Cancelled",
              value: data.statistics.cancelledOrders,
              color: "bg-red-50",
              textColor: "text-red-900",
              subText: "text-red-700",
              status: "Cancelled",
            },
          ].map((stat) => (
            <Card
              key={stat.label}
              hover
              className={`${stat.color} cursor-pointer text-center`}
              onClick={() => handleFilterChange(stat.status)}
              padding="sm"
            >
              <div className={`text-sm ${stat.subText} mb-1`}>{stat.label}</div>
              <div className={`text-2xl font-bold ${stat.textColor}`}>
                {stat.value}
              </div>
            </Card>
          ))}
        </div>
      )}

      <Card padding="lg">
        {/* Filters and Search */}
        <div className="mb-6 flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <Select
              label="Filter by status"
              value={statusFilter}
              onChange={(e) => handleFilterChange(e.target.value)}
            >
              <option value="">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Processing">Processing</option>
              <option value="Shipped">Shipped</option>
              <option value="Delivered">Delivered</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </Select>
          </div>
          <div className="flex-1">
            <Input
              label="Search by tracking number"
              type="text"
              value={searchQuery}
              onChange={(e) => handleSearchChange(e.target.value)}
              placeholder="Enter tracking number..."
            />
          </div>
          {(statusFilter || searchQuery) && (
            <div className="flex items-end">
              <Button
                variant="ghost"
                onClick={() => {
                  setStatusFilter("");
                  setSearchQuery("");
                  setCurrentPage(1);
                }}
                size="sm"
              >
                Clear filters
              </Button>
            </div>
          )}
        </div>

        <h2 className="text-xl font-bold mb-4 text-gray-900">
          Orders
          {statusFilter && ` (${statusFilter})`}
          {searchQuery && ` matching "${searchQuery}"`}
        </h2>

        {isLoading ? (
          <LoadingState message="Loading orders..." />
        ) : currentOrders.length > 0 ? (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b">
                    <th className="text-left py-3 px-2 font-semibold text-gray-700">
                      Tracking #
                    </th>
                    <th className="text-left py-3 px-2 font-semibold text-gray-700">
                      Status
                    </th>
                    <th className="text-left py-3 px-2 font-semibold text-gray-700">
                      Date
                    </th>
                    <th className="text-right py-3 px-2 font-semibold text-gray-700">
                      Total
                    </th>
                    <th className="text-right py-3 px-2 font-semibold text-gray-700">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {currentOrders.map((order) => (
                    <tr
                      key={order.id}
                      className="border-b hover:bg-gray-50 transition-colors"
                    >
                      <td className="py-3 px-2 font-mono text-sm text-gray-900">
                        {order.trackingNumber}
                      </td>
                      <td className="py-3 px-2">
                        <Badge variant={getStatusVariant(order.status)}>
                          {order.status}
                        </Badge>
                      </td>
                      <td className="py-3 px-2 text-sm text-gray-700">
                        {new Date(order.purchaseDate).toLocaleDateString()}
                      </td>
                      <td className="text-right py-3 px-2 font-medium text-gray-900">
                        ${order.total.toFixed(2)}
                      </td>
                      <td className="text-right py-3 px-2">
                        <OrderActionsMenu
                          orderId={order.id}
                          currentStatus={order.status}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="mt-6 flex flex-col sm:flex-row items-center justify-between gap-4">
                <div className="text-sm text-gray-600">
                  Showing {startIndex + 1} to{" "}
                  {Math.min(endIndex, orders.length)} of {orders.length} orders
                </div>
                <div className="flex gap-2 items-center">
                  <Button
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                    variant="outline"
                    size="sm"
                  >
                    Previous
                  </Button>
                  <div className="flex gap-1">
                    {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                      let pageNum;
                      if (totalPages <= 5) {
                        pageNum = i + 1;
                      } else if (currentPage <= 3) {
                        pageNum = i + 1;
                      } else if (currentPage >= totalPages - 2) {
                        pageNum = totalPages - 4 + i;
                      } else {
                        pageNum = currentPage - 2 + i;
                      }
                      return (
                        <Button
                          key={pageNum}
                          onClick={() => setCurrentPage(pageNum)}
                          variant={
                            currentPage === pageNum ? "primary" : "outline"
                          }
                          size="sm"
                        >
                          {pageNum}
                        </Button>
                      );
                    })}
                  </div>
                  <Button
                    onClick={() =>
                      setCurrentPage((p) => Math.min(totalPages, p + 1))
                    }
                    disabled={currentPage === totalPages}
                    variant="outline"
                    size="sm"
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </>
        ) : (
          <p className="text-gray-600 py-8 text-center">
            {statusFilter || searchQuery
              ? "No orders found matching your filters"
              : "No orders yet"}
          </p>
        )}
      </Card>
    </div>
  );
};
