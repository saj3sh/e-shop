import { Outlet, Link } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { useCartStore } from "../stores/cartStore";
import { apiClient, setAccessToken } from "../lib/apiClient";

export const Layout = () => {
  const { isAuthenticated, role, clearAuth } = useAuthStore();
  const totalItems = useCartStore((state) => state.getTotalItems());

  const handleLogout = async () => {
    try {
      await apiClient.post("/auth/logout");
    } catch (error) {
      console.error("logout failed", error);
    } finally {
      setAccessToken(null);
      clearAuth();
      window.location.href = "/login";
    }
  };

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
              <Link
                to="/"
                className="flex items-center text-gray-700 hover:text-gray-900"
              >
                Products
              </Link>
            </div>

            <div className="flex items-center space-x-4">
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

              {isAuthenticated ? (
                <>
                  <Link
                    to="/user"
                    className="text-gray-700 hover:text-gray-900"
                  >
                    Profile
                  </Link>
                  {role === "Admin" && (
                    <Link
                      to="/admin"
                      className="text-gray-700 hover:text-gray-900"
                    >
                      Admin
                    </Link>
                  )}
                  <button
                    onClick={handleLogout}
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
