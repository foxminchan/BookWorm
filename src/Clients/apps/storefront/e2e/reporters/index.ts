import * as reporter from "cucumber-html-reporter";
import { resolve } from "path";

const options = {
  theme: "bootstrap" as const,
  jsonFile: resolve(__dirname, "../../reports/cucumber-report.json"),
  output: resolve(__dirname, "../../reports/cucumber-report.html"),
  reportSuiteAsScenarios: true,
  scenarioTimestamp: true,
  launchReport: false,
  ignoreBadJsonFile: true,
  metadata: {
    "App Version": "1.0.0",
    "Test Environment": process.env.NODE_ENV || "development",
    Browser: "Chromium",
    Platform: process.platform,
    Parallel: "Scenarios",
    Executed: new Date().toLocaleString(),
  },
  failedSummaryReport: true,
};

reporter.generate(options);
