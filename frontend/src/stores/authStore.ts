import { create } from "zustand";
import { persist } from "zustand/middleware";

interface AuthState {
  userId: string | null;
  role: string | null;
  isAuthenticated: boolean;
  setAuth: (userId: string, role: string) => void;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      userId: null,
      role: null,
      isAuthenticated: false,
      setAuth: (userId, role) => set({ userId, role, isAuthenticated: true }),
      clearAuth: () =>
        set({ userId: null, role: null, isAuthenticated: false }),
    }),
    {
      name: "auth-storage",
    }
  )
);
