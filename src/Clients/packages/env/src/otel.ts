import { createEnv } from "@t3-oss/env-nextjs";
import * as v from "valibot";

/** Validates OpenTelemetry exporter and Next.js runtime environment variables. */
export const otelKeys = () =>
  createEnv({
    server: {
      OTEL_EXPORTER_OTLP_ENDPOINT: v.optional(v.string()),
      OTEL_EXPORTER_OTLP_HEADERS: v.optional(v.string()),
    },
    shared: {
      NEXT_RUNTIME: v.optional(v.picklist(["nodejs", "edge"])),
    },
    runtimeEnv: {
      OTEL_EXPORTER_OTLP_ENDPOINT: process.env.OTEL_EXPORTER_OTLP_ENDPOINT,
      OTEL_EXPORTER_OTLP_HEADERS: process.env.OTEL_EXPORTER_OTLP_HEADERS,
      NEXT_RUNTIME: process.env.NEXT_RUNTIME,
    },
    skipValidation: !!process.env.SKIP_ENV_VALIDATION,
    emptyStringAsUndefined: true,
  });
