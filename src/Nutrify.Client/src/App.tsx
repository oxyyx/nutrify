import { QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "@tanstack/react-router";
import { OidcProvider } from "@/oidc";
import { queryClient } from "@/shared/lib/query-client";
import { router } from "@/router";

export function App() {
  return (
    <OidcProvider
      fallback={
        <div className="flex h-screen items-center justify-center">
          <div className="text-lg text-gray-500">Loading authentication...</div>
        </div>
      }
    >
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </OidcProvider>
  );
}
