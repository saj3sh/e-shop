import { useState, useRef, useEffect } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "../lib/apiClient";

interface OrderActionsMenuProps {
  orderId: string;
  currentStatus: string;
}

export const OrderActionsMenu = ({
  orderId,
  currentStatus,
}: OrderActionsMenuProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const queryClient = useQueryClient();

  const updateStatusMutation = useMutation({
    mutationFn: async (newStatus: string) => {
      await apiClient.put(`/admin/orders/${orderId}/status`, {
        status: newStatus,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-orders"] });
      setIsOpen(false);
    },
  });

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  // Get available status transitions based on current status
  const getAvailableActions = () => {
    const status = currentStatus.toLowerCase();

    if (status === "completed" || status === "cancelled") {
      return [];
    }

    const actions: { label: string; value: string; color: string }[] = [];

    switch (status) {
      case "pending":
        actions.push(
          { label: "Mark as Processing", value: "Processing", color: "blue" },
          { label: "Cancel Order", value: "Cancelled", color: "red" }
        );
        break;
      case "processing":
        actions.push(
          { label: "Mark as Shipped", value: "Shipped", color: "purple" },
          { label: "Cancel Order", value: "Cancelled", color: "red" }
        );
        break;
      case "shipped":
        actions.push(
          { label: "Mark as Delivered", value: "Delivered", color: "green" },
          { label: "Cancel Order", value: "Cancelled", color: "red" }
        );
        break;
      case "delivered":
        actions.push(
          { label: "Mark as Completed", value: "Completed", color: "gray" },
          { label: "Cancel Order", value: "Cancelled", color: "red" }
        );
        break;
    }

    return actions;
  };

  const availableActions = getAvailableActions();
  const hasActions = availableActions.length > 0;

  const getButtonColor = (color: string) => {
    switch (color) {
      case "blue":
        return "text-blue-600 hover:bg-blue-50";
      case "purple":
        return "text-purple-600 hover:bg-purple-50";
      case "green":
        return "text-green-600 hover:bg-green-50";
      case "gray":
        return "text-gray-600 hover:bg-gray-50";
      case "red":
        return "text-red-600 hover:bg-red-50";
      default:
        return "text-gray-600 hover:bg-gray-50";
    }
  };

  return (
    <div className="relative" ref={menuRef}>
      <button
        onClick={() => hasActions && setIsOpen(!isOpen)}
        disabled={!hasActions}
        className={`p-2 rounded transition-colors ${
          hasActions
            ? "hover:bg-gray-100 cursor-pointer"
            : "cursor-not-allowed opacity-40"
        }`}
        aria-label="Order actions"
      >
        <svg
          className="w-5 h-5 text-gray-600"
          fill="currentColor"
          viewBox="0 0 16 16"
        >
          <circle cx="8" cy="3" r="1.5" />
          <circle cx="8" cy="8" r="1.5" />
          <circle cx="8" cy="13" r="1.5" />
        </svg>
      </button>

      {isOpen && hasActions && (
        <div className="absolute right-0 mt-1 w-48 bg-white rounded-md shadow-lg border border-gray-200 z-10">
          <div className="py-1">
            {availableActions.map((action) => (
              <button
                key={action.value}
                onClick={() => updateStatusMutation.mutate(action.value)}
                disabled={updateStatusMutation.isPending}
                className={`w-full text-left px-4 py-2 text-sm ${getButtonColor(
                  action.color
                )} transition-colors disabled:opacity-50 disabled:cursor-not-allowed`}
              >
                {action.label}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
