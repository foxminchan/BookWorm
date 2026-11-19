import { defineConfig } from "allure";
import path from "node:path";
import process from "node:process";

const reportName =
  process.env.ALLURE_REPORT_NAME || "BookWorm Unified Test Report";
const historyPath =
  process.env.ALLURE_HISTORY_PATH || "./.allure/history.jsonl";
const enricherPluginPath = path.resolve(
  import.meta.dirname,
  "./allure-enricher.mjs",
);

export default defineConfig({
  name: reportName,
  output: "allure-report",
  historyPath,
  historyLimit: 20,
  defaultLabels: {
    severity: "normal",
  },
  environments: {
    backend_unit: {
      name: "Backend Unit & Architecture",
      matcher: ({ labels }) => {
        const isDotNet = labels.some(
          ({ name, value }) =>
            (name === "language" && value?.toLowerCase() === "c#") ||
            (name === "parentSuite" && value?.startsWith("BookWorm.")),
        );
        const isContractOrIntegration = labels.some(
          ({ name, value }) =>
            name === "parentSuite" &&
            (value?.endsWith(".ContractTests") ||
              value?.endsWith(".IntegrationTests")),
        );
        return isDotNet && !isContractOrIntegration;
      },
    },
    backend_contract: {
      name: "Backend Contract & Integration",
      matcher: ({ labels }) =>
        labels.some(
          ({ name, value }) =>
            name === "parentSuite" &&
            (value?.endsWith(".ContractTests") ||
              value?.endsWith(".IntegrationTests")),
        ),
    },
    backoffice: {
      name: "Backoffice (FE)",
      matcher: ({ labels }) =>
        labels.some(
          ({ name, value }) =>
            name === "package" && value?.includes("backoffice"),
        ),
    },
    storefront: {
      name: "Storefront (FE)",
      matcher: ({ labels }) =>
        labels.some(
          ({ name, value }) =>
            name === "package" && value?.includes("storefront"),
        ),
    },
  },
  plugins: {
    enricher: {
      import: enricherPluginPath,
      options: {},
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
});
