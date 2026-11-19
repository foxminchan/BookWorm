import { Locator, Page, expect } from "@playwright/test";

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
    const timestamp = Date.now();
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
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  // Locators
  get logo(): Locator {
    return this.page.locator('header a[href="/"]').first();
  }

  get searchButton(): Locator {
    return this.page.locator('header button[aria-label="Search books"]');
  }

  get searchInput(): Locator {
    return this.page.locator("#header-search-input, #mobile-search-input");
  }

  get basketIcon(): Locator {
    return this.page.locator('header a[href="/basket"]').first();
  }

  get basketCount(): Locator {
    return this.basketIcon;
  }

  get accountNav(): Locator {
    return this.page.locator('header nav[aria-label="Account"]').first();
  }

  get userIcon(): Locator {
    return this.page
      .locator('header button[aria-label="Account menu"]')
      .first();
  }

  get accountMenu(): Locator {
    return this.page.locator('[role="menu"][aria-label="Account menu"]');
  }

  // Actions
  async clickLogo(): Promise<void> {
    await this.logo.click();
  }

  async search(query: string): Promise<void> {
    if (!(await this.searchInput.first().isVisible())) {
      await this.searchButton.click();
    }
    await this.searchInput.first().fill(query);
    await this.searchInput.first().press("Enter");
  }

  async goToBasket(): Promise<void> {
    await this.basketIcon.click();
  }

  async getBasketCount(): Promise<string> {
    const text = await this.basketCount.textContent();
    const match = text?.match(/\d+/);
    return match ? match[0] : "0";
  }

  async openAccountMenu(): Promise<void> {
    await this.accountNav.hover();
    await this.accountMenu.waitFor({ state: "visible", timeout: 3000 });
  }

  async clickUserIcon(): Promise<void> {
    await this.openAccountMenu();
  }

  async isLoggedIn(): Promise<boolean> {
    try {
      await this.basketIcon.waitFor({ state: "visible", timeout: 3000 });
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
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  get backToTopButton(): Locator {
    return this.page.locator(
      'button[aria-label="Scroll to top of page"], button:has-text("Back to top")',
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
