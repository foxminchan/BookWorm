import {
  After,
  AfterAll,
  Before,
  BeforeAll,
  ITestCaseHookParameter,
  Status,
  setDefaultTimeout,
} from "@cucumber/cucumber";
import { Browser, BrowserContext, Page, chromium } from "@playwright/test";

// Set default timeout for steps
setDefaultTimeout(60000);

/**
 * Parse boolean environment variable with support for multiple formats
 * @param value - Environment variable value
 * @param defaultValue - Default value if undefined
 * @returns Parsed boolean value
 */
function parseBoolean(
  value: string | undefined,
  defaultValue: boolean,
): boolean {
  if (value === undefined) return defaultValue;
  const normalized = value.toLowerCase().trim();
  return normalized === "true" || normalized === "1" || normalized === "yes";
}

/**
 * Custom World interface that includes Playwright page and context
 */
type CucumberWorld = ITestCaseHookParameter & {
  page: Page;
  context: BrowserContext;
  attach: (data: Buffer | string, mediaType: string) => void;
};

let browser: Browser;
let context: BrowserContext;
let page: Page;

/**
 * Launch browser before all scenarios
 */
BeforeAll(async function () {
  browser = await chromium.launch({
    headless: parseBoolean(process.env.HEADLESS, true),
    slowMo: process.env.SLOW_MO ? parseInt(process.env.SLOW_MO, 10) : 0,
  });
});

/**
 * Create new context and page before each scenario
 */
Before(async function (this: CucumberWorld) {
  context = await browser.newContext({
    viewport: { width: 1920, height: 1080 },
    baseURL: process.env.BASE_URL || "http://localhost:3001",
    recordVideo: parseBoolean(process.env.VIDEO, false)
      ? { dir: "e2e/reports/videos" }
      : undefined,
  });

  page = await context.newPage();

  // Attach page to world object for step definitions
  this.page = page;
  this.context = context;
});

/**
 * Capture screenshot on failure and close page after each scenario
 */
After(async function (
  this: CucumberWorld,
  { result, pickle }: ITestCaseHookParameter,
) {
  if (result?.status === Status.FAILED) {
    const screenshot = await page.screenshot({ fullPage: true });
    this.attach(screenshot, "image/png");

    // Save screenshot to file
    const timestamp = Date.now();
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
