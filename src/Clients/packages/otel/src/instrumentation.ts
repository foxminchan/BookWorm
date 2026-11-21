/** Next.js instrumentation hook that initializes OpenTelemetry on Node.js runtime. */
export async function register() {
  if (process.env.NEXT_RUNTIME === "nodejs") {
    const { initializeOpenTelemetry } = await import("./index");
    initializeOpenTelemetry();
  }
}
