import { Page } from "@playwright/test";

/**
 * World context for Cucumber scenarios
 * Provides access to page, context, and shared state across steps
 */
export class World {
  page!: Page;
  testData: Record<string, unknown> = {};

  constructor() {
    this.testData = {};
  }

  /**
   * Store data that persists across steps in a scenario
   */
  setData(key: string, value: unknown): void {
    this.testData[key] = value;
  }

  /**
   * Retrieve stored data
   * Returns undefined if key doesn't exist
   */
  getData<T = unknown>(key: string): T | undefined {
    return this.testData[key] as T | undefined;
  }

  /**
   * Retrieve stored data or throw if key doesn't exist
   * Use when the data is expected to be present
   */
  getDataOrThrow<T = unknown>(key: string): T {
    if (!this.hasData(key)) {
      throw new Error(
        `Test data key "${key}" not found. Available keys: ${Object.keys(this.testData).join(", ") || "none"}`,
      );
    }
    return this.testData[key] as T;
  }

  /**
   * Check if data exists
   */
  hasData(key: string): boolean {
    return key in this.testData;
  }
}

/**
 * Wait for page to be fully loaded
 */
export async function waitForPageLoad(page: Page): Promise<void> {
  await page.waitForLoadState("networkidle");
  await page.waitForLoadState("domcontentloaded");
}

/**
 * Wait for element to be visible
 */
export async function waitForElement(
  page: Page,
  selector: string,
  timeout = 10000,
): Promise<void> {
  await page.waitForSelector(selector, { state: "visible", timeout });
}

/**
 * Extract text content from element
 */
export async function getTextContent(
  page: Page,
  selector: string,
): Promise<string> {
  const element = await page.waitForSelector(selector);
  const text = await element?.textContent();
  return text?.trim() || "";
}

/**
 * Click element with retry logic
 */
export async function clickElement(
  page: Page,
  selector: string,
): Promise<void> {
  await page.waitForSelector(selector, { state: "visible" });
  await page.click(selector, { force: false });
}

/**
 * Type text into input field
 */
export async function typeText(
  page: Page,
  selector: string,
  text: string,
): Promise<void> {
  await page.waitForSelector(selector, { state: "visible" });
  await page.fill(selector, text);
}

/**
 * Select option from dropdown
 */
export async function selectOption(
  page: Page,
  selector: string,
  value: string,
): Promise<void> {
  await page.waitForSelector(selector, { state: "visible" });
  await page.selectOption(selector, value);
}

/**
 * Check if element is visible
 */
export async function isVisible(
  page: Page,
  selector: string,
): Promise<boolean> {
  try {
    await page.waitForSelector(selector, { state: "visible", timeout: 5000 });
    return true;
  } catch {
    return false;
  }
}

/**
 * Check if element contains text
 */
export async function containsText(
  page: Page,
  selector: string,
  text: string,
): Promise<boolean> {
  const element = await page.waitForSelector(selector);
  const content = await element?.textContent();
  return content?.includes(text) || false;
}

/**
 * Wait for URL to contain path
 */
export async function waitForUrl(
  page: Page,
  path: string,
  timeout = 10000,
): Promise<void> {
  await page.waitForURL((url) => url.pathname.includes(path), { timeout });
}

/**
 * Scroll to element
 */
export async function scrollToElement(
  page: Page,
  selector: string,
): Promise<void> {
  await page.locator(selector).scrollIntoViewIfNeeded();
}

/**
 * Get attribute value from element
 */
export async function getAttribute(
  page: Page,
  selector: string,
  attribute: string,
): Promise<string | null> {
  const element = await page.waitForSelector(selector);
  return await element?.getAttribute(attribute);
}

/**
 * Wait for element to be hidden
 */
export async function waitForElementHidden(
  page: Page,
  selector: string,
  timeout = 10000,
): Promise<void> {
  await page.waitForSelector(selector, { state: "hidden", timeout });
}

/**
 * Press keyboard key
 */
export async function pressKey(page: Page, key: string): Promise<void> {
  await page.keyboard.press(key);
}

/**
 * Hover over element
 */
export async function hoverElement(
  page: Page,
  selector: string,
): Promise<void> {
  await page.hover(selector);
}
