import { Navigate, useLocation } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string;
  excludeRole?: string;
}

export const ProtectedRoute = ({
  children,
  requiredRole,
  excludeRole,
}: ProtectedRouteProps) => {
  const { isAuthenticated, role } = useAuthStore();
  const location = useLocation();

  if (!isAuthenticated) {
    // Save the intended destination to redirect after login
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requiredRole && role !== requiredRole) {
    return <Navigate to="/" replace />;
  }

  if (excludeRole && role === excludeRole) {
    return <Navigate to="/admin" replace />;
  }

  return <>{children}</>;
};
