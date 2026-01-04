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
    return this.page.locator('h1:has-text("Shipping")');
  }

  get shippingMethods(): Locator {
    return this.page.locator(
      '[data-testid="shipping-methods"], section:has-text("Methods")',
    );
  }

  get deliveryTimeframes(): Locator {
    return this.page.locator(
      '[data-testid="delivery-timeframes"], :has-text("Delivery Time")',
    );
  }

  get shippingCosts(): Locator {
    return this.page.locator(
      '[data-testid="shipping-costs"], :has-text("Shipping Cost")',
    );
  }

  get internationalShipping(): Locator {
    return this.page.locator(
      '[data-testid="international-shipping"], :has-text("International")',
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
  async isOnShippingPage(): Promise<boolean> {
    return this.page.url().includes("/shipping");
  }
}
