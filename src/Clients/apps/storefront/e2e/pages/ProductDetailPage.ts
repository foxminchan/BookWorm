import { Locator, Page, expect } from "@playwright/test";

import { forEachSequentially } from "../utils/forEachSequentially";
import { BasePage } from "./BasePage";

/**
 * Product Detail Page Object Model
 * Represents individual book detail pages at '/shop/[id]'
 */
export class ProductDetailPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  // Locators
  get bookTitle(): Locator {
    return this.page.locator("main h1").first();
  }

  get bookImage(): Locator {
    return this.page.locator("main img").first();
  }

  get price(): Locator {
    return this.page.locator('[itemprop="price"]').first();
  }

  get originalPrice(): Locator {
    return this.page.locator("main span.line-through").first();
  }

  get salePrice(): Locator {
    return this.page
      .locator("main span.text-primary.text-3xl.font-bold")
      .first();
  }

  get category(): Locator {
    return this.page.locator('[itemprop="bookEdition"]').first();
  }

  get publisher(): Locator {
    return this.page.locator('[itemprop="publisher"]').first();
  }

  get authors(): Locator {
    return this.page.locator('[itemprop="author"]').first();
  }

  get rating(): Locator {
    return this.page.locator('[itemprop="aggregateRating"]').first();
  }

  get description(): Locator {
    return this.page.locator('[itemprop="description"]').first();
  }

  get stockStatus(): Locator {
    return this.page
      .locator('p:has-text("Available in Store"), p:has-text("Out of Stock")')
      .first();
  }

  get quantityInput(): Locator {
    return this.page.locator('input[aria-label="Book quantity"]');
  }

  get increaseQuantityButton(): Locator {
    return this.page.locator('button[aria-label="Increase quantity"]').first();
  }

  get decreaseQuantityButton(): Locator {
    return this.page.locator('button[aria-label="Decrease quantity"]').first();
  }

  get addToBasketButton(): Locator {
    return this.page.locator('button:has-text("Add to Basket")');
  }

  get reviewsSection(): Locator {
    return this.page
      .locator('section:has(h2:has-text("Customer Feedback"))')
      .first();
  }

  get writeReviewButton(): Locator {
    return this.page.locator(
      'button[aria-label="Write a review"], button[aria-label="Write the first review for this book"]',
    );
  }

  get reviewForm(): Locator {
    return this.page.locator('form:has(button:has-text("Submit Review"))');
  }

  get reviewStarButtons(): Locator {
    return this.reviewForm.locator("div.flex.gap-1 button");
  }

  get reviewFirstNameInput(): Locator {
    return this.reviewForm.locator('input[name="firstName"]');
  }

  get reviewLastNameInput(): Locator {
    return this.reviewForm.locator('input[name="lastName"]');
  }

  get reviewCommentInput(): Locator {
    return this.reviewForm.locator('textarea[name="comment"]');
  }

  get submitReviewButton(): Locator {
    return this.reviewForm.locator(
      'button[type="submit"]:has-text("Submit Review")',
    );
  }

  get reviewsList(): Locator {
    return this.page.locator(String.raw`div.space-y-8.lg\:col-span-2`);
  }

  get reviewItems(): Locator {
    return this.reviewsList.locator("> div.group");
  }

  get reviewSortDropdown(): Locator {
    return this.page.locator('button[aria-label="Sort reviews by"]');
  }

  get generateSummaryButton(): Locator {
    return this.page.locator('button[aria-label="Summarize reviews"]');
  }

  get reviewSummaryCard(): Locator {
    return this.page.locator('aside:has(h3:has-text("AI Review Summary"))');
  }

  get reviewPagination(): Locator {
    return this.page.locator('nav[aria-label="Pagination navigation"]');
  }

  // Actions
  async navigateToBook(bookId: string): Promise<void> {
    await this.goto(`/shop/${bookId}`);
  }

  async getTitle(): Promise<string> {
    return (await this.bookTitle.textContent()) || "";
  }

  async getPrice(): Promise<string> {
    return (await this.price.textContent()) || "";
  }

  async getSalePrice(): Promise<string | null> {
    if (await this.salePrice.isVisible()) {
      return await this.salePrice.textContent();
    }
    return null;
  }

  async getQuantity(): Promise<number> {
    if (await this.quantityInput.isVisible()) {
      const value = await this.quantityInput.inputValue();
      return Number.parseInt(value, 10) || 1;
    }
    return 0;
  }

  async setQuantity(quantity: number): Promise<void> {
    if ((await this.getQuantity()) === 0) {
      await this.addToBasket();
    }
    await this.quantityInput.fill(quantity.toString());
  }

  async increaseQuantity(times: number = 1): Promise<void> {
    await forEachSequentially(Array.from({ length: times }), async () => {
      await this.increaseQuantityButton.click();
      await this.page.waitForTimeout(200);
    });
  }

  async decreaseQuantity(times: number = 1): Promise<void> {
    await forEachSequentially(Array.from({ length: times }), async () => {
      await this.decreaseQuantityButton.click();
      await this.page.waitForTimeout(200);
    });
  }

  async addToBasket(): Promise<void> {
    await this.addToBasketButton.click();
    await this.page.waitForTimeout(1000);
  }

  async isInStock(): Promise<boolean> {
    const status = await this.stockStatus.textContent();
    return status?.includes("Available in Store") || false;
  }

  async isAddToBasketEnabled(): Promise<boolean> {
    return await this.addToBasketButton.isEnabled();
  }

  async scrollToReviews(): Promise<void> {
    await this.reviewsSection.scrollIntoViewIfNeeded();
  }

  async clickWriteReview(): Promise<void> {
    await this.writeReviewButton.first().click();
    await expect(this.reviewForm).toBeVisible();
  }

  async selectRating(stars: number): Promise<void> {
    await this.reviewStarButtons.nth(stars - 1).click();
  }

  async submitReview(
    firstName: string,
    lastName: string,
    comment: string,
    rating: number = 5,
  ): Promise<void> {
    await this.selectRating(rating);
    await this.reviewFirstNameInput.fill(firstName);
    await this.reviewLastNameInput.fill(lastName);
    await this.reviewCommentInput.fill(comment);
    await this.submitReviewButton.click();
    await this.page.waitForTimeout(2000);
  }

  async sortReviewsBy(option: string): Promise<void> {
    await this.reviewSortDropdown.click();
    await this.page.locator(`[role="menuitem"]:has-text("${option}")`).click();
    await this.waitForPageLoad();
  }

  async getReviewsCount(): Promise<number> {
    return await this.reviewItems.count();
  }

  async goToReviewPage(pageNumber: number): Promise<void> {
    const pageButton = this.reviewPagination.locator(
      `button[aria-label*="page ${pageNumber} of"]`,
    );
    await pageButton.click();
    await this.waitForPageLoad();
  }

  async generateAISummary(): Promise<void> {
    await this.generateSummaryButton.click();
    await this.page.waitForTimeout(2000);
  }

  // Assertions
  async assertBookDetailsVisible(): Promise<void> {
    await expect(this.bookTitle).toBeVisible();
    await expect(this.price).toBeVisible();
    await expect(this.bookImage).toBeVisible();
  }

  async assertInStock(): Promise<void> {
    const inStock = await this.isInStock();
    expect(inStock).toBe(true);
  }

  async assertOutOfStock(): Promise<void> {
    const inStock = await this.isInStock();
    expect(inStock).toBe(false);
    await expect(this.addToBasketButton).toBeDisabled();
  }

  async assertReviewsVisible(): Promise<void> {
    await expect(this.reviewsSection).toBeVisible();
  }

  async assertReviewSubmitted(comment: string): Promise<void> {
    const review = this.reviewItems.filter({ hasText: comment }).first();
    await expect(review).toBeVisible();
  }
}
