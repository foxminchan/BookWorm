import { Locator, Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Account Page Object Model
 * Represents the user account page at '/account'
 */
export class AccountPage extends BasePage {
  readonly path = "/account";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get pageHeading(): Locator {
    return this.page.locator(
      'h1:has-text("My Account"), h1:has-text("Account")',
    );
  }

  get profileSection(): Locator {
    return this.page.locator(
      '[data-testid="profile-section"], section:has-text("Profile")',
    );
  }

  get ordersLink(): Locator {
    return this.page.locator(
      'a[href="/account/orders"], a:has-text("My Orders")',
    );
  }

  get addressesSection(): Locator {
    return this.page.locator(
      '[data-testid="addresses"], section:has-text("Addresses")',
    );
  }

  get settingsLink(): Locator {
    return this.page.locator('a:has-text("Settings"), a[href*="settings"]');
  }

  get logoutButton(): Locator {
    return this.page.locator(
      'button:has-text("Logout"), button:has-text("Sign Out")',
    );
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async goToOrders(): Promise<void> {
    await this.ordersLink.click();
    await this.waitForPageLoad();
  }

  async logout(): Promise<void> {
    await this.logoutButton.click();
    await this.waitForPageLoad();
  }

  // Assertions
  async isOnAccountPage(): Promise<boolean> {
    return this.page.url().includes("/account");
  }
}
