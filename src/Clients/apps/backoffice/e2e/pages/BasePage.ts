import { Locator, Page, expect } from "@playwright/test";

/**
 * Base Page Object with common functionality for backoffice pages
 */
export abstract class BasePage {
  protected page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  /**
   * Navigate to specific URL
   */
  async goto(path: string): Promise<void> {
    await this.page.goto(path);
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
   * Check if URL matches exactly
   */
  async assertUrlEquals(url: string): Promise<void> {
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
   * Select option from dropdown
   */
  async select(selector: string, value: string): Promise<void> {
    await this.page.selectOption(selector, value);
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
   * Get text content by role
   */
  async getTextByRole(
    role: "button" | "link" | "heading" | "textbox",
    name: string,
  ): Promise<string> {
    return (await this.page.getByRole(role, { name }).textContent()) || "";
  }

  /**
   * Check if element contains text
   */
  async assertContainsText(selector: string, text: string): Promise<void> {
    await expect(this.page.locator(selector)).toContainText(text);
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
   * Scroll to element
   */
  async scrollTo(selector: string): Promise<void> {
    await this.page.locator(selector).scrollIntoViewIfNeeded();
  }

  /**
   * Press keyboard key
   */
  async pressKey(key: string): Promise<void> {
    await this.page.keyboard.press(key);
  }

  /**
   * Hover over element
   */
  async hover(selector: string): Promise<void> {
    await this.page.hover(selector);
  }

  /**
   * Get attribute value
   */
  async getAttribute(
    selector: string,
    attribute: string,
  ): Promise<string | null> {
    return await this.page.locator(selector).getAttribute(attribute);
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

  /**
   * Take screenshot
   */
  async screenshot(path?: string): Promise<Buffer> {
    return await this.page.screenshot({ path, fullPage: true });
  }
}
