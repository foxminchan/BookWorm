import { HttpResponse } from "msw";

import { HttpHeaders } from "@workspace/api-client/headers";

const DEFAULT_ALLOW_HEADERS = [
  HttpHeaders.Authorization,
  HttpHeaders.CacheControl,
  HttpHeaders.ContentType,
  HttpHeaders.IfMatch,
  HttpHeaders.IfNoneMatch,
  HttpHeaders.RequestId,
].join(", ");

const DEFAULT_EXPOSE_HEADERS = [
  HttpHeaders.ETag,
  HttpHeaders.PaginationCount,
  HttpHeaders.Link,
].join(", ");

export function createCorsHeaders(
  request: Request,
  extraHeaders?: Record<string, string>,
): Record<string, string> {
  const origin = request.headers.get("Origin") || "*";
  const requestedHeaders = request.headers.get(
    "Access-Control-Request-Headers",
  );

  return {
    "Access-Control-Allow-Origin": origin,
    "Access-Control-Allow-Credentials": "true",
    "Access-Control-Allow-Methods":
      "GET, POST, PUT, PATCH, DELETE, OPTIONS, QUERY",
    "Access-Control-Allow-Headers": requestedHeaders || DEFAULT_ALLOW_HEADERS,
    "Access-Control-Expose-Headers": DEFAULT_EXPOSE_HEADERS,
    ...extraHeaders,
  };
}

export function createPreflightResponse(request: Request) {
  return new HttpResponse(null, {
    status: 204,
    headers: createCorsHeaders(request),
  });
}

export function withCorsHeaders(
  request: Request,
  response: Response,
): Response {
  for (const [key, value] of Object.entries(createCorsHeaders(request))) {
    response.headers.set(key, value);
  }
  return response;
}
