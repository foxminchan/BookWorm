import { defineConfig } from "allure";

export default defineConfig({
  name: "Storefront Unit Tests",
  output: "allure-report",
  qualityGate: {
    rules: [
      {
        maxFailures: 0,
      },
    ],
  },
});
