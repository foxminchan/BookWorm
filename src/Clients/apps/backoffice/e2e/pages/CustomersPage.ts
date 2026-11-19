import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Customers Management Page Object
 * Handles /backoffice/customers
 */
export class CustomersPage extends BasePage {
  private readonly pageHeading = 'h1:has-text("Customers Management")';
  private readonly customersTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly deleteButtons = 'button[aria-label^="Delete "]';
  private readonly confirmDialog = '[role="alertdialog"]';
  private readonly confirmDeleteButton =
    '[role="alertdialog"] button:has-text("Delete")';
  private readonly cancelDialogButton =
    '[role="alertdialog"] button:has-text("Cancel")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to customers page
   */
  async navigate(): Promise<void> {
    await this.goto("/customers");
  }

  /**
   * Assert customers page is loaded
   */
  async assertIsCustomersPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
    await this.assertVisible(this.customersTable);
  }

  /**
   * Get customers count
   */
  async getCustomersCount(): Promise<number> {
    const emptyCell = this.page.locator(
      'table tbody td:has-text("No data found")',
    );
    if (await emptyCell.isVisible()) {
      return 0;
    }
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * Click delete button on the first customer
   */
  async deleteFirstCustomer(): Promise<void> {
    await this.page.locator(this.deleteButtons).first().click();
    await this.waitForElement(this.confirmDialog);
  }

  /**
   * Confirm customer deletion
   */
  async confirmDelete(): Promise<void> {
    await this.click(this.confirmDeleteButton);
    await this.waitForPageLoad();
  }

  /**
   * Cancel customer deletion
   */
  async cancelDelete(): Promise<void> {
    await this.click(this.cancelDialogButton);
  }
}
