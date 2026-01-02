import {
  After,
  AfterAll,
  Before,
  BeforeAll,
  Status,
  setDefaultTimeout,
} from "@cucumber/cucumber";
import { ITestCaseHookParameter } from "@cucumber/cucumber/lib/support_code_library_builder/types";
import { Browser, BrowserContext, Page, chromium } from "@playwright/test";

// Set default timeout for steps
setDefaultTimeout(60000);

let browser: Browser;
let context: BrowserContext;
let page: Page;

/**
 * Launch browser before all scenarios
 */
BeforeAll(async function () {
  browser = await chromium.launch({
    headless: process.env.HEADLESS !== "false",
    slowMo: process.env.SLOW_MO ? parseInt(process.env.SLOW_MO) : 0,
  });
});

/**
 * Create new context and page before each scenario
 */
Before(async function (this: ITestCaseHookParameter) {
  context = await browser.newContext({
    viewport: { width: 1920, height: 1080 },
    baseURL: process.env.BASE_URL || "http://localhost:3000",
    recordVideo:
      process.env.VIDEO === "true" ? { dir: "e2e/reports/videos" } : undefined,
  });

  page = await context.newPage();

  // Attach page to world object for step definitions
  (this as any).page = page;
  (this as any).context = context;
});

/**
 * Capture screenshot on failure and close page after each scenario
 */
After(async function (
  this: ITestCaseHookParameter & {
    attach: (data: Buffer | string, mediaType: string) => void;
  },
  { result, pickle }: any,
) {
  if (result?.status === Status.FAILED) {
    const screenshot = await page.screenshot({ fullPage: true });
    this.attach(screenshot, "image/png");

    // Save screenshot to file
    const timestamp = new Date().getTime();
    const screenshotPath = `e2e/reports/screenshots/${pickle.name}-${timestamp}.png`;
    await page.screenshot({ path: screenshotPath, fullPage: true });
  }

  await page.close();
  await context.close();
});

/**
 * Close browser after all scenarios
 */
AfterAll(async function () {
  await browser.close();
});
