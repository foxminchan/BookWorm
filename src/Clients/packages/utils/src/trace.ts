/**
 * Generate a W3C Trace Context compliant trace ID
 * @returns A trace ID in the format "00-{trace-id}-{parent-id}-01"
 * @see https://www.w3.org/TR/trace-context/
 */
export const generateTraceId = (): string => {
  const traceId = crypto.randomUUID().replaceAll("-", "");
  const parentId = crypto.randomUUID().substring(0, 16).replaceAll("-", "");
  return `00-${traceId}-${parentId}-01`;
};
