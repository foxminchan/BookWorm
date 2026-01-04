import { Locator, Page } from "@playwright/test";
import { expect } from "@playwright/test";

import { BasePage } from "./BasePage.js";

/**
 * Orders Page Object Model
 * Represents the order history/list page at '/orders' or '/account/orders'
 */
export class OrdersPage extends BasePage {
  readonly path = "/account/orders";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get pageTitle(): Locator {
    return this.page.locator('h1, [data-testid="orders-title"]');
  }

  get orderCards(): Locator {
    return this.page.locator(
      '[data-testid="order-card"], article.order, .order-card',
    );
  }

  get emptyState(): Locator {
    return this.page.locator(
      '[data-testid="empty-orders"], :has-text("No orders found"), :has-text("You haven\'t placed any orders")',
    );
  }

  get startShoppingButton(): Locator {
    return this.page.locator('a:has-text("Start Shopping"), a[href="/shop"]');
  }

  get filterDropdown(): Locator {
    return this.page.locator(
      'select[name="status"], [data-testid="order-status-filter"]',
    );
  }

  get searchInput(): Locator {
    return this.page.locator(
      'input[placeholder*="Search orders"], input[name="search"]',
    );
  }

  get clearFilterButton(): Locator {
    return this.page.locator(
      'button[aria-label="Clear order status filter"], button:has-text("Clear Filter"), button:has-text("Clear")',
    );
  }

  get sortDropdown(): Locator {
    return this.page.locator('select[name="sort"], [data-testid="order-sort"]');
  }

  get loadMoreButton(): Locator {
    return this.page.locator(
      'button:has-text("Load More"), button:has-text("Show More")',
    );
  }

  get pagination(): Locator {
    return this.page.locator(
      '[data-testid="pagination"], nav[aria-label="Pagination"]',
    );
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
    const text = await order
      .locator('[data-testid="order-id"], :has-text("Order")')
      .textContent();
    // Extract UUID from text
    const match = text?.match(
      /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i,
    );
    return match ? match[0]! : "";
  }

  async getOrderDate(index: number): Promise<string> {
    const order = this.orderCards.nth(index);
    const text = await order
      .locator('[data-testid="order-date"], time, .date')
      .textContent();
    return text?.trim() || "";
  }

  async getOrderStatus(index: number): Promise<string> {
    const order = this.orderCards.nth(index);
    const text = await order
      .locator('[data-testid="order-status"], .status-badge, .status')
      .textContent();
    return text?.trim() || "";
  }

  async getOrderTotal(index: number): Promise<number> {
    const order = this.orderCards.nth(index);
    const text = await order
      .locator('[data-testid="order-total"], .total, :has-text("Total")')
      .textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? parseFloat(match[1]!.replace(/,/g, "")) : 0;
  }

  async getOrderItemCount(index: number): Promise<number> {
    const order = this.orderCards.nth(index);
    const text = await order
      .locator('[data-testid="item-count"], :has-text("item")')
      .textContent();
    const match = text?.match(/(\d+)/);
    return match ? parseInt(match[1]!) : 0;
  }

  async clickOrder(index: number): Promise<void> {
    const order = this.orderCards.nth(index);
    await order.click();
    await this.waitForPageLoad();
  }

  async clickOrderById(orderId: string): Promise<void> {
    const order = this.page
      .locator(`[data-testid="order-card"]:has-text("${orderId}")`)
      .first();
    await order.click();
    await this.waitForPageLoad();
  }

  async viewOrderDetails(index: number): Promise<void> {
    const order = this.orderCards.nth(index);
    const viewButton = order.locator(
      'a:has-text("View Details"), button:has-text("Details")',
    );
    await viewButton.click();
    await this.waitForPageLoad();
  }

  async filterByStatus(
    status: "New" | "Completed" | "Cancelled",
  ): Promise<void> {
    await this.filterDropdown.selectOption({ label: status });
    await this.waitForPageLoad();
  }

  async searchOrders(query: string): Promise<void> {
    await this.searchInput.fill(query);
    await this.searchInput.press("Enter");
    await this.waitForPageLoad();
  }

  async sortBy(option: string): Promise<void> {
    await this.sortDropdown.selectOption({ label: option });
    await this.waitForPageLoad();
  }

  async loadMore(): Promise<void> {
    if (await this.loadMoreButton.isVisible()) {
      await this.loadMoreButton.click();
      await this.page.waitForTimeout(1000);
    }
  }

  async startShopping(): Promise<void> {
    await this.startShoppingButton.click();
    await this.waitForPageLoad();
  }

  async getOrdersByStatus(status: string): Promise<number> {
    const orders = await this.orderCards
      .locator(`:has-text("${status}")`)
      .count();
    return orders;
  }

  // Assertions
  async assertEmpty(): Promise<void> {
    await expect(this.emptyState).toBeVisible();
  }

  async assertNotEmpty(): Promise<void> {
    const count = await this.getOrdersCount();
    expect(count).toBeGreaterThan(0);
  }

  async assertOrdersCount(expectedCount: number): Promise<void> {
    const count = await this.getOrdersCount();
    expect(count).toBe(expectedCount);
  }

  async assertOrderExists(orderId: string): Promise<void> {
    const order = this.page.locator(`:has-text("${orderId}")`);
    await expect(order).toBeVisible();
  }

  async assertOrderStatus(
    index: number,
    expectedStatus: string,
  ): Promise<void> {
    const status = await this.getOrderStatus(index);
    expect(status.toLowerCase()).toContain(expectedStatus.toLowerCase());
  }
}
