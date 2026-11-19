import { HttpResponse } from "msw";

import { generateTraceId } from "@workspace/utils/trace";

/**
 * Create an RFC 9110 Problem Details response for validation errors.
 *
 * @see https://tools.ietf.org/html/rfc9110#section-15.5.1
 */
export function createValidationErrorResponse(
  errors: Record<string, string[]>,
  status = 400,
) {
  return HttpResponse.json(
    {
      type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      title: "One or more validation errors occurred.",
      status,
      errors,
      traceId: generateTraceId(),
    },
    { status },
  );
}

/** Shorthand 404 with a plain-text body. */
export function createNotFoundResponse(entity: string) {
  return new HttpResponse(`${entity} not found`, { status: 404 });
}

/**
 * Create an RFC 9110 Problem Details response for optimistic concurrency conflicts (412 Precondition Failed).
 */
export function createPreconditionFailedResponse(
  detail = "The resource was modified by another request. Please retry with the latest version.",
) {
  return HttpResponse.json(
    {
      type: "https://tools.ietf.org/html/rfc9110#section-15.5.13",
      title: detail,
      status: 412,
      traceId: generateTraceId(),
    },
    { status: 412 },
  );
}
