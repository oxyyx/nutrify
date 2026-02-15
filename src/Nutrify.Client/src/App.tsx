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
          <div className="h-5 w-5 animate-spin rounded-full border-2 border-gray-200 border-t-primary" />
        </div>
      }
    >
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </OidcProvider>
  );
}
