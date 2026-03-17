import { betterAuth } from "better-auth";
import { genericOAuth, keycloak } from "better-auth/plugins";

import { env } from "@/env.mjs";

// Defense-in-depth: this check runs even when SKIP_ENV_VALIDATION is set,
// because auth secrets must never be optional in any environment.
if (!process.env.BETTER_AUTH_SECRET) {
  throw new Error(
    "BETTER_AUTH_SECRET environment variable is required and must not be empty.",
  );
}

export const auth = betterAuth({
  baseURL: env.NEXT_PUBLIC_APP_URL || "http://localhost:3001",
  secret: env.BETTER_AUTH_SECRET,
  trustedOrigins: [
    env.KEYCLOAK_URL,
    env.NEXT_PUBLIC_GATEWAY_HTTP || "",
    env.NEXT_PUBLIC_APP_URL || "",
    ...(process.env.NODE_ENV === "development" ? ["http://localhost:*"] : []),
  ].filter((origin): origin is string => Boolean(origin)),
  emailAndPassword: {
    enabled: false,
  },
  plugins: [
    genericOAuth({
      config: [
        keycloak({
          clientId: env.KEYCLOAK_CLIENT_ID!,
          clientSecret: "",
          issuer: `${env.KEYCLOAK_URL}/realms/${env.KEYCLOAK_REALM}`,
          pkce: true,
          scopes: [
            "openid",
            "profile",
            "email",
            "catalog_read",
            "catalog_write",
            "ordering_read",
            "ordering_write",
            "rating_read",
            "rating_write",
          ],
        }),
      ],
    }),
  ],
});
