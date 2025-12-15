import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";
import { useState } from "react";
import { Card, Badge, Select } from "../components/ui";
import { LoadingState } from "../components/LoadingState";

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

  const getActionVariant = (
    action: string
  ): "success" | "info" | "danger" | "gray" => {
    if (action.includes("Created") || action.includes("Placed"))
      return "success";
    if (action.includes("Updated") || action.includes("Processing"))
      return "info";
    if (action.includes("Deleted") || action.includes("Cancelled"))
      return "danger";
    return "gray";
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
      <h1 className="text-3xl font-bold mb-8 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
        Activity Logs
      </h1>

      {/* Filters */}
      <Card className="mb-6" padding="lg">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <Select
              label="Filter by entity type"
              value={entityTypeFilter}
              onChange={(e) => setEntityTypeFilter(e.target.value)}
            >
              <option value="">All types</option>
              {uniqueEntityTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </Select>
          </div>
          <div className="flex-1">
            <Select
              label="Limit"
              value={limit}
              onChange={(e) => setLimit(Number(e.target.value))}
            >
              <option value={50}>50 entries</option>
              <option value={100}>100 entries</option>
              <option value={250}>250 entries</option>
              <option value={500}>500 entries</option>
            </Select>
          </div>
        </div>
      </Card>

      {/* Activity Logs Table */}
      <Card padding="lg">
        <h2 className="text-xl font-bold mb-4 text-gray-900">
          Recent Activity
          {entityTypeFilter && ` (${entityTypeFilter})`}
        </h2>

        {isLoading ? (
          <LoadingState message="Loading activity logs..." />
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
                      <Badge variant={getActionVariant(log.action)}>
                        {log.action}
                      </Badge>
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
            Showing {logs.length} activities
          </div>
        )}
      </Card>
    </div>
  );
};
