import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";
import path from "node:path";

import { defaultCategoriesConfig } from "../../allure-enricher.mjs";

const reportName = "Backoffice Test Report";
const enricherPluginPath = path.resolve(
  import.meta.dirname,
  "../../allure-enricher.mjs",
);

const hasPathSegment = (labels, segment) =>
  labels.some(({ name, value }) => {
    if (!value || (name !== "parentSuite" && name !== "package")) {
      return false;
    }
    const normalized = value.replaceAll("\\", "/").toLowerCase();
    return (
      normalized.includes(`/${segment}/`) ||
      normalized.startsWith(`${segment}/`) ||
      normalized.includes(`.${segment}.`)
    );
  });

const isE2ETest = (labels) =>
  labels.some(
    ({ name, value }) =>
      (name === "framework" && value?.toLowerCase() === "playwright") ||
      (name === "layer" && value?.toLowerCase() === "e2e") ||
      (name === "parentSuite" &&
        (value?.startsWith("e2e/") || value?.endsWith(".feature"))),
  );

export default defineConfig({
  name: reportName,
  output: "allure-report",
  historyPath: "./.allure/history.jsonl",
  historyLimit: 20,
  defaultLabels: {
    severity: "normal",
    owner: "BookWorm Engineering",
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
  categories: defaultCategoriesConfig,
  plugins: {
    enricher: {
      import: enricherPluginPath,
      options: {
        scope: "backoffice",
        defaultEpic: "Backoffice",
      },
    },
    awesome: {
      options: {
        singleFile: true,
        reportLanguage: "en",
        reportName,
        groupBy: ["parentSuite", "suite", "subSuite"],
      },
    },
    log: {
      options: {
        groupBy: "none",
        filter: ({ status }) => status === "failed" || status === "broken",
      },
    },
    dashboard: {
      options: {
        reportName,
        reportLanguage: "en",
      },
    },
  },
  qualityGate: {
    rules: [
      {
        maxFailures: 0,
        fastFail: true,
      },
    ],
    use: [...qualityGateDefaultRules],
  },
});
