import { validateRouting } from "@vercel/microfrontends/next/testing";
import path from "node:path";
import { describe, expect, it } from "vitest";

const configPath = path.resolve(
  import.meta.dirname,
  "../microfrontends.config.json",
);

describe("microfrontends routing contract", () => {
  it("routes commerce pages and asset prefix to commerce and discovery/auth/api routes to storefront", () => {
    expect(() =>
      validateRouting(configPath, {
        storefront: [
          "/",
          "/shop",
          "/shop/book-123",
          "/categories",
          "/publishers",
          "/about",
          "/shipping",
          "/returns",
          "/login",
          "/register",
          "/api/auth/session",
          "/api/copilotkit",
        ],
        commerce: [
          "/basket",
          "/checkout/confirmation",
          "/account",
          "/account/orders",
          "/account/orders/ord-123",
          "/commerce-assets/_next/static/chunks/main.js",
          "/commerce-assets/_next/image",
        ],
      }),
    ).not.toThrow();
  });
});
