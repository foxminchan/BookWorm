import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

export const env = createEnv({
  server: {
    BETTER_AUTH_SECRET: z.string().optional(),
    KEYCLOAK_URL: z.url().optional(),
    KEYCLOAK_REALM: z.string().optional(),
    KEYCLOAK_CLIENT_ID: z.string().optional(),
    NEXT_PUBLIC_APP_URL: z.url().optional(),
    NEXT_PUBLIC_COPILOT_AGENT_NAME: z.string().default("chat-workflow"),
  },

  client: {
    NEXT_PUBLIC_GATEWAY_HTTPS: z.url().optional(),
    NEXT_PUBLIC_GATEWAY_HTTP: z.url().optional(),
    NEXT_PUBLIC_COPILOT_ENABLED: z
      .string()
      .transform((val) => val === "true")
      .default("false"),
  },

  runtimeEnv: {
    BETTER_AUTH_SECRET: process.env.BETTER_AUTH_SECRET,
    KEYCLOAK_URL: process.env.KEYCLOAK_URL,
    KEYCLOAK_REALM: process.env.KEYCLOAK_REALM,
    KEYCLOAK_CLIENT_ID: process.env.KEYCLOAK_CLIENT_ID,
    NEXT_PUBLIC_APP_URL: process.env.NEXT_PUBLIC_APP_URL,
    NEXT_PUBLIC_GATEWAY_HTTPS: process.env.NEXT_PUBLIC_GATEWAY_HTTPS,
    NEXT_PUBLIC_GATEWAY_HTTP: process.env.NEXT_PUBLIC_GATEWAY_HTTP,
    NEXT_PUBLIC_COPILOT_ENABLED: process.env.NEXT_PUBLIC_COPILOT_ENABLED,
    NEXT_PUBLIC_COPILOT_AGENT_NAME: process.env.NEXT_PUBLIC_COPILOT_AGENT_NAME,
  },

  skipValidation: !!process.env.SKIP_ENV_VALIDATION,

  emptyStringAsUndefined: true,
});
