import { Locator, Page, expect } from "@playwright/test";

import { BasePage } from "./BasePage.js";

/**
 * Orders Page Object Model
 * Represents the order history/list page at '/account/orders'
 */
export class OrdersPage extends BasePage {
  readonly path = "/account/orders";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get pageTitle(): Locator {
    return this.page.locator('h1:has-text("Order History")');
  }

  get orderCards(): Locator {
    return this.page.locator('main a[href^="/account/orders/"]');
  }

  get emptyState(): Locator {
    return this.page.locator('h2:has-text("No Orders Found")');
  }

  get filterDropdown(): Locator {
    return this.page.locator("#order-status-filter");
  }

  get clearFilterButton(): Locator {
    return this.page.locator('button[aria-label="Clear order status filter"]');
  }

  get pagination(): Locator {
    return this.page.locator('nav[aria-label="Pagination navigation"]');
  }

  get nextPageButton(): Locator {
    return this.pagination.locator('button[aria-label="Go to next page"]');
  }

  get backToAccountLink(): Locator {
    return this.page.locator('a[href="/account"]:has-text("Back to Account")');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async getOrdersCount(): Promise<number> {
    return await this.orderCards.count();
  }

  async isEmpty(): Promise<boolean> {
    try {
      await this.emptyState.waitFor({ state: "visible", timeout: 3000 });
      return true;
    } catch {
      return false;
    }
  }

  async getOrderId(index: number): Promise<string> {
    const order = this.orderCards.nth(index);
    const text = await order.locator("h3").textContent();
    return text?.trim() || "";
  }

  async getOrderDate(index: number): Promise<string> {
    const order = this.orderCards.nth(index);
    const text = await order
      .locator('div:has(> p:text-is("Order Date")) p.font-medium')
      .textContent();
    return text?.trim() || "";
  }

  async getOrderStatus(index: number): Promise<string> {
    const order = this.orderCards.nth(index);
    const text = await order.locator("span").first().textContent();
    return text?.trim() || "";
  }

  async getOrderTotal(index: number): Promise<number> {
    const order = this.orderCards.nth(index);
    const text = await order
      .locator('div:has(> p:text-is("Total Amount")) p.font-serif')
      .textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? Number.parseFloat(match[1]!.replaceAll(",", "")) : 0;
  }

  async clickOrder(index: number): Promise<void> {
    const order = this.orderCards.nth(index);
    await order.click();
    await this.waitForPageLoad();
  }

  async clickOrderById(orderId: string): Promise<void> {
    const order = this.page
      .locator(`main a[href="/account/orders/${orderId}"]`)
      .first();
    await order.click();
    await this.waitForPageLoad();
  }

  async filterByStatus(status: string): Promise<void> {
    await this.filterDropdown.click();
    await this.page.locator(`[role="option"]:has-text("${status}")`).click();
    await this.waitForPageLoad();
  }

  async clearFilter(): Promise<void> {
    if (await this.clearFilterButton.isVisible()) {
      await this.clearFilterButton.click();
      await this.waitForPageLoad();
    }
  }

  async goToPage(pageNumber: number): Promise<void> {
    const pageButton = this.pagination.locator(
      `button[aria-label*="page ${pageNumber} of"]`,
    );
    await pageButton.click();
    await this.waitForPageLoad();
  }

  // Assertions
  async assertOrdersVisible(): Promise<void> {
    await expect(this.pageTitle).toBeVisible();
  }

  async assertEmpty(): Promise<void> {
    await expect(this.emptyState).toBeVisible();
  }

  async assertNotEmpty(): Promise<void> {
    const count = await this.getOrdersCount();
    expect(count).toBeGreaterThan(0);
  }

  async assertOrderExists(orderId: string): Promise<void> {
    const order = this.orderCards.filter({ hasText: orderId }).first();
    await expect(order).toBeVisible();
  }
}
