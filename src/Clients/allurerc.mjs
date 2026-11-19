import { defineConfig } from "allure";
import process from "node:process";
import { fileURLToPath } from "node:url";

import { defaultCategoriesConfig } from "@workspace/allure-config";

const reportName =
  process.env.ALLURE_REPORT_NAME || "BookWorm Unified Test Report";
const reportScope =
  process.env.ALLURE_REPORT_SCOPE ||
  (reportName.toLowerCase().includes("backend") ? "backend" : "unified");
const isBackendScope = reportScope === "backend";
const historyPath =
  process.env.ALLURE_HISTORY_PATH || "./.allure/history.jsonl";
const enricherPluginPath = fileURLToPath(
  import.meta.resolve("@workspace/allure-config"),
);

const isDotNetTest = (labels) =>
  labels.some(
    ({ name, value }) =>
      (name === "language" && value?.toLowerCase() === "c#") ||
      (name === "parentSuite" && value?.startsWith("BookWorm.")),
  );

const isArchTest = (labels) =>
  labels.some(
    ({ name, value }) =>
      name === "parentSuite" &&
      (value === "BookWorm.ArchTests" || value?.endsWith(".ArchTests")),
  );

const isContractOrIntegrationTest = (labels) =>
  labels.some(
    ({ name, value }) =>
      name === "parentSuite" &&
      (value?.endsWith(".ContractTests") ||
        value?.endsWith(".IntegrationTests")),
  );

const backendEnvironments = {
  unit: {
    name: "Microservice Unit Tests",
    variables: {
      Layer: "Unit",
      Scope: "Domain & Application Vertical Slices",
      Runner: "TUnit + Microsoft.Testing.Platform",
    },
    matcher: ({ labels }) =>
      isDotNetTest(labels) &&
      !isArchTest(labels) &&
      !isContractOrIntegrationTest(labels),
  },
  architecture: {
    name: "Architecture Fitness Tests",
    variables: {
      Layer: "Architecture",
      Scope: "DDD & Vertical Slice Boundaries",
      Runner: "ArchUnitNET + TUnit",
    },
    matcher: ({ labels }) => isArchTest(labels),
  },
  contract: {
    name: "Contract & Integration Tests",
    variables: {
      Layer: "Contract & Integration",
      Scope: "Pact Consumer & Provider Messaging",
      Runner: "PactNet + TUnit",
    },
    matcher: ({ labels }) => isContractOrIntegrationTest(labels),
  },
};

const unifiedEnvironments = {
  backend_unit: {
    name: "Backend Unit",
    variables: {
      Stack: ".NET 11 / C# 15",
      Runner: "TUnit",
      Layer: "Unit",
    },
    matcher: ({ labels }) =>
      isDotNetTest(labels) &&
      !isArchTest(labels) &&
      !isContractOrIntegrationTest(labels),
  },
  backend_arch: {
    name: "Backend Architecture",
    variables: {
      Stack: ".NET 11 / C# 15",
      Runner: "ArchUnitNET + TUnit",
      Layer: "Architecture",
    },
    matcher: ({ labels }) => isArchTest(labels),
  },
  backend_contract: {
    name: "Backend Contract & Integration",
    variables: {
      Stack: ".NET 11 / C# 15",
      Runner: "PactNet + TUnit",
      Layer: "Contract & Integration",
    },
    matcher: ({ labels }) => isContractOrIntegrationTest(labels),
  },
  backoffice: {
    name: "Backoffice (FE)",
    variables: {
      Stack: "Next.js 16 / React 19",
      Runner: "Vitest + Playwright BDD",
      Application: "@bookworm/backoffice",
    },
    matcher: ({ labels }) =>
      labels.some(
        ({ name, value }) =>
          (name === "package" && value?.includes("backoffice")) ||
          (name === "epic" && value?.toLowerCase() === "backoffice"),
      ),
  },
  storefront: {
    name: "Storefront (FE)",
    variables: {
      Stack: "Next.js 16 / React 19",
      Runner: "Vitest + Playwright BDD",
      Application: "@bookworm/storefront",
    },
    matcher: ({ labels }) =>
      labels.some(
        ({ name, value }) =>
          (name === "package" && value?.includes("storefront")) ||
          (name === "epic" && value?.toLowerCase() === "storefront"),
      ),
  },
};

export default defineConfig({
  name: reportName,
  output: "allure-report",
  historyPath,
  historyLimit: 20,
  defaultLabels: {
    severity: "normal",
    owner: "BookWorm Engineering",
  },
  variables: isBackendScope
    ? {
        Platform: ".NET 11 / C# 15",
        Orchestration: ".NET Aspire 17",
        "Test Runner": "TUnit + Microsoft.Testing.Platform",
        "Contract Framework": "PactNet v5",
        "Architecture Fitness": "ArchUnitNET",
      }
    : {
        Application: "BookWorm Distributed System",
        Backend: ".NET 11 / C# 15 / TUnit / PactNet / ArchUnitNET",
        Frontend: "Next.js 16 / React 19 / Vitest / Playwright BDD",
        Orchestration: "Bun + Turborepo + .NET Aspire 17",
      },
  environments: isBackendScope ? backendEnvironments : unifiedEnvironments,
  categories: defaultCategoriesConfig,
  plugins: {
    enricher: {
      import: enricherPluginPath,
      options: {
        scope: reportScope,
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
});
