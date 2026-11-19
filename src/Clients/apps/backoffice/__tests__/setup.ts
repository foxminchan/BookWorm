import { faker } from "@faker-js/faker";
import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterAll, afterEach, beforeAll, vi } from "vitest";

import {
  createNextNavigationMock,
  setupTestLifecycle,
} from "@workspace/app-config/vitest-setup";
import { server } from "@workspace/mocks/node";

setupTestLifecycle({
  faker,
  server,
  cleanup,
  beforeAll,
  afterEach,
  afterAll,
  onUnhandledFrame: "warn",
  httpsGatewayUrl: "http://localhost:5000",
});

vi.mock("next/navigation", () => createNextNavigationMock(vi));

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
