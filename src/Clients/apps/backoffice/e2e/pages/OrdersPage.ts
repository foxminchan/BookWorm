import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Orders Management Page Object
 */
export class OrdersPage extends BasePage {
  // Selectors
  private readonly pageHeading = 'h1:has-text("Orders")';
  private readonly ordersTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly searchInput = 'input[placeholder*="Search"]';
  private readonly filterButton = 'button:has-text("Filter")';
  private readonly statusFilter = 'select[name="status"]';
  private readonly viewButtons = 'button:has-text("View")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to orders page
   */
  async navigate(): Promise<void> {
    await this.goto("/ordering/orders");
  }

  /**
   * Assert orders page is loaded
   */
  async assertIsOrdersPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
    await this.assertVisible(this.ordersTable);
  }

  /**
   * Search orders
   */
  async searchOrders(query: string): Promise<void> {
    await this.fill(this.searchInput, query);
    await this.pressKey("Enter");
    await this.waitForPageLoad();
  }

  /**
   * Filter orders by status
   */
  async filterByStatus(status: string): Promise<void> {
    await this.select(this.statusFilter, status);
    await this.waitForPageLoad();
  }

  /**
   * Get orders count
   */
  async getOrdersCount(): Promise<number> {
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * View first order details
   */
  async viewFirstOrder(): Promise<void> {
    await this.page.locator(this.viewButtons).first().click();
    await this.waitForPageLoad();
  }
}
