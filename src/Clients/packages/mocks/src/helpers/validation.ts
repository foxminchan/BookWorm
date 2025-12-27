import { generateTraceId } from "./trace";

/**
 * Convert Zod validation errors to FluentValidation-compatible error response
 * @param error - Zod error object containing validation issues
 * @returns A FluentValidation-formatted error response with RFC9110 reference
 * @see https://tools.ietf.org/html/rfc9110#section-15.5.1
 */
export function formatValidationErrors(error: {
  issues: Array<{ path: Array<string | number>; message: string }>;
}): {
  type: string;
  title: string;
  status: number;
  errors: Record<string, string[]>;
  traceId: string;
} {
  const errors: Record<string, string[]> = {};

  for (const issue of error.issues) {
    const field = issue.path[0]?.toString() || "Unknown";
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
