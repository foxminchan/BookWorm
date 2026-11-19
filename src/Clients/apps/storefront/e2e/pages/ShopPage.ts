import { Locator, Page, expect } from "@playwright/test";

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
    return this.page.locator('ul[aria-label="Book catalog"], main');
  }

  get bookCards(): Locator {
    return this.page.locator('ul[aria-label="Book catalog"] article');
  }

  get searchButton(): Locator {
    return this.page.locator('header button[aria-label="Search books"]');
  }

  get searchInput(): Locator {
    return this.page.locator("#header-search-input, #mobile-search-input");
  }

  get sortDropdown(): Locator {
    return this.page.locator('button[aria-label="Sort options"]');
  }

  get filterSection(): Locator {
    return this.page.locator('aside[aria-label="Filters"]');
  }

  get categoryFilters(): Locator {
    return this.filterSection.locator('button[role="checkbox"]');
  }

  get priceRangeSlider(): Locator {
    return this.filterSection.locator('[role="slider"]');
  }

  get clearFiltersButton(): Locator {
    return this.page.locator(
      'button[aria-label="Clear all filters"], button:has-text("Clear Filters")',
    );
  }

  get pagination(): Locator {
    return this.page.locator('nav[aria-label="Pagination navigation"]');
  }

  get nextPageButton(): Locator {
    return this.pagination.locator('button[aria-label="Go to next page"]');
  }

  get previousPageButton(): Locator {
    return this.pagination.locator('button[aria-label="Go to previous page"]');
  }

  get emptyState(): Locator {
    return this.page.locator('h2:has-text("No Books Found")');
  }

  get loadingSkeleton(): Locator {
    return this.page.locator('output[aria-label="Loading books"]');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async search(query: string): Promise<void> {
    if (!(await this.searchInput.first().isVisible())) {
      await this.searchButton.click();
    }
    await this.searchInput.first().fill(query);
    await this.searchInput.first().press("Enter");
    await this.waitForPageLoad();
  }

  async sortBy(option: string): Promise<void> {
    await this.sortDropdown.click();
    await this.page.locator(`[role="option"]:has-text("${option}")`).click();
    await this.waitForPageLoad();
  }

  async selectCategoryFilter(categoryName: string): Promise<void> {
    const checkbox = this.filterSection
      .getByRole("checkbox", { name: categoryName })
      .first();
    await checkbox.click();
    await this.waitForPageLoad();
  }

  async selectPublisherFilter(publisherName: string): Promise<void> {
    const checkbox = this.filterSection
      .getByRole("checkbox", { name: publisherName })
      .first();
    await checkbox.click();
    await this.waitForPageLoad();
  }

  async setPriceRange(min: number, max: number): Promise<void> {
    const sliders = await this.priceRangeSlider.all();

    if (sliders.length < 2) {
      throw new Error(
        `Expected 2 price range sliders but found ${sliders.length}. ` +
          `Verify the price filter UI is properly loaded.`,
      );
    }

    const minSlider = sliders[0]!;
    const maxSlider = sliders[1]!;

    const currentMin = Number.parseInt(
      (await minSlider.getAttribute("aria-valuenow")) ?? "0",
      10,
    );
    const minSteps = min - currentMin;
    await minSlider.focus();
    const minKey = minSteps >= 0 ? "ArrowRight" : "ArrowLeft";
    for (let i = 0; i < Math.abs(minSteps); i++) {
      await this.page.keyboard.press(minKey);
    }

    const currentMax = Number.parseInt(
      (await maxSlider.getAttribute("aria-valuenow")) ?? "100",
      10,
    );
    const maxSteps = max - currentMax;
    await maxSlider.focus();
    const maxKey = maxSteps >= 0 ? "ArrowRight" : "ArrowLeft";
    for (let i = 0; i < Math.abs(maxSteps); i++) {
      await this.page.keyboard.press(maxKey);
    }

    await this.waitForPageLoad();
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
        `Book card at index ${index} not found. Total cards: ${cards.length}.`,
      );
    }
  }

  async clickBookByTitle(title: string): Promise<void> {
    const book = this.bookCards.filter({ hasText: title }).first();
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
    const pageButton = this.pagination.locator(
      `button[aria-label*="page ${pageNumber} of"]`,
    );
    await pageButton.click();
    await this.waitForPageLoad();
  }

  async getBooksCount(): Promise<number> {
    return await this.bookCards.count();
  }

  async getBookTitles(): Promise<string[]> {
    const titles = await this.bookCards.locator("h3").allTextContents();
    return titles.map((title) => title.trim());
  }

  async getBookPrices(): Promise<number[]> {
    const cards = await this.bookCards.all();
    const prices: number[] = [];
    for (const card of cards) {
      const priceText = await card
        .locator("span.font-bold")
        .first()
        .textContent();
      const cleaned = priceText?.replaceAll(/[$,]/g, "") || "0";
      prices.push(Number.parseFloat(cleaned));
    }
    return prices;
  }

  getCurrentPage(): number {
    const url = new URL(this.getCurrentUrl());
    const page = url.searchParams.get("page");
    return page ? Number.parseInt(page) : 1;
  }

  // Assertions
  async assertBooksDisplayed(): Promise<void> {
    const count = await this.getBooksCount();
    expect(count).toBeGreaterThan(0);
  }

  async assertEmptyState(): Promise<void> {
    await expect(this.emptyState).toBeVisible();
  }

  assertUrlHasFilter(filterType: string, value: string): void {
    const url = this.getCurrentUrl();
    expect(url).toContain(`${filterType}=${value}`);
  }

  assertUrlHasSearchQuery(query: string): void {
    const url = this.getCurrentUrl();
    expect(url).toContain(`search=${query}`);
  }

  async assertSortedByPrice(ascending: boolean = true): Promise<void> {
    const prices = await this.getBookPrices();
    const sorted = [...prices].sort((a, b) => (ascending ? a - b : b - a));
    expect(prices).toEqual(sorted);
  }
}
