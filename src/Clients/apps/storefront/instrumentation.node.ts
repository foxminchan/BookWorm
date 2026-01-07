import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-proto";
import { NodeSDK } from "@opentelemetry/sdk-node";
import { SimpleSpanProcessor } from "@opentelemetry/sdk-trace-node";

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

const otlpEndpoint = `${process.env.OTEL_EXPORTER_OTLP_ENDPOINT}/v1/traces`;

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
