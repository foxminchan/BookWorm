import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";
import path from "node:path";

const enricherPluginPath = path.resolve(
  import.meta.dirname,
  "../../allure-enricher.mjs",
);

export default defineConfig({
  name: "Storefront Unit Tests",
  output: "allure-report",
  historyPath: "./.allure/history.jsonl",
  historyLimit: 20,
  defaultLabels: {
    severity: "normal",
    epic: "Storefront",
  },
  plugins: {
    enricher: {
      import: enricherPluginPath,
      options: {
        defaultEpic: "Storefront",
      },
    },
    awesome: {
      options: {
        singleFile: true,
        reportLanguage: "en",
        reportName: "Storefront Unit Tests",
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
        reportName: "Storefront Unit Tests",
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
