import { betterAuth } from "better-auth";
import {
  genericOAuth,
  microsoftEntraId,
} from "better-auth/plugins/generic-oauth";

export interface EntraAuthEnv {
  BETTER_AUTH_SECRET?: string;
  ENTRA_INSTANCE?: string;
  ENTRA_TENANT_ID?: string;
  ENTRA_CLIENT_ID?: string;
  NEXT_PUBLIC_APP_URL?: string;
  NEXT_PUBLIC_GATEWAY_HTTPS?: string;
  NEXT_PUBLIC_GATEWAY_HTTP?: string;
}

export interface CreateEntraAuthOptions {
  env: EntraAuthEnv;
  defaultBaseUrl: string;
  scopes: readonly string[];
  cookiePrefix?: string;
}

function stripTrailingSlashes(value: string): string {
  let end = value.length;
  while (end > 0 && value[end - 1] === "/") {
    end--;
  }
  return end === value.length ? value : value.slice(0, end);
}

/** Creates a Better Auth instance configured with Microsoft Entra ID OAuth. */
export function createEntraAuth({
  env,
  defaultBaseUrl,
  scopes,
  cookiePrefix,
}: CreateEntraAuthOptions) {
  const isDev = process.env.NODE_ENV === "development";
  const entraInstance = stripTrailingSlashes(
    env.ENTRA_INSTANCE || "https://login.microsoftonline.com",
  );
  const tenantId =
    env.ENTRA_TENANT_ID || "00000000-0000-0000-0000-000000000000";

  if (!env.ENTRA_TENANT_ID || !env.ENTRA_CLIENT_ID) {
    console.warn(
      "Microsoft Entra ID tenant or client ID is not configured. Authentication will not work without them.",
    );
  }

  const apiScopes = scopes.map((scope) =>
    env.ENTRA_CLIENT_ID ? `api://${env.ENTRA_CLIENT_ID}/${scope}` : scope,
  );

  // Route server-side OIDC discovery and token exchange through YARP (/entra)
  // while keeping browser authorization redirects pointed directly at Entra ID.
  const gatewayUrl = stripTrailingSlashes(
    (isDev ? env.NEXT_PUBLIC_GATEWAY_HTTP : env.NEXT_PUBLIC_GATEWAY_HTTPS) ||
      env.NEXT_PUBLIC_GATEWAY_HTTP ||
      env.NEXT_PUBLIC_GATEWAY_HTTPS ||
      "",
  );
  const entraProxyBase = gatewayUrl ? `${gatewayUrl}/entra` : entraInstance;

  return betterAuth({
    baseURL: env.NEXT_PUBLIC_APP_URL || defaultBaseUrl,
    secret: env.BETTER_AUTH_SECRET,
    trustedOrigins: [
      entraInstance,
      env.NEXT_PUBLIC_GATEWAY_HTTPS || "",
      env.NEXT_PUBLIC_GATEWAY_HTTP || "",
      env.NEXT_PUBLIC_APP_URL || "",
      ...(isDev ? ["http://localhost:*", "https://localhost:*"] : []),
    ].filter((origin): origin is string => Boolean(origin)),
    emailAndPassword: {
      enabled: false,
    },
    account: {
      encryptOAuthTokens: true,
    },
    advanced: {
      ...(cookiePrefix ? { cookiePrefix } : {}),
      ipAddress: {
        ipAddressHeaders: ["x-forwarded-for", "x-real-ip"],
      },
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
            discoveryUrl: `${entraProxyBase}/${tenantId}/v2.0/.well-known/openid-configuration`,
            authorizationUrl: `${entraInstance}/${tenantId}/oauth2/v2.0/authorize`,
            tokenUrl: `${entraProxyBase}/${tenantId}/oauth2/v2.0/token`,
          },
        ],
      }),
    ],
  });
}
