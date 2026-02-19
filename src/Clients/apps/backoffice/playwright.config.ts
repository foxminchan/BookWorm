import { defineConfig, devices } from "@playwright/test";
import { defineBddConfig } from "playwright-bdd";

const isCI = !!process.env.CI;

const testDir = defineBddConfig({
  features: "e2e/features/**/*.feature",
  steps: "e2e/steps/**/*.ts",
  importTestFrom: "e2e/steps/fixtures.ts",
});

export default defineConfig({
  testDir,
  fullyParallel: true,
  forbidOnly: isCI,
  retries: isCI ? 2 : 0,
  workers: isCI ? 4 : undefined,
  reporter: [
    ["allure-playwright", { outputFolder: "allure-results", suiteTitle: true }],
  ],
  use: {
    baseURL: process.env.BASE_URL || "http://localhost:3001",
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    viewport: { width: 1920, height: 1080 },
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
});
