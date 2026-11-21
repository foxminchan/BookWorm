import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

/** Validates OpenTelemetry exporter and Next.js runtime environment variables. */
export const otelKeys = () =>
  createEnv({
    server: {
      OTEL_EXPORTER_OTLP_ENDPOINT: z.string().optional(),
      OTEL_EXPORTER_OTLP_HEADERS: z.string().optional(),
    },
    shared: {
      NEXT_RUNTIME: z.enum(["nodejs", "edge"]).optional(),
    },
    runtimeEnv: {
      OTEL_EXPORTER_OTLP_ENDPOINT: process.env.OTEL_EXPORTER_OTLP_ENDPOINT,
      OTEL_EXPORTER_OTLP_HEADERS: process.env.OTEL_EXPORTER_OTLP_HEADERS,
      NEXT_RUNTIME: process.env.NEXT_RUNTIME,
    },
    skipValidation: !!process.env.SKIP_ENV_VALIDATION,
    emptyStringAsUndefined: true,
  });
