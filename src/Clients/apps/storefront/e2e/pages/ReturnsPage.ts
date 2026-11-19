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
    return this.page
      .locator('h1:has-text("Returns"), h1:has-text("Refunds")')
      .first();
  }

  get returnPolicy(): Locator {
    return this.page
      .locator(
        'main section:has-text("Policy"), main section:has-text("Return")',
      )
      .first();
  }

  get refundProcess(): Locator {
    return this.page.locator('main :has-text("Refund")').first();
  }

  get returnTimeWindow(): Locator {
    return this.page
      .locator('main :has-text("30 days"), main :has-text("day")')
      .first();
  }

  get contactInfo(): Locator {
    return this.page
      .locator('main a[href^="mailto:"], main :has-text("contact")')
      .first();
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  // Assertions
  isOnReturnsPage(): boolean {
    return this.page.url().includes("/returns");
  }
}
