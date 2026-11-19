import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";
import { fileURLToPath } from "node:url";

import {
  createAppAllureConfig,
  hasPathSegment,
  isE2ETest,
} from "@workspace/allure-config";

export default defineConfig(
  createAppAllureConfig({
    reportName: "Backoffice Test Report",
    enricherPluginPath: fileURLToPath(
      import.meta.resolve("@workspace/allure-config"),
    ),
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
    environments: {
      features: {
        name: "Domain Features",
        variables: {
          Layer: "Integration",
          Scope: "Books, Orders, Customers, Overview, Reviews",
          Runner: "Vitest + MSW + TanStack Query",
        },
        matcher: ({ labels }) =>
          !isE2ETest(labels) && hasPathSegment(labels, "features"),
      },
      components: {
        name: "UI Components",
        variables: {
          Layer: "Unit",
          Scope: "Dashboard Layout, Tables, Dialogs & Guards",
          Runner: "Vitest + Testing Library",
        },
        matcher: ({ labels }) =>
          !isE2ETest(labels) && hasPathSegment(labels, "components"),
      },
      core: {
        name: "Hooks & Core Utilities",
        variables: {
          Layer: "Unit",
          Scope: "Hooks, API Client, Lib, Factories & Utils",
          Runner: "Vitest",
        },
        matcher: ({ labels }) =>
          !isE2ETest(labels) &&
          (hasPathSegment(labels, "hooks") ||
            hasPathSegment(labels, "lib") ||
            hasPathSegment(labels, "utils") ||
            hasPathSegment(labels, "factories")),
      },
      e2e: {
        name: "E2E (Playwright BDD)",
        variables: {
          Layer: "E2E",
          Scope: "BDD Backoffice Admin Scenarios",
          Runner: "Playwright + Chromium",
        },
        matcher: ({ labels }) => isE2ETest(labels),
      },
    },
    qualityGateRules: qualityGateDefaultRules,
  }),
);
