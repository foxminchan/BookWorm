import { defineConfig } from "allure";
import { qualityGateDefaultRules } from "allure/rules";

import {
  createAppAllureConfig,
  createFrontendEnvironments,
} from "@workspace/allure-config";

export default defineConfig(
  createAppAllureConfig({
    reportName: "Commerce Test Report",
    enricherOptions: {
      scope: "commerce",
      defaultEpic: "Commerce",
    },
    variables: {
      Application: "@bookworm/commerce",
      Framework: "Next.js 16 / React 19",
      "Unit Runner": "Vitest + Happy DOM + MSW",
    },
    environments: createFrontendEnvironments({
      featuresScope: "Account, Basket, Checkout & Orders",
      componentsScope: "Commerce Zone Shell & Skeletons",
      coreName: "Proxy & Utilities",
      coreScope: "Auth Proxy, Query Client & Utils",
      coreSegments: ["lib", "utils"],
      e2eScope: "Commerce Journey Scenarios",
      includeFuzz: false,
    }),
    qualityGateRules: qualityGateDefaultRules,
  }),
);
