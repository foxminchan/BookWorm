import { createEnv } from "@t3-oss/env-nextjs";
import * as v from "valibot";

/** Validates authentication and Microsoft Entra ID environment variables. */
export const authKeys = () =>
  createEnv({
    server: {
      BETTER_AUTH_SECRET: v.optional(v.string()),
      ENTRA_INSTANCE: v.optional(v.pipe(v.string(), v.url())),
      ENTRA_TENANT_ID: v.optional(v.pipe(v.string(), v.minLength(1))),
      ENTRA_CLIENT_ID: v.optional(v.pipe(v.string(), v.minLength(1))),
    },
    client: {
      NEXT_PUBLIC_APP_URL: v.optional(v.pipe(v.string(), v.url())),
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
