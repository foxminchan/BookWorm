import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-grpc";
import { BatchSpanProcessor } from "@opentelemetry/sdk-trace-node";
import { registerOTel } from "@vercel/otel";

export async function register() {
  if (process.env.NEXT_RUNTIME === "nodejs") {
    const spanProcessors = [new BatchSpanProcessor(new OTLPTraceExporter())];
    registerOTel({ spanProcessors: spanProcessors });
  }
}
