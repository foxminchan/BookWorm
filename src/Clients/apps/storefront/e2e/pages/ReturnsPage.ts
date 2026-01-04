import { Locator, Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Returns Page Object Model
 * Represents the returns and refunds page at '/returns'
 */
export class ReturnsPage extends BasePage {
  readonly path = "/returns";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get pageHeading(): Locator {
    return this.page.locator('h1:has-text("Returns"), h1:has-text("Refunds")');
  }

  get returnPolicy(): Locator {
    return this.page.locator(
      '[data-testid="return-policy"], section:has-text("Policy")',
    );
  }

  get refundProcess(): Locator {
    return this.page.locator(
      '[data-testid="refund-process"], :has-text("Refund")',
    );
  }

  get returnTimeWindow(): Locator {
    return this.page.locator(':has-text("30 days"), :has-text("day")');
  }

  get contactInfo(): Locator {
    return this.page.locator(
      '[data-testid="contact-info"], :has-text("contact")',
    );
  }

  get breadcrumbs(): Locator {
    return this.page.locator(
      'nav[aria-label="Breadcrumb"], [data-testid="breadcrumbs"]',
    );
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  // Assertions
  async isOnReturnsPage(): Promise<boolean> {
    return this.page.url().includes("/returns");
  }
}
