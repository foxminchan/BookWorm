import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";

export default defineConfig({
  name: "Storefront Unit Tests",
  output: "allure-report",
  plugins: {
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
