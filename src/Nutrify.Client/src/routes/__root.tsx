import { createRootRoute, ErrorComponent } from "@tanstack/react-router";
import { Layout } from "@/shared/components/Layout";
import { useOidc } from "@/oidc";

function RootComponent() {
  const oidc = useOidc();

  if (!oidc.isUserLoggedIn) {
    return (
      <div className="flex h-screen flex-col items-center justify-center gap-4 bg-gray-50">
        <h1 className="text-3xl font-bold text-primary">Nutrify</h1>
        <p className="text-gray-600">Track your daily nutrition intake</p>
        <button
          onClick={() => oidc.login()}
          className="rounded-md bg-primary px-6 py-2 text-white hover:bg-primary-dark"
        >
          Sign In
        </button>
      </div>
    );
  }

  return <Layout />;
}

export const Route = createRootRoute({
  component: RootComponent,
  errorComponent: ErrorComponent,
});
