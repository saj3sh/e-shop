import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

interface AuthState {
  userId: string | null;
  role: string | null;
  isAuthenticated: boolean;
  accessToken: string | null;
  setAuth: (userId: string, role: string, accessToken: string) => void;
  clearAuth: () => void;
  setAccessToken: (token: string | null) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      userId: null,
      role: null,
      isAuthenticated: false,
      accessToken: null,
      setAuth: (userId, role, accessToken) =>
        set({ userId, role, isAuthenticated: true, accessToken }),
      clearAuth: () =>
        set({
          userId: null,
          role: null,
          isAuthenticated: false,
          accessToken: null,
        }),
      setAccessToken: (token) => set({ accessToken: token }),
    }),
    {
      name: "auth-storage",
    }
  )
);
