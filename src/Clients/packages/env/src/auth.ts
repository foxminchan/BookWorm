import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

/** Validates authentication and Microsoft Entra ID environment variables. */
export const authKeys = () =>
  createEnv({
    server: {
      BETTER_AUTH_SECRET: z.string().optional(),
      ENTRA_INSTANCE: z.url().optional(),
      ENTRA_TENANT_ID: z.string().min(1).optional(),
      ENTRA_CLIENT_ID: z.string().min(1).optional(),
    },
    client: {
      NEXT_PUBLIC_APP_URL: z.url().optional(),
    },
    runtimeEnv: {
      BETTER_AUTH_SECRET: process.env.BETTER_AUTH_SECRET,
      ENTRA_INSTANCE: process.env.ENTRA_INSTANCE,
      ENTRA_TENANT_ID: process.env.ENTRA_TENANT_ID,
      ENTRA_CLIENT_ID: process.env.ENTRA_CLIENT_ID,
      NEXT_PUBLIC_APP_URL: process.env.NEXT_PUBLIC_APP_URL,
    },
    skipValidation: !!process.env.SKIP_ENV_VALIDATION,
    emptyStringAsUndefined: true,
  });
