import { Locator, Page } from "@playwright/test";
import { expect } from "@playwright/test";

/**
 * Base Page Object with common functionality
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
   * Wait for selector
   */
  async waitForSelector(selector: string, timeout = 10000): Promise<void> {
    await this.page.waitForSelector(selector, { state: "visible", timeout });
  }

  /**
   * Click element
   */
  async click(selector: string): Promise<void> {
    await this.page.click(selector);
  }

  /**
   * Fill input field
   */
  async fill(selector: string, text: string): Promise<void> {
    await this.page.fill(selector, text);
  }

  /**
   * Get text content
   */
  async getTextContent(selector: string): Promise<string> {
    const element = await this.page.waitForSelector(selector);
    const text = await element?.textContent();
    return text?.trim() || "";
  }

  /**
   * Check if element is visible
   */
  async isVisible(selector: string): Promise<boolean> {
    try {
      await this.page.waitForSelector(selector, {
        state: "visible",
        timeout: 3000,
      });
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Get element count
   */
  async getElementCount(selector: string): Promise<number> {
    return await this.page.locator(selector).count();
  }

  /**
   * Scroll to element
   */
  async scrollToElement(selector: string): Promise<void> {
    await this.page.locator(selector).scrollIntoViewIfNeeded();
  }

  /**
   * Take screenshot
   */
  async screenshot(name: string): Promise<void> {
    const timestamp = new Date().getTime();
    await this.page.screenshot({
      path: `e2e/reports/screenshots/${name}-${timestamp}.png`,
      fullPage: true,
    });
  }

  /**
   * Get header component
   */
  get header(): HeaderComponent {
    return new HeaderComponent(this.page);
  }

  /**
   * Get footer component
   */
  get footer(): FooterComponent {
    return new FooterComponent(this.page);
  }
}

/**
 * Header Component - Appears on all pages
 */
export class HeaderComponent {
  private page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  // Locators
  get logo(): Locator {
    return this.page.locator('header a[href="/"]').first();
  }

  get searchInput(): Locator {
    return this.page.locator(
      'input[type="search"], input[placeholder*="Search"]',
    );
  }

  get basketIcon(): Locator {
    return this.page.locator('a[href="/basket"]');
  }

  get basketCount(): Locator {
    return this.page.locator(
      'a[href="/basket"] span, [data-testid="basket-count"]',
    );
  }

  get userIcon(): Locator {
    return this.page
      .locator(
        'button:has-text("Account"), button[aria-label*="User"], button[aria-label*="Account"]',
      )
      .first();
  }

  // Actions
  async clickLogo(): Promise<void> {
    await this.logo.click();
  }

  async search(query: string): Promise<void> {
    await this.searchInput.fill(query);
    await this.searchInput.press("Enter");
  }

  async goToBasket(): Promise<void> {
    await this.basketIcon.click();
  }

  async getBasketCount(): Promise<string> {
    const text = await this.basketCount.textContent();
    return text?.trim() || "0";
  }

  async clickUserIcon(): Promise<void> {
    await this.userIcon.click();
  }

  async isLoggedIn(): Promise<boolean> {
    try {
      await this.userIcon.waitFor({ state: "visible", timeout: 3000 });
      return true;
    } catch {
      return false;
    }
  }
}

/**
 * Footer Component - Appears on all pages
 */
export class FooterComponent {
  private page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  get backToTopButton(): Locator {
    return this.page.locator(
      'button:has-text("Back to top"), [aria-label="Back to top"]',
    );
  }

  async clickBackToTop(): Promise<void> {
    await this.backToTopButton.click();
  }

  async getFooterLinks(): Promise<string[]> {
    const links = await this.page.locator("footer a").allTextContents();
    return links.map((link) => link.trim());
  }
}
