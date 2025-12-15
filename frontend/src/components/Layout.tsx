import { Outlet, Link } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { useCartStore } from "../stores/cartStore";
import { handleLogout } from "../lib/apiClient";

export const Layout = () => {
  const { isAuthenticated, role } = useAuthStore();
  const totalItems = useCartStore((state) => state.getTotalItems());
  const isAdmin = role === "Admin";

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex space-x-8">
              <Link
                to="/"
                className="flex items-center text-xl font-bold text-gray-900"
              >
                EShop
              </Link>
              {!isAdmin && (
                <Link
                  to="/"
                  className="flex items-center text-gray-700 hover:text-gray-900"
                >
                  Products
                </Link>
              )}
            </div>

            <div className="flex items-center space-x-4">
              {!isAdmin && (
                <Link
                  to="/cart"
                  className="relative text-gray-700 hover:text-gray-900"
                >
                  <span className="text-sm">Cart</span>
                  {totalItems > 0 && (
                    <span className="absolute -top-2 -right-2 bg-blue-600 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
                      {totalItems}
                    </span>
                  )}
                </Link>
              )}

              {isAuthenticated ? (
                <>
                  {!isAdmin && (
                    <Link
                      to="/user"
                      className="text-gray-700 hover:text-gray-900"
                    >
                      Profile
                    </Link>
                  )}
                  {isAdmin && (
                    <>
                      <Link
                        to="/admin"
                        className="text-gray-700 hover:text-gray-900"
                      >
                        Orders
                      </Link>
                      <Link
                        to="/admin/activity-logs"
                        className="text-gray-700 hover:text-gray-900"
                      >
                        Activity Logs
                      </Link>
                    </>
                  )}
                  <button
                    onClick={() => handleLogout()}
                    className="text-gray-700 hover:text-gray-900"
                  >
                    Logout
                  </button>
                </>
              ) : (
                <Link to="/login" className="text-gray-700 hover:text-gray-900">
                  Login
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
