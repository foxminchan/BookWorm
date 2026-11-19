import { Locator, Page, expect } from "@playwright/test";

import { BasePage } from "./BasePage.js";

/**
 * Order Detail Page Object Model
 * Represents the individual order details page at '/account/orders/{orderId}'
 */
export class OrderDetailPage extends BasePage {
  readonly pathPattern = "/account/orders/";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get pageTitle(): Locator {
    return this.page.locator("main h1").first();
  }

  get orderStatus(): Locator {
    return this.page.locator("main div.mb-8 div.flex span").first();
  }

  get orderDate(): Locator {
    return this.page.locator('main p:has-text("Placed on")').first();
  }

  get orderItemsSection(): Locator {
    return this.page.locator(
      'div:has(> div > div > h2:has-text("Order Items"))',
    );
  }

  get orderItems(): Locator {
    return this.orderItemsSection.locator("> div.divide-y > div");
  }

  get orderTimeline(): Locator {
    return this.page.locator('div:has(> h2:has-text("Order Timeline"))');
  }

  get orderSummarySection(): Locator {
    return this.page.locator('div:has(> h2:has-text("Order Summary"))');
  }

  get subtotalAmount(): Locator {
    return this.orderSummarySection.locator(
      'div:has(> span:text-is("Subtotal"))',
    );
  }

  get shippingAmount(): Locator {
    return this.orderSummarySection.locator(
      'div:has(> span:text-is("Shipping"))',
    );
  }

  get totalAmount(): Locator {
    return this.orderSummarySection.locator('div:has(> span:text-is("Total"))');
  }

  get backToOrdersButton(): Locator {
    return this.page.locator(
      'a[href="/account/orders"]:has-text("Back to Orders")',
    );
  }

  // Actions
  async navigateToOrder(orderId: string): Promise<void> {
    await this.goto(`${this.pathPattern}${orderId}`);
  }

  async getOrderId(): Promise<string> {
    const text = await this.pageTitle.textContent();
    const match = text?.match(/Order\s+(.+)/i);
    return match ? match[1]!.trim() : "";
  }

  async getOrderStatus(): Promise<string> {
    const text = await this.orderStatus.textContent();
    return text?.trim() || "";
  }

  async getOrderDate(): Promise<string> {
    const text = await this.orderDate.textContent();
    return text?.trim() || "";
  }

  async getItemsCount(): Promise<number> {
    return await this.orderItems.count();
  }

  async getItemTitle(index: number): Promise<string> {
    const item = this.orderItems.nth(index);
    const title = await item.locator("h3").textContent();
    return title?.trim() || "";
  }

  async getItemQuantity(index: number): Promise<number> {
    const item = this.orderItems.nth(index);
    const text = await item.locator('p:has-text("Quantity:")').textContent();
    const match = text?.match(/Quantity:\s*(\d+)/i);
    return match ? Number.parseInt(match[1] ?? "0", 10) : 0;
  }

  async getItemPrice(index: number): Promise<number> {
    const item = this.orderItems.nth(index);
    const text = await item
      .locator("div.text-right p.font-semibold")
      .textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? Number.parseFloat(match[1]!.replaceAll(",", "")) : 0;
  }

  async getSubtotal(): Promise<number> {
    const text = await this.subtotalAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? Number.parseFloat(match[1]!.replaceAll(",", "")) : 0;
  }

  async getShippingText(): Promise<string> {
    const text = await this.shippingAmount.textContent();
    return text?.trim() || "";
  }

  async getTotal(): Promise<number> {
    const text = await this.totalAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? Number.parseFloat(match[1]!.replaceAll(",", "")) : 0;
  }

  async goBackToOrders(): Promise<void> {
    await this.backToOrdersButton.click();
    await this.waitForPageLoad();
  }

  // Assertions
  async assertOrderVisible(): Promise<void> {
    await expect(this.pageTitle).toBeVisible();
    await expect(this.orderStatus).toBeVisible();
  }

  async assertOrderStatus(expectedStatus: string): Promise<void> {
    const status = await this.getOrderStatus();
    expect(status.toLowerCase()).toContain(expectedStatus.toLowerCase());
  }
}
