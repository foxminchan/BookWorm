import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Reviews Management Page Object
 */
export class ReviewsPage extends BasePage {
  // Selectors
  private readonly pageHeading = 'h1:has-text("Reviews")';
  private readonly reviewsTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly searchInput = 'input[placeholder*="Search"]';
  private readonly approveButtons = 'button:has-text("Approve")';
  private readonly rejectButtons = 'button:has-text("Reject")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to reviews page
   */
  async navigate(): Promise<void> {
    await this.goto("/rating/reviews");
  }

  /**
   * Assert reviews page is loaded
   */
  async assertIsReviewsPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
    await this.assertVisible(this.reviewsTable);
  }

  /**
   * Search reviews
   */
  async searchReviews(query: string): Promise<void> {
    await this.fill(this.searchInput, query);
    await this.pressKey("Enter");
    await this.waitForPageLoad();
  }

  /**
   * Get reviews count
   */
  async getReviewsCount(): Promise<number> {
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * Approve first review
   */
  async approveFirstReview(): Promise<void> {
    await this.page.locator(this.approveButtons).first().click();
    await this.waitForPageLoad();
  }

  /**
   * Reject first review
   */
  async rejectFirstReview(): Promise<void> {
    await this.page.locator(this.rejectButtons).first().click();
    await this.waitForPageLoad();
  }
}
