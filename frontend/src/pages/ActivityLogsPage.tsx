import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";
import { useState } from "react";

interface ActivityLog {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  userId: string | null;
  userEmail: string | null;
  timestamp: string;
  details: string | null;
  ipAddress: string | null;
}

export const ActivityLogsPage = () => {
  const [entityTypeFilter, setEntityTypeFilter] = useState<string>("");
  const [limit, setLimit] = useState(100);

  const { data: logs, isLoading } = useQuery<ActivityLog[]>({
    queryKey: ["activity-logs", entityTypeFilter, limit],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (entityTypeFilter) params.append("entityType", entityTypeFilter);
      params.append("limit", limit.toString());

      const response = await apiClient.get(
        `/activitylogs${params.toString() ? `?${params.toString()}` : ""}`
      );
      return response.data;
    },
    refetchOnMount: "always",
  });

  const getActionColor = (action: string) => {
    if (action.includes("Created") || action.includes("Placed"))
      return "text-green-700 bg-green-50";
    if (action.includes("Updated") || action.includes("Processing"))
      return "text-blue-700 bg-blue-50";
    if (action.includes("Deleted") || action.includes("Cancelled"))
      return "text-red-700 bg-red-50";
    if (action.includes("Completed")) return "text-gray-700 bg-gray-50";
    return "text-gray-700 bg-gray-100";
  };

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    return new Intl.DateTimeFormat("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    }).format(date);
  };

  const uniqueEntityTypes = ["UserAccount", "Order"];

  return (
    <div className="max-w-7xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">activity logs</h1>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              filter by entity type
            </label>
            <select
              value={entityTypeFilter}
              onChange={(e) => setEntityTypeFilter(e.target.value)}
              className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">all types</option>
              {uniqueEntityTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <div className="flex-1">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              limit
            </label>
            <select
              value={limit}
              onChange={(e) => setLimit(Number(e.target.value))}
              className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value={50}>50 entries</option>
              <option value={100}>100 entries</option>
              <option value={250}>250 entries</option>
              <option value={500}>500 entries</option>
            </select>
          </div>
          {entityTypeFilter && (
            <div className="flex items-end">
              <button
                onClick={() => setEntityTypeFilter("")}
                className="px-4 py-2 text-sm text-gray-600 hover:text-gray-900 underline"
              >
                clear filter
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Activity Logs Table */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-xl font-bold mb-4">
          recent activity
          {entityTypeFilter && ` (${entityTypeFilter})`}
        </h2>

        {isLoading ? (
          <p>loading activity logs...</p>
        ) : logs && logs.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-3 px-2 font-semibold">
                    timestamp
                  </th>
                  <th className="text-left py-3 px-2 font-semibold">entity</th>
                  <th className="text-left py-3 px-2 font-semibold">action</th>
                  <th className="text-left py-3 px-2 font-semibold">user</th>
                  <th className="text-left py-3 px-2 font-semibold">details</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((log) => (
                  <tr
                    key={log.id}
                    className="border-b hover:bg-gray-50 transition-colors"
                  >
                    <td className="py-3 px-2 text-sm text-gray-600">
                      {formatTimestamp(log.timestamp)}
                    </td>
                    <td className="py-3 px-2">
                      <div className="text-sm">
                        <div className="font-medium text-gray-900">
                          {log.entityType}
                        </div>
                        <div className="text-xs text-gray-500 font-mono">
                          {log.entityId.substring(0, 8)}...
                        </div>
                      </div>
                    </td>
                    <td className="py-3 px-2">
                      <span
                        className={`inline-block px-2 py-1 rounded-full text-xs font-medium ${getActionColor(
                          log.action
                        )}`}
                      >
                        {log.action}
                      </span>
                    </td>
                    <td className="py-3 px-2">
                      {log.userEmail ? (
                        <div className="text-sm">
                          <div className="text-gray-900">{log.userEmail}</div>
                          {log.ipAddress && (
                            <div className="text-xs text-gray-500">
                              {log.ipAddress}
                            </div>
                          )}
                        </div>
                      ) : (
                        <span className="text-sm text-gray-400">system</span>
                      )}
                    </td>
                    <td className="py-3 px-2 text-sm text-gray-600">
                      {log.details || "-"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-gray-600">
            {entityTypeFilter
              ? `no activity logs found for ${entityTypeFilter}`
              : "no activity logs yet"}
          </p>
        )}

        {logs && logs.length > 0 && (
          <div className="mt-4 text-sm text-gray-600">
            showing {logs.length} activities
          </div>
        )}
      </div>
    </div>
  );
};
