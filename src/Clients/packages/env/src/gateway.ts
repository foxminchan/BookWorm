import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

/** Validates API gateway and HTTP client environment variables. */
export const gatewayKeys = () =>
  createEnv({
    client: {
      NEXT_PUBLIC_GATEWAY_HTTPS: z.url().optional(),
      NEXT_PUBLIC_GATEWAY_HTTP: z.url().optional(),
      NEXT_PUBLIC_MAX_RETRIES: z.coerce.number().int().positive().default(5),
    },
    runtimeEnv: {
      NEXT_PUBLIC_GATEWAY_HTTPS: process.env.NEXT_PUBLIC_GATEWAY_HTTPS,
      NEXT_PUBLIC_GATEWAY_HTTP: process.env.NEXT_PUBLIC_GATEWAY_HTTP,
      NEXT_PUBLIC_MAX_RETRIES: process.env.NEXT_PUBLIC_MAX_RETRIES,
    },
    skipValidation: !!process.env.SKIP_ENV_VALIDATION,
    emptyStringAsUndefined: true,
  });
