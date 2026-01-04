/**
 * Generate a W3C Trace Context compliant trace ID
 * @returns A trace ID in the format "00-{trace-id}-{parent-id}-01"
 * @see https://www.w3.org/TR/trace-context/
 */
export const generateTraceId = (): string => {
  const traceId = crypto.randomUUID().replace(/-/g, "");
  const parentId = crypto.randomUUID().substring(0, 16).replace(/-/g, "");
  return `00-${traceId}-${parentId}-01`;
};
