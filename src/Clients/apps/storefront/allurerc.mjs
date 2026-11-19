import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";

import {
  createAppAllureConfig,
  createFrontendEnvironments,
} from "@workspace/allure-config";

export default defineConfig(
  createAppAllureConfig({
    reportName: "Storefront Test Report",
    enricherOptions: {
      scope: "storefront",
      defaultEpic: "Storefront",
    },
    variables: {
      Application: "@bookworm/storefront",
      Framework: "Next.js 16 / React 19 / CopilotKit",
      "Unit Runner": "Vitest + Happy DOM + MSW + fast-check",
      "E2E Runner": "Playwright BDD + Chromium",
    },
    environments: createFrontendEnvironments({
      featuresScope: "Account, Basket, Catalog, Home, Ordering, Rating",
      componentsScope: "Storefront Shell, AI Copilot & A2UI Widgets",
      coreName: "Hooks, Atoms & Utilities",
      coreScope: "Hooks, Jotai Atoms, WebMCP, Lib & Utils",
      coreSegments: ["hooks", "atoms", "lib", "utils"],
      e2eScope: "BDD Customer Journey Scenarios",
      includeFuzz: true,
    }),
    qualityGateRules: qualityGateDefaultRules,
  }),
);
