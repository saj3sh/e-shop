import { createBrowserRouter } from "react-router-dom";
import { Layout } from "../components/Layout";
import { ProtectedRoute } from "../components/ProtectedRoute";
import { LoginPage } from "../pages/LoginPage";
import { RegisterPage } from "../pages/RegisterPage";
import { HomePage } from "../pages/HomePage";
import { ProductPage } from "../pages/ProductPage";
import { CartPage } from "../pages/CartPage";
import { CheckoutPage } from "../pages/CheckoutPage";
import { UserPage } from "../pages/UserPage";
import { AdminPage } from "../pages/AdminPage";
import { OrderDetailsPage } from "../pages/OrderDetailsPage";

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/register",
    element: <RegisterPage />,
  },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <Layout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: "products/:id",
        element: <ProductPage />,
      },
      {
        path: "cart",
        element: <CartPage />,
      },
      {
        path: "checkout",
        element: <CheckoutPage />,
      },
      {
        path: "user",
        element: <UserPage />,
      },
      {
        path: "orders/:id",
        element: <OrderDetailsPage />,
      },
      {
        path: "admin",
        element: (
          <ProtectedRoute requiredRole="Admin">
            <AdminPage />
          </ProtectedRoute>
        ),
      },
    ],
  },
]);
