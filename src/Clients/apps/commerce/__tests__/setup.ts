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
  filePath: path.resolve(
    import.meta.dirname,
    "../../storefront/microfrontends.config.json",
  ),
});
const mfeAppName = "commerce";
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

vi.mock("next/image", () => ({
  __esModule: true,
  default: (props: Record<string, unknown>) => {
    const {
      src,
      alt,
      fill: _fill,
      priority: _priority,
      quality: _quality,
      sizes: _sizes,
      loader: _loader,
      ...rest
    } = props;
    const resolvedSrc =
      typeof src === "string"
        ? src
        : ((src as { src?: string } | undefined)?.src ?? "");

    return React.createElement("img", {
      src: resolvedSrc,
      alt: (alt as string | undefined) ?? "",
      ...rest,
    });
  },
}));
