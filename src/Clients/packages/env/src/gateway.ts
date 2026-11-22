import { createEnv } from "@t3-oss/env-nextjs";
import * as v from "valibot";

/** Validates API gateway and HTTP client environment variables. */
export const gatewayKeys = () =>
  createEnv({
    client: {
      NEXT_PUBLIC_GATEWAY_HTTPS: v.optional(v.pipe(v.string(), v.url())),
      NEXT_PUBLIC_GATEWAY_HTTP: v.optional(v.pipe(v.string(), v.url())),
      NEXT_PUBLIC_MAX_RETRIES: v.pipe(
        v.optional(v.string(), "5"),
        v.transform(Number),
        v.number(),
        v.integer(),
        v.gtValue(0),
      ),
    },
    runtimeEnv: {
      NEXT_PUBLIC_GATEWAY_HTTPS: process.env.NEXT_PUBLIC_GATEWAY_HTTPS,
      NEXT_PUBLIC_GATEWAY_HTTP: process.env.NEXT_PUBLIC_GATEWAY_HTTP,
      NEXT_PUBLIC_MAX_RETRIES: process.env.NEXT_PUBLIC_MAX_RETRIES,
    },
    skipValidation: !!process.env.SKIP_ENV_VALIDATION,
    emptyStringAsUndefined: true,
  });
