import { NextRequest } from "next/server";

import { describe, expect, it } from "vitest";

import { proxy } from "@/proxy";

describe("storefront auth proxy", () => {
  it("redirects unauthenticated requests on protected routes to /login with callbackUrl", () => {
    const request = new NextRequest("http://localhost:3000/checkout");
    const response = proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe(
      "http://localhost:3000/login?callbackUrl=%2Fcheckout",
    );
  });

  it("allows authenticated requests with standard session cookie on protected routes", () => {
    const request = new NextRequest("http://localhost:3000/basket");
    request.cookies.set("better-auth.session_token", "token-123");
    const response = proxy(request);

    expect(response.status).toBe(200);
    expect(response.headers.get("location")).toBeNull();
  });

  it("allows authenticated requests with __Secure- prefixed session cookie on protected routes", () => {
    const request = new NextRequest("https://bookworm.example.com/account");
    request.cookies.set("__Secure-better-auth.session_token", "secure-token");
    const response = proxy(request);

    expect(response.status).toBe(200);
    expect(response.headers.get("location")).toBeNull();
  });

  it("redirects authenticated users away from auth routes", () => {
    const request = new NextRequest("https://bookworm.example.com/login");
    request.cookies.set("__Secure-better-auth.session_token", "secure-token");
    const response = proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe(
      "https://bookworm.example.com/",
    );
  });
});
