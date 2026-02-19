import { defineConfig } from "allure";

export default defineConfig({
  name: "Backoffice Unit Tests",
  output: "allure-report",
  qualityGate: {
    rules: [
      {
        maxFailures: 0,
      },
    ],
  },
});
