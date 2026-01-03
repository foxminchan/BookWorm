import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const protectedRoutes = ["/basket", "/checkout", "/account"];
const authRoutes = ["/login", "/register"];

export async function proxy(request: NextRequest) {
  const path = request.nextUrl.pathname;
  const sessionToken = request.cookies.get("better-auth.session_token")?.value;

  if (authRoutes.some((route) => path.startsWith(route)) && sessionToken) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  if (
    protectedRoutes.some((route) => path.startsWith(route)) &&
    !sessionToken
  ) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("callbackUrl", path);
    return NextResponse.redirect(loginUrl);
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
