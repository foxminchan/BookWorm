import { betterAuth } from "better-auth";
import { genericOAuth, microsoftEntraId } from "better-auth/plugins";

import { env } from "@/env.mjs";

const isDev = process.env.NODE_ENV === "development";
const entraInstance = (
  env.ENTRA_INSTANCE || "https://login.microsoftonline.com"
).replace(/\/+$/, "");
const tenantId = env.ENTRA_TENANT_ID || "00000000-0000-0000-0000-000000000000";

if (!env.ENTRA_TENANT_ID || !env.ENTRA_CLIENT_ID) {
  console.warn(
    "Microsoft Entra ID tenant or client ID is not configured. Authentication will not work without them.",
  );
}

const apiScopes = [
  "catalog_read",
  "catalog_write",
  "ordering_read",
  "ordering_write",
  "rating_read",
  "rating_write",
].map((scope) =>
  env.ENTRA_CLIENT_ID ? `api://${env.ENTRA_CLIENT_ID}/${scope}` : scope,
);

export const auth = betterAuth({
  baseURL: env.NEXT_PUBLIC_APP_URL || "http://localhost:3001",
  secret: env.BETTER_AUTH_SECRET,
  trustedOrigins: [
    entraInstance,
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
        {
          ...microsoftEntraId({
            clientId: env.ENTRA_CLIENT_ID ?? "",
            clientSecret: "",
            tenantId,
            pkce: true,
            scopes: ["openid", "profile", "email", ...apiScopes],
          }),
          discoveryUrl: `${entraInstance}/${tenantId}/v2.0/.well-known/openid-configuration`,
          authorizationUrl: `${entraInstance}/${tenantId}/oauth2/v2.0/authorize`,
          tokenUrl: `${entraInstance}/${tenantId}/oauth2/v2.0/token`,
        },
      ],
    }),
  ],
});
