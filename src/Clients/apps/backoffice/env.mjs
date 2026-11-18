import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

export const env = createEnv({
  server: {
    BETTER_AUTH_SECRET: z.string().optional(),
    ENTRA_INSTANCE: z.url().optional(),
    ENTRA_TENANT_ID: z.string().min(1).optional(),
    ENTRA_CLIENT_ID: z.string().min(1).optional(),
    NEXT_PUBLIC_APP_URL: z.url().optional(),
  },

  client: {
    NEXT_PUBLIC_APP_URL: z.url().optional(),
    NEXT_PUBLIC_GATEWAY_HTTPS: z.url().optional(),
    NEXT_PUBLIC_GATEWAY_HTTP: z.url().optional(),
  },

  runtimeEnv: {
    BETTER_AUTH_SECRET: process.env.BETTER_AUTH_SECRET,
    ENTRA_INSTANCE: process.env.ENTRA_INSTANCE,
    ENTRA_TENANT_ID: process.env.ENTRA_TENANT_ID,
    ENTRA_CLIENT_ID: process.env.ENTRA_CLIENT_ID,
    NEXT_PUBLIC_APP_URL: process.env.NEXT_PUBLIC_APP_URL,
    NEXT_PUBLIC_GATEWAY_HTTPS: process.env.NEXT_PUBLIC_GATEWAY_HTTPS,
    NEXT_PUBLIC_GATEWAY_HTTP: process.env.NEXT_PUBLIC_GATEWAY_HTTP,
  },

  skipValidation: !!process.env.SKIP_ENV_VALIDATION,
  emptyStringAsUndefined: true,
});
