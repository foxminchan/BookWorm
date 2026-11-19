import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";
import path from "node:path";

const enricherPluginPath = path.resolve(
  import.meta.dirname,
  "../../allure-enricher.mjs",
);

export default defineConfig({
  name: "Backoffice Unit Tests",
  output: "allure-report",
  historyPath: "./.allure/history.jsonl",
  historyLimit: 20,
  defaultLabels: {
    severity: "normal",
    epic: "Backoffice",
  },
  plugins: {
    enricher: {
      import: enricherPluginPath,
      options: {
        defaultEpic: "Backoffice",
      },
    },
    awesome: {
      options: {
        singleFile: true,
        reportLanguage: "en",
        reportName: "Backoffice Unit Tests",
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
        reportName: "Backoffice Unit Tests",
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
