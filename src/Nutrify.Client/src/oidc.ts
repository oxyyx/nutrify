import { createReactOidc } from "oidc-spa/react";

export const {
  OidcProvider,
  useOidc,
  getOidc,
} = createReactOidc({
  issuerUri: import.meta.env.VITE_OIDC_ISSUER || "http://localhost:8080/realms/nutrify",
  clientId: import.meta.env.VITE_OIDC_CLIENT_ID || "nutrify-spa",
  publicUrl: import.meta.env.BASE_URL,
  decodedIdTokenSchema: {
    parse: (token: Record<string, unknown>) => ({
      sub: token["sub"] as string,
      preferred_username: token["preferred_username"] as string,
      name: (token["name"] as string) || (token["preferred_username"] as string),
      email: token["email"] as string,
    }),
  },
});
