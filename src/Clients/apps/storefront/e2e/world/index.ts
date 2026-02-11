import { Page } from "@playwright/test";

/**
 * World context for Cucumber scenarios
 * Provides access to page, context, and shared state across steps
 */
export class World {
  page!: Page;
  testData: Record<string, any> = {};

  constructor() {
    this.testData = {};
  }

  /**
   * Store data that persists across steps in a scenario
   */
  setData(key: string, value: any): void {
    this.testData[key] = value;
  }

  /**
   * Retrieve stored data
   * Returns undefined if key doesn't exist
   */
  getData<T = any>(key: string): T | undefined {
    return this.testData[key] as T | undefined;
  }

  /**
   * Retrieve stored data or throw if key doesn't exist
   * Use when the data is expected to be present
   */
  getDataOrThrow<T = any>(key: string): T {
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
 * Parse currency string to number (e.g., "$25.99" -> 25.99)
 */
export function parseCurrency(currencyString: string): number {
  const cleaned = currencyString.replaceAll(/[$,]/g, "");
  return Number.parseFloat(cleaned);
}

/**
 * Format number as currency (e.g., 25.99 -> "$25.99")
 */
export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(amount);
}

/**
 * Scroll element into view
 */
export async function scrollToElement(
  page: Page,
  selector: string,
): Promise<void> {
  await page.locator(selector).scrollIntoViewIfNeeded();
}

/**
 * Check if element exists (doesn't wait)
 */
export async function elementExists(
  page: Page,
  selector: string,
): Promise<boolean> {
  return (await page.locator(selector).count()) > 0;
}

/**
 * Get all matching elements
 */
export async function getElements(page: Page, selector: string) {
  return await page.locator(selector).all();
}

/**
 * Wait for navigation
 */
export async function waitForNavigation(page: Page): Promise<void> {
  await page.waitForLoadState("networkidle");
}

/**
 * Reload page
 */
export async function reloadPage(page: Page): Promise<void> {
  await page.reload({ waitUntil: "networkidle" });
}

/**
 * Press keyboard key
 */
export async function pressKey(page: Page, key: string): Promise<void> {
  await page.keyboard.press(key);
}

/**
 * Select option from dropdown
 */
export async function selectOption(
  page: Page,
  selector: string,
  value: string,
): Promise<void> {
  await page.selectOption(selector, value);
}
