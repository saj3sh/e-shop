import { useState } from "react";
import { useNavigate, useLocation, Link } from "react-router-dom";
import toast from "react-hot-toast";
import { apiClient } from "../lib/apiClient";
import { useAuthStore } from "../stores/authStore";
import { useCartStore } from "../stores/cartStore";
import { Button, Input, Card, Alert } from "../components/ui";

export const LoginPage = () => {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const setAuth = useAuthStore((state) => state.setAuth);
  const switchUser = useCartStore((state) => state.switchUser);
  const successMessage = (location.state as any)?.message;
  const from = (location.state as any)?.from?.pathname || "/";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const response = await apiClient.post("/auth/login", { email });
      const { accessToken, role, userId } = response.data;

      setAuth(userId, role, accessToken);
      switchUser(userId);

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
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center px-4">
      <Card className="max-w-md w-full" padding="lg">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent mb-2">
            Welcome to EShop
          </h1>
          <p className="text-gray-600">Enter your email to login</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            id="email"
            type="email"
            label="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            placeholder="you@example.com"
          />

          {successMessage && <Alert variant="success">{successMessage}</Alert>}

          {error && <Alert variant="error">{error}</Alert>}

          <Button
            type="submit"
            isLoading={loading}
            className="w-full"
            size="lg"
          >
            {loading ? "Logging in..." : "Login"}
          </Button>
        </form>

        <p className="text-center text-sm text-gray-600 mt-6">
          Don't have an account?{" "}
          <Link
            to="/register"
            className="text-blue-600 hover:text-blue-700 font-medium"
          >
            Register here
          </Link>
        </p>
      </Card>
    </div>
  );
};
