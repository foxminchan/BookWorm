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
    return this.page.locator('h1:has-text("Order Confirmed")');
  }

  get orderIdDisplay(): Locator {
    return this.page.locator('p:has-text("#")').first();
  }

  get orderStatus(): Locator {
    return this.page
      .locator('div:has(> h3:has-text("Order Status")) span')
      .first();
  }

  get totalAmount(): Locator {
    return this.page
      .locator('div:has(> h3:has-text("Order Total")) p.text-primary')
      .first();
  }

  get buyerName(): Locator {
    return this.page
      .locator('div:has(> h3:has-text("Shipping Address")) p.font-medium')
      .first();
  }

  get deliveryAddress(): Locator {
    return this.page
      .locator('div:has(> h3:has-text("Shipping Address"))')
      .first();
  }

  get viewOrderDetailsButton(): Locator {
    return this.page.locator('a:has-text("View Order")');
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
    const url = new URL(this.page.url());
    const queryOrderId = url.searchParams.get("orderId");
    if (queryOrderId) return queryOrderId;

    const text = await this.orderIdDisplay.textContent();
    const match = text?.match(/#([0-9A-F]{8})/i);
    return match?.[1] ?? "";
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
    const text = await this.orderIdDisplay.textContent();
    expect(text).toMatch(/#[0-9A-F]{8}/i);
  }

  async assertSuccessMessage(): Promise<void> {
    await expect(this.confirmationMessage).toBeVisible();
  }

  async assertTotalAmount(expectedAmount: string): Promise<void> {
    const total = await this.getTotalAmount();
    expect(total).toBe(expectedAmount);
  }
}
