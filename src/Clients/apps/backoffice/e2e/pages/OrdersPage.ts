import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Orders Management Page Object
 * Handles /backoffice/orders and /backoffice/orders/[id]
 */
export class OrdersPage extends BasePage {
  private readonly pageHeading = 'h1:has-text("Orders Management")';
  private readonly ordersTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly viewOrderLinks = 'a[aria-label^="View order details "]';
  private readonly completeButtons = 'button[aria-label^="Complete order "]';
  private readonly cancelOrderButtons = 'button[aria-label^="Cancel order "]';
  private readonly confirmDialog = '[role="alertdialog"]';
  private readonly confirmCompleteButton =
    '[role="alertdialog"] button:has-text("Complete")';
  private readonly confirmCancelButton =
    '[role="alertdialog"] button:has-text("Cancel")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to orders page
   */
  async navigate(): Promise<void> {
    await this.goto("/orders");
  }

  /**
   * Assert orders page is loaded
   */
  async assertIsOrdersPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
    await this.assertVisible(this.ordersTable);
  }

  /**
   * Filter orders by status using StatusFilter buttons
   */
  async filterByStatus(
    status: "All Orders" | "New" | "Completed" | "Cancelled",
  ): Promise<void> {
    await this.page.getByRole("button", { name: status, exact: true }).click();
    await this.waitForPageLoad();
  }

  /**
   * Get orders count
   */
  async getOrdersCount(): Promise<number> {
    const emptyCell = this.page.locator(
      'table tbody td:has-text("No data found")',
    );
    if (await emptyCell.isVisible()) {
      return 0;
    }
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * View first order details
   */
  async viewFirstOrder(): Promise<void> {
    await this.page.locator(this.viewOrderLinks).first().click();
    await this.waitForPageLoad();
  }

  /**
   * Complete the first New order
   */
  async completeFirstOrder(): Promise<void> {
    await this.page.locator(this.completeButtons).first().click();
    await this.waitForElement(this.confirmDialog);
    await this.click(this.confirmCompleteButton);
    await this.waitForPageLoad();
  }

  /**
   * Cancel the first New order
   */
  async cancelFirstOrder(): Promise<void> {
    await this.page.locator(this.cancelOrderButtons).first().click();
    await this.waitForElement(this.confirmDialog);
    await this.click(this.confirmCancelButton);
    await this.waitForPageLoad();
  }
}
