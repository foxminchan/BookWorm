import { generateTraceId } from "./trace";

/**
 * Convert Valibot validation errors to FluentValidation-compatible error response
 * @param error - Validation error object containing issues
 * @returns A FluentValidation-formatted error response with RFC9110 reference
 * @see https://tools.ietf.org/html/rfc9110#section-15.5.1
 */
export function formatValidationErrors(error: {
  issues: ReadonlyArray<{
    path?: ReadonlyArray<PropertyKey | { key: unknown }>;
    message: string;
  }>;
}): {
  type: string;
  title: string;
  status: number;
  errors: Record<string, string[]>;
  traceId: string;
} {
  const errors: Record<string, string[]> = {};

  for (const issue of error.issues) {
    const firstSegment = issue.path?.[0];
    const rawKey =
      firstSegment !== null &&
      typeof firstSegment === "object" &&
      "key" in firstSegment
        ? firstSegment.key
        : firstSegment;
    const field = (rawKey != null ? String(rawKey) : "") || "Unknown";
    const fieldName = field.charAt(0).toUpperCase() + field.slice(1);
    errors[fieldName] = [issue.message];
  }

  return {
    type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    title: "One or more validation errors occurred.",
    status: 400,
    errors,
    traceId: generateTraceId(),
  };
}
