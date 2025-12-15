import { useState } from "react";
import { useNavigate, useLocation, Link } from "react-router-dom";
import { apiClient, setAccessToken } from "../lib/apiClient";
import { useAuthStore } from "../stores/authStore";

export const LoginPage = () => {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const setAuth = useAuthStore((state) => state.setAuth);
  const successMessage = (location.state as any)?.message;
  const from = (location.state as any)?.from?.pathname || "/";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const response = await apiClient.post("/auth/login", { email });
      const { accessToken, role, userId } = response.data;

      setAccessToken(accessToken);
      setAuth(userId, role);

      navigate(from, { replace: true });
    } catch (err: any) {
      const errorResponse = err.response?.data;
      const errorCode = errorResponse?.code;

      // redirect to register if user not found
      if (errorCode === "USER_NOT_FOUND") {
        navigate("/register", {
          state: {
            email,
            message: "account not found. please register to continue.",
          },
        });
        return;
      }

      setError(errorResponse?.error || "login failed, try again");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow-md p-8">
        <h1 className="text-2xl font-bold text-center mb-6">
          Welcome to EShop
        </h1>
        <p className="text-gray-600 text-center mb-8">
          enter your email to login (no password needed)
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label
              htmlFor="email"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="you@example.com"
            />
          </div>

          {successMessage && (
            <div className="bg-green-50 text-green-600 p-3 rounded-md text-sm">
              {successMessage}
            </div>
          )}

          {error && (
            <div className="bg-red-50 text-red-600 p-3 rounded-md text-sm">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {loading ? "logging in..." : "login"}
          </button>
        </form>

        <p className="text-center text-sm text-gray-600 mt-6">
          don't have an account?{" "}
          <Link to="/register" className="text-blue-600 hover:underline">
            register here
          </Link>
        </p>
      </div>
    </div>
  );
};
