import React from "react";

import { faker } from "@faker-js/faker";
import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { MicrofrontendsServer } from "@vercel/microfrontends/microfrontends/server";
import path from "node:path";
import { afterAll, afterEach, beforeAll, vi } from "vitest";

import {
  createNextImageMock,
  createNextNavigationMock,
  setupMfeTestEnv,
  setupTestLifecycle,
} from "@workspace/app-config/vitest-setup";
import { server } from "@workspace/mocks/node";

setupMfeTestEnv({
  MicrofrontendsServer,
  configPath: path.resolve(
    import.meta.dirname,
    "../../storefront/microfrontends.json",
  ),
  appName: "commerce",
});

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
vi.mock("next/image", () => createNextImageMock(React.createElement));
