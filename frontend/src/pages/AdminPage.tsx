import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";
import { useState } from "react";
import { OrderActionsMenu } from "../components/OrderActionsMenu";

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

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "pending":
        return "bg-yellow-100 text-yellow-800";
      case "processing":
        return "bg-blue-100 text-blue-800";
      case "shipped":
        return "bg-purple-100 text-purple-800";
      case "delivered":
        return "bg-green-100 text-green-800";
      case "completed":
        return "bg-gray-100 text-gray-800";
      case "cancelled":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <div className="max-w-7xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">admin dashboard</h1>

      {/* Statistics Cards */}
      {data?.statistics && (
        <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-4 mb-8">
          <div className="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow">
            <div className="text-sm text-gray-600 mb-1">total orders</div>
            <div className="text-2xl font-bold text-gray-900">
              {data.statistics.totalOrders}
            </div>
          </div>
          <div
            className="bg-yellow-50 rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer"
            onClick={() => handleFilterChange("Pending")}
          >
            <div className="text-sm text-yellow-700 mb-1">pending</div>
            <div className="text-2xl font-bold text-yellow-900">
              {data.statistics.pendingOrders}
            </div>
          </div>
          <div
            className="bg-blue-50 rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer"
            onClick={() => handleFilterChange("Processing")}
          >
            <div className="text-sm text-blue-700 mb-1">processing</div>
            <div className="text-2xl font-bold text-blue-900">
              {data.statistics.processingOrders}
            </div>
          </div>
          <div
            className="bg-purple-50 rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer"
            onClick={() => handleFilterChange("Shipped")}
          >
            <div className="text-sm text-purple-700 mb-1">shipped</div>
            <div className="text-2xl font-bold text-purple-900">
              {data.statistics.shippedOrders}
            </div>
          </div>
          <div
            className="bg-green-50 rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer"
            onClick={() => handleFilterChange("Delivered")}
          >
            <div className="text-sm text-green-700 mb-1">delivered</div>
            <div className="text-2xl font-bold text-green-900">
              {data.statistics.deliveredOrders}
            </div>
          </div>
          <div
            className="bg-gray-50 rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer"
            onClick={() => handleFilterChange("Completed")}
          >
            <div className="text-sm text-gray-700 mb-1">completed</div>
            <div className="text-2xl font-bold text-gray-900">
              {data.statistics.completedOrders}
            </div>
          </div>
          <div
            className="bg-red-50 rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow cursor-pointer"
            onClick={() => handleFilterChange("Cancelled")}
          >
            <div className="text-sm text-red-700 mb-1">cancelled</div>
            <div className="text-2xl font-bold text-red-900">
              {data.statistics.cancelledOrders}
            </div>
          </div>
        </div>
      )}

      <div className="bg-white rounded-lg shadow-md p-6">
        {/* Filters and Search */}
        <div className="mb-6 flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              filter by status
            </label>
            <select
              value={statusFilter}
              onChange={(e) => handleFilterChange(e.target.value)}
              className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">all statuses</option>
              <option value="Pending">pending</option>
              <option value="Processing">processing</option>
              <option value="Shipped">shipped</option>
              <option value="Delivered">delivered</option>
              <option value="Completed">completed</option>
              <option value="Cancelled">cancelled</option>
            </select>
          </div>
          <div className="flex-1">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              search by tracking number
            </label>
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => handleSearchChange(e.target.value)}
              placeholder="enter tracking number..."
              className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          {(statusFilter || searchQuery) && (
            <div className="flex items-end">
              <button
                onClick={() => {
                  setStatusFilter("");
                  setSearchQuery("");
                  setCurrentPage(1);
                }}
                className="px-4 py-2 text-sm text-gray-600 hover:text-gray-900 underline"
              >
                clear filters
              </button>
            </div>
          )}
        </div>

        <h2 className="text-xl font-bold mb-4">
          orders {statusFilter && `(${statusFilter.toLowerCase()})`}
          {searchQuery && ` matching "${searchQuery}"`}
        </h2>

        {isLoading ? (
          <p>loading orders...</p>
        ) : currentOrders.length > 0 ? (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b">
                    <th className="text-left py-3 px-2">tracking #</th>
                    <th className="text-left py-3 px-2">status</th>
                    <th className="text-left py-3 px-2">date</th>
                    <th className="text-right py-3 px-2">total</th>
                    <th className="text-right py-3 px-2">actions</th>
                  </tr>
                </thead>
                <tbody>
                  {currentOrders.map((order) => (
                    <tr
                      key={order.id}
                      className="border-b hover:bg-gray-50 transition-colors"
                    >
                      <td className="py-3 px-2 font-mono text-sm">
                        {order.trackingNumber}
                      </td>
                      <td className="py-3 px-2">
                        <span
                          className={`inline-block px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                            order.status
                          )}`}
                        >
                          {order.status}
                        </span>
                      </td>
                      <td className="py-3 px-2 text-sm">
                        {new Date(order.purchaseDate).toLocaleDateString()}
                      </td>
                      <td className="text-right py-3 px-2 font-medium">
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
              <div className="mt-6 flex items-center justify-between">
                <div className="text-sm text-gray-600">
                  showing {startIndex + 1} to{" "}
                  {Math.min(endIndex, orders.length)} of {orders.length} orders
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                    className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    previous
                  </button>
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
                        <button
                          key={pageNum}
                          onClick={() => setCurrentPage(pageNum)}
                          className={`px-3 py-1 border rounded transition-colors ${
                            currentPage === pageNum
                              ? "bg-blue-600 text-white border-blue-600"
                              : "border-gray-300 hover:bg-gray-50"
                          }`}
                        >
                          {pageNum}
                        </button>
                      );
                    })}
                  </div>
                  <button
                    onClick={() =>
                      setCurrentPage((p) => Math.min(totalPages, p + 1))
                    }
                    disabled={currentPage === totalPages}
                    className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    next
                  </button>
                </div>
              </div>
            )}
          </>
        ) : (
          <p className="text-gray-600">
            {statusFilter || searchQuery
              ? "no orders found matching your filters"
              : "no orders yet"}
          </p>
        )}
      </div>
    </div>
  );
};
