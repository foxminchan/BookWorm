import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

const isDev = process.env.NODE_ENV === "development";

export const env = createEnv({
  server: {
    KEYCLOAK_URL: z.url().optional(),
    KEYCLOAK_REALM: z.string().optional(),
    KEYCLOAK_CLIENT_ID: z.string().optional(),
  },

  client: {
    NEXT_PUBLIC_GATEWAY_HTTPS: isDev ? z.url().optional() : z.url(),
    NEXT_PUBLIC_GATEWAY_HTTP: isDev ? z.url().optional() : z.url(),
  },

  runtimeEnv: {
    KEYCLOAK_URL: process.env.KEYCLOAK_URL,
    KEYCLOAK_REALM: process.env.KEYCLOAK_REALM,
    KEYCLOAK_CLIENT_ID: process.env.KEYCLOAK_CLIENT_ID,
    NEXT_PUBLIC_GATEWAY_HTTPS: process.env.NEXT_PUBLIC_GATEWAY_HTTPS,
    NEXT_PUBLIC_GATEWAY_HTTP: process.env.NEXT_PUBLIC_GATEWAY_HTTP,
  },

  skipValidation: !!process.env.SKIP_ENV_VALIDATION,

  emptyStringAsUndefined: true,
});
