import React from "react";

import { faker } from "@faker-js/faker";
import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterAll, afterEach, beforeAll, vi } from "vitest";

import { server } from "@workspace/mocks/node";

// Set a consistent seed for faker to ensure deterministic test data
const fakerSeed = process.env.FAKER_SEED
  ? parseInt(process.env.FAKER_SEED, 10)
  : 12345;
faker.seed(fakerSeed);

// Establish API mocking before all tests
beforeAll(() => {
  server.listen({ onUnhandledRequest: "error" });
});

// Reset any request handlers that we may add during the tests,
// so they don't affect other tests
afterEach(() => {
  server.resetHandlers();
  cleanup();
});

// Clean up after the tests are finished
afterAll(() => {
  server.close();
});

// Mock Next.js router
vi.mock("next/navigation", () => ({
  useRouter() {
    return {
      push: vi.fn(),
      replace: vi.fn(),
      prefetch: vi.fn(),
      back: vi.fn(),
      pathname: "/",
      query: {},
      asPath: "/",
    };
  },
  useSearchParams() {
    return new URLSearchParams();
  },
  usePathname() {
    return "/";
  },
  useParams() {
    return {};
  },
  redirect: vi.fn(),
  notFound: vi.fn(),
}));

// Mock next/image to avoid URL parsing issues in node/happy-dom
vi.mock("next/image", () => ({
  __esModule: true,
  default: (props: any) => {
    const { src, alt, fill, priority, quality, sizes, loader, ...rest } = props;
    const resolvedSrc = typeof src === "string" ? src : (src?.src ?? "");

    // Drop Next.js specific props like `fill` that leak onto img in the test env
    return React.createElement("img", {
      src: resolvedSrc,
      alt: alt ?? "",
      ...rest,
    });
  },
}));

// Mock env variables
process.env.NEXT_PUBLIC_GATEWAY_HTTP = "http://localhost:5000";
process.env.NEXT_PUBLIC_GATEWAY_HTTPS = "https://localhost:5001";
