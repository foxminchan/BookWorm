import { betterAuth } from "better-auth";
import { genericOAuth, keycloak } from "better-auth/plugins";
import { env } from "@/env.mjs";

export const auth = betterAuth({
  trustedOrigins: [
    env.KEYCLOAK_URL,
    env.NEXT_PUBLIC_GATEWAY_HTTP || "",
    ...(process.env.NODE_ENV === "development" ? ["http://localhost:*"] : []),
  ].filter(Boolean),
  emailAndPassword: {
    enabled: false,
  },
  plugins: [
    genericOAuth({
      config: [
        keycloak({
          clientId: env.KEYCLOAK_CLIENT_ID,
          clientSecret: "",
          issuer: `${env.KEYCLOAK_URL}/realms/${env.KEYCLOAK_REALM}`,
          pkce: true,
          scopes: [
            "openid",
            "profile",
            "email",
            "catalog_read",
            "basket_read",
            "basket_write",
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
