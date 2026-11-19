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
    return this.page.locator('h1:has-text("Your Account")');
  }

  get profileSection(): Locator {
    return this.page.locator("main h3").first();
  }

  get deliveryAddressSection(): Locator {
    return this.page.locator('div:has(> div h3:has-text("Delivery Address"))');
  }

  get editAddressButton(): Locator {
    return this.deliveryAddressSection.locator('button:has-text("Edit")');
  }

  get streetInput(): Locator {
    return this.page.locator('input[name="street"]');
  }

  get cityInput(): Locator {
    return this.page.locator('input[name="city"]');
  }

  get provinceInput(): Locator {
    return this.page.locator('input[name="province"]');
  }

  get saveAddressButton(): Locator {
    return this.page.locator('button[type="submit"]:has-text("Save Address")');
  }

  get cancelEditButton(): Locator {
    return this.page.locator('button:has-text("Cancel")');
  }

  get ordersLink(): Locator {
    return this.page.locator('a[href="/account/orders"]');
  }

  get logoutButton(): Locator {
    return this.page.locator('main button:has-text("Logout")');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async openEditAddress(): Promise<void> {
    await this.editAddressButton.click();
  }

  async fillAddress(
    street: string,
    city: string,
    province: string,
  ): Promise<void> {
    await this.streetInput.fill(street);
    await this.cityInput.fill(city);
    await this.provinceInput.fill(province);
  }

  async saveAddress(): Promise<void> {
    await this.saveAddressButton.click();
    await this.waitForPageLoad();
  }

  async cancelEditAddress(): Promise<void> {
    await this.cancelEditButton.click();
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
  isOnAccountPage(): boolean {
    return this.page.url().includes("/account");
  }
}
