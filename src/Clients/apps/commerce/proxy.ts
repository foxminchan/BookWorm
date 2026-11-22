import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

import { getSessionCookie } from "better-auth/cookies";

import { resolvePublicOrigin } from "@workspace/storefront-shell/lib/proxy";

const protectedRoutes = ["/basket", "/checkout", "/account"];

export function proxy(request: NextRequest) {
  const path = request.nextUrl.pathname;
  const sessionToken = getSessionCookie(request);

  if (
    protectedRoutes.some((route) => path.startsWith(route)) &&
    !sessionToken
  ) {
    const loginUrl = new URL("/login", resolvePublicOrigin(request));
    loginUrl.searchParams.set("callbackUrl", path);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api (API routes, including /api/health)
     * - commerce-assets (microfrontend static/image assets)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    "/((?!api|commerce-assets|_next/static|_next/image|favicon.ico|.*\\..*|mockServiceWorker\\.js).*)",
  ],
};
