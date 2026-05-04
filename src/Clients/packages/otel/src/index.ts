import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-proto";
import { NodeSDK } from "@opentelemetry/sdk-node";
import { SimpleSpanProcessor } from "@opentelemetry/sdk-trace-node";

/**
 * Parse OTLP headers from a comma-separated string format.
 * Example: "key1=value1,key2=value2"
 */
function parseOtlpHeaders(headerString: string): Record<string, string> {
  return headerString
    .split(",")
    .map((pair) => pair.trim())
    .reduce(
      (headers, pair) => {
        const [key, value] = pair.split("=");
        if (key && value) {
          headers[key.trim()] = value.trim();
        }
        return headers;
      },
      {} as Record<string, string>,
    );
}

/**
 * Initialize OpenTelemetry SDK with OTLP exporter.
 * Reads configuration from environment variables:
 * - OTEL_EXPORTER_OTLP_ENDPOINT: The OTLP collector endpoint
 * - OTEL_EXPORTER_OTLP_HEADERS: Optional comma-separated headers (e.g., "Authorization=Bearer token,X-Custom=value")
 */
export function initializeOpenTelemetry(): void {
  const otlpBaseEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT;

  if (otlpBaseEndpoint) {
    const otlpEndpoint = `${otlpBaseEndpoint}/v1/traces`;

    const additionalHeaders = process.env.OTEL_EXPORTER_OTLP_HEADERS
      ? parseOtlpHeaders(process.env.OTEL_EXPORTER_OTLP_HEADERS)
      : {};

    const exporterHeaders = {
      "Content-Type": "application/x-protobuf",
      ...additionalHeaders,
    };

    const exporter = new OTLPTraceExporter({
      url: otlpEndpoint,
      headers: exporterHeaders,
    });

    const sdk = new NodeSDK({
      spanProcessors: [new SimpleSpanProcessor(exporter)],
    });

    sdk.start();
  } else {
    console.warn(
      "OTEL_EXPORTER_OTLP_ENDPOINT is not set. Skipping OpenTelemetry instrumentation.",
    );
  }
}
