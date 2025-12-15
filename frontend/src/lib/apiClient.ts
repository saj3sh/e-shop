import axios, { AxiosError } from "axios";

const API_BASE =
  import.meta.env.VITE_API_BASE || "http://localhost:5001/api/v1";

export const apiClient = axios.create({
  baseURL: API_BASE,
  withCredentials: true,
});

let accessToken: string | null = null;

export const setAccessToken = (token: string | null) => {
  accessToken = token;
};

export const getAccessToken = () => accessToken;

// where 401 should not trigger token refresh retry
const NO_RETRY_ENDPOINTS = [
  "/auth/login", // 401 = authentication failure (expected)
  "/auth/refresh", // 401 = refresh token expired (expected, prevent retry loop)
];

// request interceptor to add access token
apiClient.interceptors.request.use((config) => {
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

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
        const response = await axios.post(
          `${API_BASE}/auth/refresh`,
          {},
          { withCredentials: true }
        );

        const { accessToken: newToken } = response.data;
        setAccessToken(newToken);

        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        setAccessToken(null);
        window.location.href = "/login";
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
