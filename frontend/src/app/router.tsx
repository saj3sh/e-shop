import { createBrowserRouter, type RouteObject } from "react-router-dom";
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
import { ActivityLogsPage } from "../pages/ActivityLogsPage";
import { OrderDetailsPage } from "../pages/OrderDetailsPage";

export const ROUTES = {
  LOGIN: "/login",
  REGISTER: "/register",
  HOME: "/",
  PRODUCT: "/products/:id",
  CART: "/cart",
  CHECKOUT: "/checkout",
  USER: "/user",
  ORDER_DETAILS: "/orders/:id",
  ADMIN: "/admin",
  ACTIVITY_LOGS: "/admin/activity-logs",
} as const;

const createCustomerRoute = (
  path: string | undefined,
  element: React.ReactElement,
  index?: boolean
): RouteObject => ({
  ...(index ? { index: true } : { path }),
  element: <ProtectedRoute excludeRole="Admin">{element}</ProtectedRoute>,
});

const createAdminRoute = (
  path: string,
  element: React.ReactElement
): RouteObject => ({
  path,
  element: <ProtectedRoute requiredRole="Admin">{element}</ProtectedRoute>,
});

const authRoutes: RouteObject[] = [
  {
    path: ROUTES.LOGIN,
    element: <LoginPage />,
  },
  {
    path: ROUTES.REGISTER,
    element: <RegisterPage />,
  },
];

const customerRoutes: RouteObject[] = [
  createCustomerRoute(undefined, <HomePage />, true),
  createCustomerRoute("products/:id", <ProductPage />),
  createCustomerRoute("cart", <CartPage />),
  createCustomerRoute("checkout", <CheckoutPage />),
  createCustomerRoute("user", <UserPage />),
  createCustomerRoute("orders/:id", <OrderDetailsPage />),
];

const adminRoutes: RouteObject[] = [
  createAdminRoute("admin", <AdminPage />),
  createAdminRoute("admin/activity-logs", <ActivityLogsPage />),
];

/**
 * main application router
 */
export const router = createBrowserRouter([
  ...authRoutes,
  {
    path: ROUTES.HOME,
    element: <Layout />,
    children: [...customerRoutes, ...adminRoutes],
  },
]);
