import { Locator, Page } from "@playwright/test";
import { expect } from "@playwright/test";

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
    return this.page.locator('h1, [data-testid="book-title"]');
  }

  get bookImage(): Locator {
    return this.page
      .locator('img[alt*="book"], [data-testid="book-image"]')
      .first();
  }

  get price(): Locator {
    return this.page.locator('[data-testid="book-price"], .price').first();
  }

  get salePrice(): Locator {
    return this.page.locator('[data-testid="sale-price"], .sale-price');
  }

  get saleBadge(): Locator {
    return this.page.locator(
      '[data-testid="sale-badge"], .sale-badge, :has-text("Sale")',
    );
  }

  get category(): Locator {
    return this.page.locator(
      '[data-testid="book-category"], :has-text("Category")',
    );
  }

  get publisher(): Locator {
    return this.page.locator(
      '[data-testid="book-publisher"], :has-text("Publisher")',
    );
  }

  get authors(): Locator {
    return this.page.locator(
      '[data-testid="book-authors"], :has-text("Author")',
    );
  }

  get rating(): Locator {
    return this.page.locator(
      '[data-testid="book-rating"], .rating, [aria-label*="rating"]',
    );
  }

  get description(): Locator {
    return this.page.locator(
      '[data-testid="book-description"], .description, p',
    );
  }

  get stockStatus(): Locator {
    return this.page.locator(
      '[data-testid="stock-status"], :has-text("In Stock"), :has-text("Out of Stock")',
    );
  }

  get quantityInput(): Locator {
    return this.page.locator(
      'input[type="number"][name="quantity"], [data-testid="quantity-input"]',
    );
  }

  get increaseQuantityButton(): Locator {
    return this.page.locator(
      'button[aria-label*="Increase"], button:has-text("+")',
    );
  }

  get decreaseQuantityButton(): Locator {
    return this.page.locator(
      'button[aria-label*="Decrease"], button:has-text("-")',
    );
  }

  get addToBasketButton(): Locator {
    return this.page.locator(
      'button:has-text("Add to Basket"), button:has-text("Add to Cart")',
    );
  }

  get reviewsSection(): Locator {
    return this.page.locator(
      '[data-testid="reviews-section"], section:has-text("Reviews")',
    );
  }

  get writeReviewButton(): Locator {
    return this.page.locator(
      'button:has-text("Write a Review"), button:has-text("Add Review")',
    );
  }

  get reviewForm(): Locator {
    return this.page.locator('[data-testid="review-form"], form');
  }

  get reviewFirstNameInput(): Locator {
    return this.reviewForm.locator(
      'input[name="firstName"], input[placeholder*="First"]',
    );
  }

  get reviewLastNameInput(): Locator {
    return this.reviewForm.locator(
      'input[name="lastName"], input[placeholder*="Last"]',
    );
  }

  get reviewCommentInput(): Locator {
    return this.reviewForm.locator(
      'textarea[name="comment"], textarea[placeholder*="comment"]',
    );
  }

  get submitReviewButton(): Locator {
    return this.reviewForm.locator(
      'button[type="submit"], button:has-text("Submit")',
    );
  }

  get reviewsList(): Locator {
    return this.page.locator('[data-testid="reviews-list"], .reviews-list');
  }

  get reviewItems(): Locator {
    return this.reviewsList.locator(
      '[data-testid="review-item"], article, .review-item',
    );
  }

  get reviewSortDropdown(): Locator {
    return this.page.locator(
      'select[name="reviewSort"], [data-testid="review-sort"]',
    );
  }

  get generateSummaryButton(): Locator {
    return this.page.locator(
      'button:has-text("Generate Summary"), button:has-text("AI Summary")',
    );
  }

  get reviewPagination(): Locator {
    return this.reviewsSection.locator('[data-testid="pagination"], nav');
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
    const value = await this.quantityInput.inputValue();
    return parseInt(value) || 1;
  }

  async setQuantity(quantity: number): Promise<void> {
    await this.quantityInput.fill(quantity.toString());
  }

  async increaseQuantity(times: number = 1): Promise<void> {
    for (let i = 0; i < times; i++) {
      await this.increaseQuantityButton.click();
      await this.page.waitForTimeout(200); // Small delay between clicks
    }
  }

  async decreaseQuantity(times: number = 1): Promise<void> {
    for (let i = 0; i < times; i++) {
      await this.decreaseQuantityButton.click();
      await this.page.waitForTimeout(200);
    }
  }

  async addToBasket(): Promise<void> {
    await this.addToBasketButton.click();
    // Wait for potential success notification or basket update
    await this.page.waitForTimeout(1000);
  }

  async isInStock(): Promise<boolean> {
    const status = await this.stockStatus.textContent();
    return status?.toLowerCase().includes("in stock") || false;
  }

  async isAddToBasketEnabled(): Promise<boolean> {
    return await this.addToBasketButton.isEnabled();
  }

  async scrollToReviews(): Promise<void> {
    await this.reviewsSection.scrollIntoViewIfNeeded();
  }

  async clickWriteReview(): Promise<void> {
    await this.writeReviewButton.click();
    await expect(this.reviewForm).toBeVisible();
  }

  async submitReview(
    firstName: string,
    lastName: string,
    comment: string,
    rating: number = 5,
  ): Promise<void> {
    // Select star rating
    const starButton = this.reviewForm.locator(
      `button[aria-label*="${rating} star"]`,
    );
    await starButton.click();

    // Fill form
    await this.reviewFirstNameInput.fill(firstName);
    await this.reviewLastNameInput.fill(lastName);
    await this.reviewCommentInput.fill(comment);

    // Submit
    await this.submitReviewButton.click();
    await this.page.waitForTimeout(2000); // Wait for submission
  }

  async sortReviewsBy(option: string): Promise<void> {
    await this.reviewSortDropdown.selectOption({ label: option });
    await this.waitForPageLoad();
  }

  async getReviewsCount(): Promise<number> {
    return await this.reviewItems.count();
  }

  async goToReviewPage(pageNumber: number): Promise<void> {
    const pageLink = this.reviewPagination.locator(
      `a:has-text("${pageNumber}"), button:has-text("${pageNumber}")`,
    );
    await pageLink.click();
    await this.waitForPageLoad();
  }

  async generateAISummary(): Promise<void> {
    await this.generateSummaryButton.click();
    await this.page.waitForTimeout(2000); // Wait for AI generation
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
