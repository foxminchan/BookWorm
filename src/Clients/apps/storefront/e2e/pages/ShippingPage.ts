import { Locator, Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Shipping Page Object Model
 * Represents the shipping information page at '/shipping'
 */
export class ShippingPage extends BasePage {
  readonly path = "/shipping";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get pageHeading(): Locator {
    return this.page.locator('h1:has-text("Shipping")').first();
  }

  get shippingMethods(): Locator {
    return this.page
      .locator(
        'main section:has-text("Shipping"), main section:has-text("Standard")',
      )
      .first();
  }

  get deliveryTimeframes(): Locator {
    return this.page
      .locator('main :has-text("business days"), main :has-text("Delivery")')
      .first();
  }

  get shippingCosts(): Locator {
    return this.page
      .locator('main :has-text("Free Shipping"), main :has-text("$")')
      .first();
  }

  get internationalShipping(): Locator {
    return this.page.locator('main :has-text("International")').first();
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  // Assertions
  isOnShippingPage(): boolean {
    return this.page.url().includes("/shipping");
  }
}
