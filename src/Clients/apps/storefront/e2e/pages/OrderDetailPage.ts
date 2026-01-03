import { Locator, Page } from "@playwright/test";
import { expect } from "@playwright/test";

import { BasePage } from "./BasePage.js";

/**
 * Order Detail Page Object Model
 * Represents the individual order details page at '/orders/{orderId}'
 */
export class OrderDetailPage extends BasePage {
  readonly pathPattern = "/account/orders/";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get pageTitle(): Locator {
    return this.page.locator('h1, [data-testid="order-title"]');
  }

  get orderIdDisplay(): Locator {
    return this.page.locator(
      '[data-testid="order-id"], :has-text("Order ID"), :has-text("Order #")',
    );
  }

  get orderStatus(): Locator {
    return this.page.locator(
      '[data-testid="order-status"], .status-badge, .status',
    );
  }

  get orderDate(): Locator {
    return this.page.locator('[data-testid="order-date"], time, .date');
  }

  get buyerInfo(): Locator {
    return this.page.locator(
      '[data-testid="buyer-info"], .buyer-details, :has-text("Customer")',
    );
  }

  get buyerName(): Locator {
    return this.buyerInfo.locator('[data-testid="buyer-name"], .name');
  }

  get deliveryAddress(): Locator {
    return this.page.locator(
      '[data-testid="delivery-address"], .address, :has-text("Delivery Address")',
    );
  }

  get orderItems(): Locator {
    return this.page.locator(
      '[data-testid="order-item"], .order-item, article.item',
    );
  }

  get subtotalAmount(): Locator {
    return this.page.locator('[data-testid="subtotal"], :has-text("Subtotal")');
  }

  get shippingAmount(): Locator {
    return this.page.locator('[data-testid="shipping"], :has-text("Shipping")');
  }

  get totalAmount(): Locator {
    return this.page.locator('[data-testid="total"], :has-text("Total")');
  }

  get cancelOrderButton(): Locator {
    return this.page.locator(
      'button:has-text("Cancel Order"), button:has-text("Cancel")',
    );
  }

  get confirmCancelButton(): Locator {
    return this.page.locator(
      '[role="dialog"] button:has-text("Confirm"), [data-testid="confirm-cancel"]',
    );
  }

  get reorderButton(): Locator {
    return this.page.locator(
      'button:has-text("Reorder"), button:has-text("Order Again")',
    );
  }

  get backToOrdersButton(): Locator {
    return this.page.locator(
      'a:has-text("Back to Orders"), a[href="/account/orders"]',
    );
  }

  get trackingInfo(): Locator {
    return this.page.locator(
      '[data-testid="tracking-info"], .tracking, :has-text("Tracking")',
    );
  }

  get downloadInvoiceButton(): Locator {
    return this.page.locator(
      'button:has-text("Download Invoice"), a:has-text("Invoice")',
    );
  }

  // Actions
  async navigateToOrder(orderId: string): Promise<void> {
    await this.goto(`${this.pathPattern}${orderId}`);
  }

  async getOrderId(): Promise<string> {
    const text = await this.orderIdDisplay.textContent();
    // Extract UUID
    const match = text?.match(
      /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i,
    );
    return match ? match[0] : "";
  }

  async getOrderStatus(): Promise<string> {
    const text = await this.orderStatus.textContent();
    return text?.trim() || "";
  }

  async getOrderDate(): Promise<string> {
    const text = await this.orderDate.textContent();
    return text?.trim() || "";
  }

  async getBuyerName(): Promise<string> {
    const text = await this.buyerName.textContent();
    return text?.trim() || "";
  }

  async getDeliveryAddress(): Promise<string> {
    const text = await this.deliveryAddress.textContent();
    return text?.trim() || "";
  }

  async getItemsCount(): Promise<number> {
    return await this.orderItems.count();
  }

  async getItemTitle(index: number): Promise<string> {
    const item = this.orderItems.nth(index);
    const title = await item
      .locator('h3, h4, [data-testid="item-title"]')
      .textContent();
    return title?.trim() || "";
  }

  async getItemQuantity(index: number): Promise<number> {
    const item = this.orderItems.nth(index);
    const text = await item
      .locator('[data-testid="quantity"], :has-text("Qty")')
      .textContent();
    const match = text?.match(/(\d+)/);
    return match ? parseInt(match[1]!) : 0;
  }

  async getItemPrice(index: number): Promise<number> {
    const item = this.orderItems.nth(index);
    const text = await item
      .locator('[data-testid="price"], .price')
      .textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? parseFloat(match[1]!.replace(/,/g, "")) : 0;
  }

  async getSubtotal(): Promise<number> {
    const text = await this.subtotalAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? parseFloat(match[1]!.replace(/,/g, "")) : 0;
  }

  async getShipping(): Promise<number> {
    const text = await this.shippingAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? parseFloat(match[1]!.replace(/,/g, "")) : 0;
  }

  async getTotal(): Promise<number> {
    const text = await this.totalAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? parseFloat(match[1]!.replace(/,/g, "")) : 0;
  }

  async cancelOrder(): Promise<void> {
    await this.cancelOrderButton.click();
    await expect(this.confirmCancelButton).toBeVisible();
    await this.confirmCancelButton.click();
    await this.page.waitForTimeout(1000);
  }

  async reorder(): Promise<void> {
    await this.reorderButton.click();
    await this.waitForPageLoad();
  }

  async backToOrders(): Promise<void> {
    await this.backToOrdersButton.click();
    await this.waitForPageLoad();
  }

  async clickItemByTitle(title: string): Promise<void> {
    const item = this.page
      .locator(`[data-testid="order-item"]:has-text("${title}")`)
      .first();
    await item.click();
    await this.waitForPageLoad();
  }

  async downloadInvoice(): Promise<void> {
    const downloadPromise = this.page.waitForEvent("download");
    await this.downloadInvoiceButton.click();
    const download = await downloadPromise;
    // Optionally save the file
    await download.saveAs(`./e2e/reports/invoice-${Date.now()}.pdf`);
  }

  async hasTrackingInfo(): Promise<boolean> {
    return await this.trackingInfo.isVisible();
  }

  // Assertions
  async assertOrderVisible(): Promise<void> {
    await expect(this.orderIdDisplay).toBeVisible();
    await expect(this.orderStatus).toBeVisible();
  }

  async assertOrderStatus(expectedStatus: string): Promise<void> {
    const status = await this.getOrderStatus();
    expect(status.toLowerCase()).toContain(expectedStatus.toLowerCase());
  }

  async assertItemsCount(expectedCount: number): Promise<void> {
    const count = await this.getItemsCount();
    expect(count).toBe(expectedCount);
  }

  async assertTotalAmount(expectedTotal: number): Promise<void> {
    const total = await this.getTotal();
    expect(Math.abs(total - expectedTotal)).toBeLessThan(0.01);
  }

  async assertBuyerName(expectedName: string): Promise<void> {
    const name = await this.getBuyerName();
    expect(name).toContain(expectedName);
  }

  async assertCancelButtonVisible(): Promise<void> {
    await expect(this.cancelOrderButton).toBeVisible();
  }

  async assertCancelButtonNotVisible(): Promise<void> {
    await expect(this.cancelOrderButton).not.toBeVisible();
  }
}
