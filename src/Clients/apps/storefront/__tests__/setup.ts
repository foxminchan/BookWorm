import React from "react";

import { faker } from "@faker-js/faker";
import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { MicrofrontendsServer } from "@vercel/microfrontends/microfrontends/server";
import crypto from "node:crypto";
import path from "node:path";
import { afterAll, afterEach, beforeAll, vi } from "vitest";

import {
  createNextNavigationMock,
  setupTestLifecycle,
} from "@workspace/app-config/vitest-setup";
import { server } from "@workspace/mocks/node";

const mfeServer = MicrofrontendsServer.fromFile({
  filePath: path.resolve(import.meta.dirname, "../microfrontends.config.json"),
});
const mfeAppName = "storefront";
process.env.NEXT_PUBLIC_MFE_CURRENT_APPLICATION = mfeAppName;
process.env.NEXT_PUBLIC_MFE_CURRENT_APPLICATION_HASH = crypto
  .createHash("md5")
  .update(mfeAppName)
  .digest("hex")
  .slice(0, 6);
process.env.NEXT_PUBLIC_MFE_CLIENT_CONFIG = JSON.stringify(
  mfeServer.config.toClientConfig({ removeFlaggedPaths: true }).serialize(),
);

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
