import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Reviews Management Page Object
 * Handles /backoffice/reviews
 */
export class ReviewsPage extends BasePage {
  private readonly pageHeading = 'h1:has-text("Reviews Management")';
  private readonly bookSelectTrigger = "#book-select";
  private readonly reviewsTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly deleteButtons = 'button[aria-label^="Delete review "]';
  private readonly confirmDialog = '[role="alertdialog"]';
  private readonly confirmDeleteButton =
    '[role="alertdialog"] button:has-text("Delete")';
  private readonly cancelDialogButton =
    '[role="alertdialog"] button:has-text("Cancel")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to reviews page
   */
  async navigate(): Promise<void> {
    await this.goto("/reviews");
  }

  /**
   * Assert reviews page is loaded
   */
  async assertIsReviewsPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
  }

  /**
   * Select the first available book in BookSelector
   */
  async selectFirstBook(): Promise<void> {
    await this.click(this.bookSelectTrigger);
    await this.page.locator('[role="option"]').first().click();
    await this.waitForPageLoad();
  }

  /**
   * Get reviews count
   */
  async getReviewsCount(): Promise<number> {
    const tableVisible = await this.isVisible(this.reviewsTable);
    if (!tableVisible) {
      return 0;
    }
    const emptyCell = this.page.locator(
      'table tbody td:has-text("No data found")',
    );
    if (await emptyCell.isVisible()) {
      return 0;
    }
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * Delete first review in table
   */
  async deleteFirstReview(): Promise<void> {
    await this.page.locator(this.deleteButtons).first().click();
    await this.waitForElement(this.confirmDialog);
  }

  /**
   * Confirm review deletion
   */
  async confirmDelete(): Promise<void> {
    await this.click(this.confirmDeleteButton);
    await this.waitForPageLoad();
  }

  /**
   * Cancel review deletion
   */
  async cancelDelete(): Promise<void> {
    await this.click(this.cancelDialogButton);
  }
}
