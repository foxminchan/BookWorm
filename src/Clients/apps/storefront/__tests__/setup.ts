import React from "react";

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
  onUnhandledFrame: "error",
  httpsGatewayUrl: "https://localhost:5001",
});

vi.mock("next/navigation", () => createNextNavigationMock(vi));

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
