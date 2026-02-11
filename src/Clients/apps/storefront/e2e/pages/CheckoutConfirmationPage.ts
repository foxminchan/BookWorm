import { Locator, Page, expect } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Checkout Confirmation Page Object Model
 * Represents the order confirmation page at '/checkout/confirmation'
 */
export class CheckoutConfirmationPage extends BasePage {
  readonly path = "/checkout/confirmation";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get confirmationMessage(): Locator {
    return this.page.locator('h1, [data-testid="confirmation-message"]');
  }

  get orderIdDisplay(): Locator {
    return this.page.locator(
      '[data-testid="order-id"], :has-text("Order ID"), :has-text("Order #")',
    );
  }

  get orderStatus(): Locator {
    return this.page.locator('[data-testid="order-status"], .status-badge');
  }

  get totalAmount(): Locator {
    return this.page.locator('[data-testid="order-total"], :has-text("Total")');
  }

  get buyerName(): Locator {
    return this.page.locator('[data-testid="buyer-name"], :has-text("Name")');
  }

  get deliveryAddress(): Locator {
    return this.page.locator(
      '[data-testid="delivery-address"], :has-text("Delivery Address")',
    );
  }

  get emailConfirmationNotice(): Locator {
    return this.page.locator(
      ':has-text("email confirmation"), :has-text("confirmation email")',
    );
  }

  get viewOrderDetailsButton(): Locator {
    return this.page.locator(
      'a:has-text("View Order Details"), a:has-text("Order Details")',
    );
  }

  get backToHomeButton(): Locator {
    return this.page.locator('a:has-text("Back to Home"), a[href="/"]');
  }

  get continueShoppingLink(): Locator {
    return this.page.locator(
      'a:has-text("Continue Shopping"), a[href="/shop"]',
    );
  }

  // Actions
  async navigateWithOrderId(orderId: string): Promise<void> {
    await this.goto(`${this.path}?orderId=${orderId}`);
  }

  async getOrderId(): Promise<string> {
    const text = await this.orderIdDisplay.textContent();
    // Extract GUID from text like "Order ID: 123e4567-e89b-12d3-a456-426614174000"
    const match = text?.match(
      /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i,
    );
    return match ? match[0] : "";
  }

  async getOrderStatus(): Promise<string> {
    const text = await this.orderStatus.textContent();
    return text?.trim() || "";
  }

  async getTotalAmount(): Promise<string> {
    const text = await this.totalAmount.textContent();
    const match = text?.match(/\$[\d,]+\.?\d*/);
    return match ? match[0] : "";
  }

  async getBuyerName(): Promise<string> {
    const text = await this.buyerName.textContent();
    return text?.trim() || "";
  }

  async getDeliveryAddress(): Promise<string> {
    const text = await this.deliveryAddress.textContent();
    return text?.trim() || "";
  }

  async viewOrderDetails(): Promise<void> {
    await this.viewOrderDetailsButton.click();
    await this.waitForPageLoad();
  }

  async backToHome(): Promise<void> {
    await this.backToHomeButton.click();
    await this.waitForPageLoad();
  }

  async continueShopping(): Promise<void> {
    if (await this.continueShoppingLink.isVisible()) {
      await this.continueShoppingLink.click();
      await this.waitForPageLoad();
    }
  }

  // Assertions
  async assertConfirmationVisible(): Promise<void> {
    await expect(this.confirmationMessage).toBeVisible();
    await expect(this.orderIdDisplay).toBeVisible();
  }

  async assertOrderCreated(): Promise<void> {
    const orderId = await this.getOrderId();
    expect(orderId).toMatch(
      /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i,
    );
  }

  async assertSuccessMessage(): Promise<void> {
    const message = await this.confirmationMessage.textContent();
    expect(message?.toLowerCase()).toContain("success");
  }

  async assertEmailConfirmationNotice(): Promise<void> {
    await expect(this.emailConfirmationNotice).toBeVisible();
  }

  async assertTotalAmount(expectedAmount: string): Promise<void> {
    const total = await this.getTotalAmount();
    expect(total).toBe(expectedAmount);
  }
}
