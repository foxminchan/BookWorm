import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

import { getSessionCookie } from "better-auth/cookies";

import { resolvePublicOrigin } from "@workspace/storefront-shell/lib/proxy";

const authRoutes = ["/login", "/register"];

export function proxy(request: NextRequest) {
  const path = request.nextUrl.pathname;
  const sessionToken = getSessionCookie(request);

  if (authRoutes.some((route) => path.startsWith(route)) && sessionToken) {
    return NextResponse.redirect(new URL("/", resolvePublicOrigin(request)));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api (API routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    "/((?!api|_next/static|_next/image|favicon.ico|.*\\..*|mockServiceWorker\\.js).*)",
  ],
};
