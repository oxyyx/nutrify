import { oidcSpa } from "oidc-spa/react-spa";
import { getConfig } from "./config";

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

/**
 * Boots OIDC using runtime configuration. Must be called after `loadConfig()`
 * and before the app renders.
 */
export function initOidc() {
  const { oidcIssuer, oidcClientId } = getConfig();
  bootstrapOidc({
    implementation: "real",
    issuerUri: oidcIssuer,
    clientId: oidcClientId,
    BASE_URL: import.meta.env.BASE_URL,
  });
}
