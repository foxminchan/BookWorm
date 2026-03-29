import { betterAuth } from "better-auth";
import { genericOAuth, keycloak } from "better-auth/plugins";

import { env } from "@/env.mjs";

const isDev = process.env.NODE_ENV === "development";
const keycloakUrl = isDev
  ? env.KEYCLOAK_HTTP || env.KEYCLOAK_HTTPS || ""
  : env.KEYCLOAK_HTTPS || env.KEYCLOAK_HTTP || "";

if (!keycloakUrl) {
  console.warn(
    "No Keycloak URL provided. Authentication will not work without it.",
  );
}

export const auth = betterAuth({
  baseURL: env.NEXT_PUBLIC_APP_URL || "http://localhost:3001",
  secret: env.BETTER_AUTH_SECRET,
  trustedOrigins: [
    keycloakUrl,
    env.NEXT_PUBLIC_GATEWAY_HTTP || "",
    env.NEXT_PUBLIC_APP_URL || "",
    ...(isDev ? ["http://localhost:*"] : []),
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
          issuer: `${keycloakUrl}/realms/${env.KEYCLOAK_REALM}`,
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
