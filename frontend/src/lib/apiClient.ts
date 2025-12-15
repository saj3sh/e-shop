import axios, { AxiosError } from "axios";

const API_BASE =
  import.meta.env.VITE_API_BASE || "http://localhost:5001/api/v1";

export const apiClient = axios.create({
  baseURL: API_BASE,
  withCredentials: true,
});

export const handleLogout = async (callApi = true) => {
  if (callApi) {
    try {
      await apiClient.post("/auth/logout");
    } catch (error) {
      console.error("logout api call failed", error);
    }
  }

  const { useAuthStore } = await import("../stores/authStore");
  useAuthStore.getState().clearAuth();

  window.location.href = "/login";
};

// where 401 should not trigger token refresh retry
const NO_RETRY_ENDPOINTS = [
  "/auth/login", // 401 = authentication failure (expected)
  "/auth/refresh", // 401 = refresh token expired (expected, prevent retry loop)
];

// request interceptor to add access token
apiClient.interceptors.request.use(async (config) => {
  const { useAuthStore } = await import("../stores/authStore");
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Token refresh queue to prevent multiple concurrent refresh requests
let isRefreshing = false;
let refreshPromise: Promise<string> | null = null;

// response interceptor to handle 401 and refresh token
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config;

    // Don't retry for endpoints where 401 is expected
    const shouldNotRetry = NO_RETRY_ENDPOINTS.some((endpoint) =>
      originalRequest?.url?.includes(endpoint)
    );

    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !shouldNotRetry
    ) {
      originalRequest._retry = true;

      try {
        // If already refreshing, wait for the existing refresh to complete
        if (!isRefreshing) {
          isRefreshing = true;
          refreshPromise = axios
            .post(`${API_BASE}/auth/refresh`, {}, { withCredentials: true })
            .then(async (response) => {
              const newToken = response.data.accessToken;
              const { useAuthStore } = await import("../stores/authStore");
              useAuthStore.getState().setAccessToken(newToken);
              return newToken;
            })
            .finally(() => {
              isRefreshing = false;
              refreshPromise = null;
            });
        }

        const newToken = await refreshPromise;
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        await handleLogout(false);
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

declare module "axios" {
  export interface AxiosRequestConfig {
    _retry?: boolean;
  }
}
