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
    reportName: "Storefront Test Report",
    enricherPluginPath: fileURLToPath(
      import.meta.resolve("@workspace/allure-config"),
    ),
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
    environments: {
      features: {
        name: "Domain Features",
        variables: {
          Layer: "Integration",
          Scope: "Account, Basket, Catalog, Home, Ordering, Rating",
          Runner: "Vitest + MSW + TanStack Query",
        },
        matcher: ({ labels }) =>
          !isE2ETest(labels) && hasPathSegment(labels, "features"),
      },
      components: {
        name: "UI Components",
        variables: {
          Layer: "Unit",
          Scope: "Storefront Shell, AI Copilot & A2UI Widgets",
          Runner: "Vitest + Testing Library",
        },
        matcher: ({ labels }) =>
          !isE2ETest(labels) && hasPathSegment(labels, "components"),
      },
      core: {
        name: "Hooks, Atoms & Utilities",
        variables: {
          Layer: "Unit",
          Scope: "Hooks, Jotai Atoms, WebMCP, Lib & Utils",
          Runner: "Vitest",
        },
        matcher: ({ labels }) =>
          !isE2ETest(labels) &&
          (hasPathSegment(labels, "hooks") ||
            hasPathSegment(labels, "atoms") ||
            hasPathSegment(labels, "lib") ||
            hasPathSegment(labels, "utils")),
      },
      fuzz: {
        name: "Property & Fuzz Tests",
        variables: {
          Layer: "Unit",
          Scope: "Property-Based Invariants & Fuzzing",
          Runner: "Vitest + fast-check",
        },
        matcher: ({ labels }) =>
          !isE2ETest(labels) && hasPathSegment(labels, "fuzz"),
      },
      e2e: {
        name: "E2E (Playwright BDD)",
        variables: {
          Layer: "E2E",
          Scope: "BDD Customer Journey Scenarios",
          Runner: "Playwright + Chromium",
        },
        matcher: ({ labels }) => isE2ETest(labels),
      },
    },
    qualityGateRules: qualityGateDefaultRules,
  }),
);
