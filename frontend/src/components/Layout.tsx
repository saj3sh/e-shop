import { Outlet, Link } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { useCartStore } from "../stores/cartStore";
import { handleLogout } from "../lib/apiClient";
import { Badge, Button } from "./ui";

export const Layout = () => {
  const { isAuthenticated, role } = useAuthStore();
  const totalItems = useCartStore((state) => state.getTotalItems());
  const isAdmin = role === "Admin";

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
      <nav className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex space-x-8">
              <Link
                to="/"
                className="flex items-center text-xl font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent hover:from-blue-700 hover:to-indigo-700 transition-all"
              >
                EShop
              </Link>
            </div>

            <div className="flex items-center space-x-4">
              {!isAdmin && (
                <Link
                  to="/cart"
                  className="relative text-gray-700 hover:text-blue-600 transition-colors"
                >
                  <div className="flex items-center gap-2">
                    <svg
                      className="h-6 w-6"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
                      />
                    </svg>
                    <span className="text-sm font-medium">Cart</span>
                    {totalItems > 0 && (
                      <Badge variant="info" size="sm">
                        {totalItems}
                      </Badge>
                    )}
                  </div>
                </Link>
              )}

              {isAuthenticated ? (
                <>
                  {!isAdmin && (
                    <Link
                      to="/user"
                      className="text-gray-700 hover:text-blue-600 font-medium transition-colors"
                    >
                      Profile
                    </Link>
                  )}
                  {isAdmin && (
                    <>
                      <Link
                        to="/admin"
                        className="text-gray-700 hover:text-blue-600 font-medium transition-colors"
                      >
                        Orders
                      </Link>
                      <Link
                        to="/admin/activity-logs"
                        className="text-gray-700 hover:text-blue-600 font-medium transition-colors"
                      >
                        Activity Logs
                      </Link>
                    </>
                  )}
                  <Button
                    onClick={() => handleLogout()}
                    variant="outline"
                    size="sm"
                  >
                    Logout
                  </Button>
                </>
              ) : (
                <Link to="/login">
                  <Button variant="primary" size="sm">
                    Login
                  </Button>
                </Link>
              )}
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Outlet />
      </main>
    </div>
  );
};
