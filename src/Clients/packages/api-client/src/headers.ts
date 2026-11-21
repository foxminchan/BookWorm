/** Standard HTTP header names used across BookWorm API clients and mocks. */
export const HttpHeaders = {
  Accept: "Accept",
  Authorization: "Authorization",
  CacheControl: "Cache-Control",
  ContentType: "Content-Type",
  ETag: "ETag",
  IfMatch: "If-Match",
  IfNoneMatch: "If-None-Match",
  Link: "Link",
  Location: "Location",
  PaginationCount: "Pagination-Count",
  RequestId: "x-request-id",
} as const;
