import { createRootRoute, ErrorComponent } from "@tanstack/react-router";
import { Layout } from "@/shared/components/Layout";
import { useOidc } from "@/oidc";

function LoginPage() {
  const oidc = useOidc();

  return (
    <div className="flex h-screen flex-col items-center justify-center bg-gray-50">
      <div className="w-full max-w-sm px-4">
        {/* Logo + wordmark */}
        <div className="mb-8 flex flex-col items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-primary shadow-sm">
            <svg
              width="22"
              height="22"
              fill="none"
              stroke="white"
              strokeWidth={2.25}
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 4C10.7 9.7 6 12.2 6 16.5a6 6 0 0 0 12 0c0-4.3-4.7-6.8-6-12.5Z"
              />
              <path strokeLinecap="round" d="M12 11v6" />
            </svg>
          </div>
          <div className="text-center">
            <h1 className="text-xl font-semibold tracking-tight text-gray-900">Nutrify</h1>
            <p className="mt-1 text-sm text-gray-500">
              Track your daily nutrition with precision
            </p>
          </div>
        </div>

        {/* Sign-in card */}
        <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
          <button
            onClick={() => oidc.login()}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-primary-dark focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            Continue with sign in
          </button>
        </div>
      </div>
    </div>
  );
}

function RootComponent() {
  const oidc = useOidc();

  if (!oidc.isUserLoggedIn) {
    return <LoginPage />;
  }

  return <Layout />;
}

export const Route = createRootRoute({
  component: RootComponent,
  errorComponent: ErrorComponent,
});
