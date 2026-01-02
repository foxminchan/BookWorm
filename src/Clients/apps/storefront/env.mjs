import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

// Debug logging - show ALL environment variables
console.log("🔍 All Environment Variables:");
console.log(JSON.stringify(process.env, null, 2));

export const env = createEnv({
  server: {
    KEYCLOAK_URL: z.url(),
    KEYCLOAK_REALM: z.string().min(1),
    KEYCLOAK_CLIENT_ID: z.string().min(1),
  },

  client: {
    NEXT_PUBLIC_GATEWAY_HTTPS: z.url().optional(),
    NEXT_PUBLIC_GATEWAY_HTTP: z.url().optional(),
  },

  runtimeEnv: {
    KEYCLOAK_URL: process.env.KEYCLOAK_URL,
    KEYCLOAK_REALM: process.env.KEYCLOAK_REALM,
    KEYCLOAK_CLIENT_ID: process.env.KEYCLOAK_CLIENT_ID,
    NEXT_PUBLIC_GATEWAY_HTTPS: process.env.GATEWAY_HTTPS,
    NEXT_PUBLIC_GATEWAY_HTTP: process.env.GATEWAY_HTTP,
  },

  skipValidation: !!process.env.SKIP_ENV_VALIDATION,

  emptyStringAsUndefined: true,
});
