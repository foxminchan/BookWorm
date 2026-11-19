import { defineConfig, devices } from "@playwright/test";
import { defineBddConfig } from "playwright-bdd";

import { createPlaywrightConfig } from "@workspace/app-config/playwright";

const testDir = defineBddConfig({
  features: "e2e/features/**/*.feature",
  steps: "e2e/steps/**/*.ts",
  importTestFrom: "e2e/steps/fixtures.ts",
});

export default defineConfig(
  createPlaywrightConfig({
    testDir,
    epic: "Storefront",
    application: "@bookworm/storefront",
    defaultPort: 3000,
    desktopChromeDevice: devices["Desktop Chrome"],
  }),
);
