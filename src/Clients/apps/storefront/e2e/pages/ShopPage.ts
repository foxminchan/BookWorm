import { Locator, Page } from "@playwright/test";
import { expect } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Shop Page Object Model
 * Represents the main catalog/shop page at '/shop'
 */
export class ShopPage extends BasePage {
  readonly path = "/shop";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get bookGrid(): Locator {
    return this.page.locator('[data-testid="book-grid"], .book-grid, main');
  }

  get bookCards(): Locator {
    return this.page.locator('[data-testid="book-card"], article, .book-card');
  }

  get searchInput(): Locator {
    return this.page.locator(
      'input[type="search"], input[placeholder*="Search"]',
    );
  }

  get sortDropdown(): Locator {
    return this.page.locator(
      'select[name="sort"], [data-testid="sort-select"]',
    );
  }

  get filterSection(): Locator {
    return this.page.locator('[data-testid="filters"], aside, .filters');
  }

  get categoryFilters(): Locator {
    return this.filterSection.locator(
      'input[type="checkbox"][name="category"], [data-testid*="category"]',
    );
  }

  get publisherFilters(): Locator {
    return this.filterSection.locator(
      'input[type="checkbox"][name="publisher"], [data-testid*="publisher"]',
    );
  }

  get authorFilters(): Locator {
    return this.filterSection.locator(
      'input[type="checkbox"][name="author"], [data-testid*="author"]',
    );
  }

  get priceRangeSlider(): Locator {
    return this.page.locator(
      'input[type="range"][name*="price"], [data-testid="price-slider"]',
    );
  }

  get clearFiltersButton(): Locator {
    return this.page.locator(
      'button:has-text("Clear Filters"), button:has-text("Reset")',
    );
  }

  get pagination(): Locator {
    return this.page.locator(
      '[data-testid="pagination"], nav[aria-label="Pagination"]',
    );
  }

  get nextPageButton(): Locator {
    return this.pagination.locator(
      'button:has-text("Next"), a:has-text("Next")',
    );
  }

  get previousPageButton(): Locator {
    return this.pagination.locator(
      'button:has-text("Previous"), a:has-text("Previous")',
    );
  }

  get emptyState(): Locator {
    return this.page.locator(
      '[data-testid="empty-state"], :has-text("No books found")',
    );
  }

  get loadingSkeleton(): Locator {
    return this.page.locator('[data-testid="loading-skeleton"], .skeleton');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async search(query: string): Promise<void> {
    await this.searchInput.fill(query);
    await this.searchInput.press("Enter");
    await this.waitForPageLoad();
  }

  async sortBy(option: string): Promise<void> {
    await this.sortDropdown.selectOption({ label: option });
    await this.waitForPageLoad();
  }

  async selectCategoryFilter(categoryName: string): Promise<void> {
    const filter = this.page
      .locator(`label:has-text("${categoryName}") input[type="checkbox"]`)
      .first();
    await filter.check();
    await this.waitForPageLoad();
  }

  async selectPublisherFilter(publisherName: string): Promise<void> {
    const filter = this.page
      .locator(`label:has-text("${publisherName}") input[type="checkbox"]`)
      .first();
    await filter.check();
    await this.waitForPageLoad();
  }

  async setPriceRange(min: number, max: number): Promise<void> {
    // This is simplified - actual implementation may vary based on UI
    const sliders = await this.priceRangeSlider.all();
    if (sliders.length >= 2) {
      await sliders[0]!.fill(min.toString());
      await sliders[1]!.fill(max.toString());
      await this.waitForPageLoad();
    }
  }

  async clearFilters(): Promise<void> {
    if (await this.clearFiltersButton.isVisible()) {
      await this.clearFiltersButton.click();
      await this.waitForPageLoad();
    }
  }

  async clickBook(index: number = 0): Promise<void> {
    const cards = await this.bookCards.all();
    if (cards.length > index) {
      await cards[index]!.click();
      await this.waitForPageLoad();
    } else {
      throw new Error(
        `Book card at index ${index} not found. Total cards: ${cards.length}. ` +
          `Suggestions: Verify that books are loaded before attempting to click. ` +
          `Try using clickBookByTitle() instead for more reliable selection.`,
      );
    }
  }

  async clickBookByTitle(title: string): Promise<void> {
    const book = this.page
      .locator(
        `[data-testid="book-card"]:has-text("${title}"), article:has-text("${title}")`,
      )
      .first();
    await book.click();
    await this.waitForPageLoad();
  }

  async goToNextPage(): Promise<void> {
    await this.nextPageButton.click();
    await this.waitForPageLoad();
  }

  async goToPreviousPage(): Promise<void> {
    await this.previousPageButton.click();
    await this.waitForPageLoad();
  }

  async goToPage(pageNumber: number): Promise<void> {
    const pageLink = this.pagination.locator(
      `a:has-text("${pageNumber}"), button:has-text("${pageNumber}")`,
    );
    await pageLink.click();
    await this.waitForPageLoad();
  }

  async getBooksCount(): Promise<number> {
    return await this.bookCards.count();
  }

  async getBookTitles(): Promise<string[]> {
    const titles = await this.bookCards
      .locator('h2, h3, [data-testid="book-title"]')
      .allTextContents();
    return titles.map((title) => title.trim());
  }

  async getBookPrices(): Promise<number[]> {
    const priceElements = await this.bookCards
      .locator('[data-testid="book-price"], .price')
      .allTextContents();
    return priceElements.map((price) => {
      const cleaned = price.replace(/[$,]/g, "");
      return parseFloat(cleaned);
    });
  }

  async getCurrentPage(): Promise<number> {
    const url = this.getCurrentUrl();
    const match = url.match(/[?&]page=(\d+)/);
    return match ? parseInt(match[1]!) : 1;
  }

  // Assertions
  async assertBooksDisplayed(): Promise<void> {
    const count = await this.getBooksCount();
    expect(count).toBeGreaterThan(0);
  }

  async assertEmptyState(): Promise<void> {
    await expect(this.emptyState).toBeVisible();
  }

  async assertUrlHasFilter(filterType: string, value: string): Promise<void> {
    const url = this.getCurrentUrl();
    expect(url).toContain(`${filterType}=${value}`);
  }

  async assertUrlHasSearchQuery(query: string): Promise<void> {
    const url = this.getCurrentUrl();
    expect(url).toContain(`search=${query}`);
  }

  async assertSortedByPrice(ascending: boolean = true): Promise<void> {
    const prices = await this.getBookPrices();
    const sorted = [...prices].sort((a, b) => (ascending ? a - b : b - a));
    expect(prices).toEqual(sorted);
  }
}
