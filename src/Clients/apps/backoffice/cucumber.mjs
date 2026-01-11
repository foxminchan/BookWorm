/**
 * Cucumber configuration for BDD E2E testing - Backoffice
 * @see https://github.com/cucumber/cucumber-js/blob/main/docs/configuration.md
 */
export default {
  default: {
    paths: ["e2e/features/**/*.feature"],
    import: [
      "e2e/step-definitions/**/*.ts",
      "e2e/hooks/*.ts",
      "e2e/world/*.ts",
      "e2e/reporters/*.ts",
    ],
    loader: ["ts-node/esm"],
    format: [
      "progress-bar",
      "@cucumber/pretty-formatter",
      "json:e2e/reports/cucumber-report.json",
      "html:e2e/reports/cucumber-report.html",
    ],
    formatOptions: {
      snippetInterface: "async-await",
    },
    publishQuiet: true,
  },
  ci: {
    paths: ["e2e/features/**/*.feature"],
    import: [
      "e2e/step-definitions/**/*.ts",
      "e2e/hooks/*.ts",
      "e2e/world/*.ts",
      "e2e/reporters/*.ts",
    ],
    loader: ["ts-node/esm"],
    format: ["progress", "json:e2e/reports/cucumber-report.json"],
    parallel: 4,
    publishQuiet: true,
  },
};
