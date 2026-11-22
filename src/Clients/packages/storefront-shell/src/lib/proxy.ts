import type { NextRequest } from "next/server";

/**
 * Resolves the external browser origin honoring reverse-proxy headers
 * (`x-forwarded-proto`, `x-forwarded-host`, `host`) so Azure Container Apps
 * internal HTTP hops never downgrade redirect `Location` headers to `http://`.
 */
export function resolvePublicOrigin(request: NextRequest): string {
  const forwardedProto = request.headers
    .get("x-forwarded-proto")
    ?.split(",")[0]
    ?.trim();
  const forwardedHost = (
    request.headers.get("x-forwarded-host") ?? request.headers.get("host")
  )
    ?.split(",")[0]
    ?.trim();

  if (forwardedProto && forwardedHost) {
    return `${forwardedProto}://${forwardedHost}`;
  }

  return request.nextUrl.origin;
}
