import { oidcSpa } from "oidc-spa/react-spa";

export const { bootstrapOidc, useOidc, getOidc, OidcInitializationGate } = oidcSpa
  .withExpectedDecodedIdTokenShape({
    decodedIdTokenSchema: {
      parse: (data: unknown) => {
        const token = data as Record<string, unknown>;
        return {
          sub: token["sub"] as string,
          preferred_username: token["preferred_username"] as string,
          name: (token["name"] as string) || (token["preferred_username"] as string),
          email: token["email"] as string,
        };
      },
    },
  })
  .createUtils();

bootstrapOidc({
  implementation: "real",
  issuerUri: import.meta.env.VITE_OIDC_ISSUER || "http://localhost:8080/realms/nutrify",
  clientId: import.meta.env.VITE_OIDC_CLIENT_ID || "nutrify-spa",
  BASE_URL: import.meta.env.BASE_URL,
});
