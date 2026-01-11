import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Customers Management Page Object
 */
export class CustomersPage extends BasePage {
  // Selectors
  private readonly pageHeading = 'h1:has-text("Customers")';
  private readonly customersTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly searchInput = 'input[placeholder*="Search"]';
  private readonly viewButtons = 'button:has-text("View")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to customers page
   */
  async navigate(): Promise<void> {
    await this.goto("/ordering/customers");
  }

  /**
   * Assert customers page is loaded
   */
  async assertIsCustomersPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
    await this.assertVisible(this.customersTable);
  }

  /**
   * Search customers
   */
  async searchCustomers(query: string): Promise<void> {
    await this.fill(this.searchInput, query);
    await this.pressKey("Enter");
    await this.waitForPageLoad();
  }

  /**
   * Get customers count
   */
  async getCustomersCount(): Promise<number> {
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * View first customer details
   */
  async viewFirstCustomer(): Promise<void> {
    await this.page.locator(this.viewButtons).first().click();
    await this.waitForPageLoad();
  }
}
