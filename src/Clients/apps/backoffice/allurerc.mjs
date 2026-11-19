import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";

import {
  createAppAllureConfig,
  createFrontendEnvironments,
} from "@workspace/allure-config";

export default defineConfig(
  createAppAllureConfig({
    reportName: "Backoffice Test Report",
    enricherOptions: {
      scope: "backoffice",
      defaultEpic: "Backoffice",
    },
    variables: {
      Application: "@bookworm/backoffice",
      Framework: "Next.js 16 / React 19",
      "Unit Runner": "Vitest + Happy DOM + MSW",
      "E2E Runner": "Playwright BDD + Chromium",
    },
    environments: createFrontendEnvironments({
      featuresScope: "Books, Orders, Customers, Overview, Reviews",
      componentsScope: "Dashboard Layout, Tables, Dialogs & Guards",
      coreName: "Hooks & Core Utilities",
      coreScope: "Hooks, API Client, Lib, Factories & Utils",
      coreSegments: ["hooks", "lib", "utils", "factories"],
      e2eScope: "BDD Backoffice Admin Scenarios",
    }),
    qualityGateRules: qualityGateDefaultRules,
  }),
);
