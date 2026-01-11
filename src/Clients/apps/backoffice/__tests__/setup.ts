import { faker } from "@faker-js/faker";
import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import os from "node:os";
import path from "node:path";
import process from "node:process";
import { afterAll, afterEach, beforeAll, vi } from "vitest";

import { server } from "@workspace/mocks/node";

// Set unique MSW cookie database per worker BEFORE importing MSW
if (!process.env.MSW_COOKIE_STORE_PATH) {
  process.env.MSW_COOKIE_STORE_PATH = path.resolve(
    os.tmpdir(),
    `msw-cookies-${process.pid}-${Date.now()}-${Math.random().toString(36).slice(2)}.db`,
  );
}

// Set a consistent seed for faker to ensure deterministic test data
const fakerSeed = process.env.FAKER_SEED
  ? parseInt(process.env.FAKER_SEED, 10)
  : 12345;
faker.seed(fakerSeed);

// Establish API mocking before all tests
beforeAll(() => {
  server.listen({ onUnhandledRequest: "warn" });
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

// Mock env variables - match MSW handler base URLs
process.env.NEXT_PUBLIC_GATEWAY_HTTP = "http://localhost:5000";
process.env.NEXT_PUBLIC_GATEWAY_HTTPS = "http://localhost:5000";

// Polyfill for Radix UI pointer capture (required for Select component in tests)
if (!HTMLElement.prototype.hasPointerCapture) {
  HTMLElement.prototype.hasPointerCapture = vi.fn(() => false);
}
if (!HTMLElement.prototype.setPointerCapture) {
  HTMLElement.prototype.setPointerCapture = vi.fn();
}
if (!HTMLElement.prototype.releasePointerCapture) {
  HTMLElement.prototype.releasePointerCapture = vi.fn();
}
