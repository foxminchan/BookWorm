import { NextRequest } from "next/server";

import { describe, expect, it } from "vitest";

import { proxy } from "@/proxy";

describe("commerce auth proxy", () => {
  it("redirects unauthenticated requests on protected routes to /login with callbackUrl", () => {
    const request = new NextRequest("http://localhost:3002/checkout");
    const response = proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe(
      "http://localhost:3002/login?callbackUrl=%2Fcheckout",
    );
  });

  it("honors x-forwarded-proto and x-forwarded-host headers when building /login redirect", () => {
    const request = new NextRequest("http://commerce:3000/account/orders", {
      headers: {
        "x-forwarded-proto": "https",
        "x-forwarded-host": "gateway.aca.example.com",
      },
    });
    const response = proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe(
      "https://gateway.aca.example.com/login?callbackUrl=%2Faccount%2Forders",
    );
  });

  it("allows authenticated requests with standard session cookie on protected routes", () => {
    const request = new NextRequest("http://localhost:3002/basket");
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
});
