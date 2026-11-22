import { NextRequest } from "next/server";

import { describe, expect, it } from "vitest";

import { proxy } from "@/proxy";

describe("storefront auth proxy", () => {
  it("redirects authenticated users away from auth routes", () => {
    const request = new NextRequest("https://bookworm.example.com/login");
    request.cookies.set("__Secure-better-auth.session_token", "secure-token");
    const response = proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe(
      "https://bookworm.example.com/",
    );
  });

  it("honors x-forwarded-proto and x-forwarded-host headers on auth redirect", () => {
    const request = new NextRequest("http://storefront:3000/login", {
      headers: {
        "x-forwarded-proto": "https",
        "x-forwarded-host": "gateway.aca.example.com",
      },
    });
    request.cookies.set("__Secure-better-auth.session_token", "secure-token");
    const response = proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toBe(
      "https://gateway.aca.example.com/",
    );
  });

  it("allows unauthenticated users on auth and discovery routes", () => {
    const loginRequest = new NextRequest("https://bookworm.example.com/login");
    const shopRequest = new NextRequest("https://bookworm.example.com/shop");

    expect(proxy(loginRequest).status).toBe(200);
    expect(proxy(shopRequest).status).toBe(200);
  });
});
