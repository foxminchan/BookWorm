import { Locator, Page, expect } from "@playwright/test";

import { BASE_PATH } from "../fixtures/test-data";

/**
 * Base Page Object with common functionality for backoffice pages
 */
export abstract class BasePage {
  protected page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  /**
   * Prefix path with Next.js basePath ("/backoffice") if not already prefixed
   */
  protected resolvePath(path: string): string {
    if (path.startsWith("http://") || path.startsWith("https://")) {
      return path;
    }
    if (path === BASE_PATH || path.startsWith(`${BASE_PATH}/`)) {
      return path;
    }
    if (path === "/") {
      return BASE_PATH;
    }
    return `${BASE_PATH}${path.startsWith("/") ? path : `/${path}`}`;
  }

  /**
   * Navigate to specific URL
   */
  async goto(path: string): Promise<void> {
    await this.page.goto(this.resolvePath(path));
    await this.waitForPageLoad();
  }

  /**
   * Wait for page to be fully loaded
   */
  async waitForPageLoad(): Promise<void> {
    await this.page.waitForLoadState("networkidle");
  }

  /**
   * Get page title
   */
  async getTitle(): Promise<string> {
    return await this.page.title();
  }

  /**
   * Get current URL
   */
  getCurrentUrl(): string {
    return this.page.url();
  }

  /**
   * Check if URL contains path
   */
  async assertUrlContains(path: string): Promise<void> {
    await expect(this.page).toHaveURL(new RegExp(path));
  }

  /**
   * Check if URL matches pattern or string
   */
  async assertUrlEquals(url: string | RegExp): Promise<void> {
    await expect(this.page).toHaveURL(url);
  }

  /**
   * Wait for element to be visible
   */
  async waitForElement(selector: string, timeout = 10000): Promise<Locator> {
    const locator = this.page.locator(selector);
    await locator.waitFor({ state: "visible", timeout });
    return locator;
  }

  /**
   * Click element
   */
  async click(selector: string): Promise<void> {
    await this.page.click(selector);
  }

  /**
   * Click button by text
   */
  async clickButtonByText(text: string): Promise<void> {
    await this.page.getByRole("button", { name: text }).click();
  }

  /**
   * Click link by text
   */
  async clickLinkByText(text: string): Promise<void> {
    await this.page.getByRole("link", { name: text }).click();
  }

  /**
   * Fill input field
   */
  async fill(selector: string, value: string): Promise<void> {
    await this.page.fill(selector, value);
  }

  /**
   * Fill input by label
   */
  async fillByLabel(label: string, value: string): Promise<void> {
    await this.page.getByLabel(label).fill(value);
  }

  /**
   * Check if element is visible
   */
  async isVisible(selector: string): Promise<boolean> {
    return await this.page.locator(selector).isVisible();
  }

  /**
   * Get text content from element
   */
  async getText(selector: string): Promise<string> {
    return (await this.page.textContent(selector)) || "";
  }

  /**
   * Assert element is visible
   */
  async assertVisible(selector: string): Promise<void> {
    await expect(this.page.locator(selector)).toBeVisible();
  }

  /**
   * Assert element is hidden
   */
  async assertHidden(selector: string): Promise<void> {
    await expect(this.page.locator(selector)).toBeHidden();
  }

  /**
   * Assert heading exists
   */
  async assertHeading(text: string): Promise<void> {
    await expect(this.page.getByRole("heading", { name: text })).toBeVisible();
  }

  /**
   * Wait for navigation
   */
  async waitForNavigation(): Promise<void> {
    await this.page.waitForLoadState("networkidle");
  }

  /**
   * Press keyboard key
   */
  async pressKey(key: string): Promise<void> {
    await this.page.keyboard.press(key);
  }

  /**
   * Wait for URL to match pattern
   */
  async waitForUrl(pattern: string | RegExp, timeout = 10000): Promise<void> {
    await this.page.waitForURL(pattern, { timeout });
  }

  /**
   * Reload page
   */
  async reload(): Promise<void> {
    await this.page.reload();
    await this.waitForPageLoad();
  }
}
