import { useEffect } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "react-router-dom";
import { router } from "./app/router";
import { useAuthStore } from "./stores/authStore";
import { useCartStore } from "./stores/cartStore";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
      retry: 1,
    },
  },
});

function App() {
  const userId = useAuthStore((state) => state.userId);
  const switchUser = useCartStore((state) => state.switchUser);

  // Sync cart with the current user on app initialization
  useEffect(() => {
    switchUser(userId);
  }, [userId, switchUser]);

  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
}

export default App;
