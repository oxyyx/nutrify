export interface RuntimeConfig {
  oidcIssuer: string;
  oidcClientId: string;
  version: string;
}

// Build-time defaults. Used for local `vite dev` and as a safety net if the
// runtime config endpoint is unreachable.
const fallback: RuntimeConfig = {
  oidcIssuer:
    import.meta.env.VITE_OIDC_ISSUER || "http://localhost:8080/realms/nutrify",
  oidcClientId: import.meta.env.VITE_OIDC_CLIENT_ID || "nutrify-spa",
  version: "dev",
};

let config: RuntimeConfig = fallback;

/**
 * Fetches runtime configuration from the API (served from the same container in
 * production, proxied via `/api` during local dev). Falls back to build-time
 * defaults if the endpoint is unavailable so the app still boots.
 */
export async function loadConfig(): Promise<void> {
  try {
    const response = await fetch("/api/config", { cache: "no-store" });
    if (response.ok) {
      const data = (await response.json()) as Partial<RuntimeConfig>;
      config = {
        oidcIssuer: data.oidcIssuer || fallback.oidcIssuer,
        oidcClientId: data.oidcClientId || fallback.oidcClientId,
        version: data.version || fallback.version,
      };
    }
  } catch {
    // Keep the build-time defaults.
  }
}

export function getConfig(): RuntimeConfig {
  return config;
}
