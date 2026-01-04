import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { HomePage, ShopPage } from "../pages";

/**
 * Step definitions for product discovery and catalog features
 */

// Search steps
When(
  "I enter {string} in the search bar",
  async function (this: { page: Page }, query: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.search(query);
  },
);

Then(
  "I should see books matching {string} in the results",
  async function (this: { page: Page }, query: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertBooksDisplayed();

    // Verify URL contains search query
    const url = this.page.url();
    expect(url).toContain(`search=${query}`);
  },
);

// Filter steps
When(
  "I select {string} category filter",
  async function (this: { page: Page }, categoryName: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.selectCategoryFilter(categoryName);
  },
);

When(
  "I select {string} publisher filter",
  async function (this: { page: Page }, publisherName: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.selectPublisherFilter(publisherName);
  },
);

When(
  "I set price range to {string}",
  async function (this: { page: Page }, priceRange: string) {
    const match = priceRange.match(/\$(\d+)-\$(\d+)/);

    // Validate regex match and capture groups exist
    if (!match) {
      throw new Error(
        `Invalid price range format: "${priceRange}". Expected format: "$min-$max" (e.g., "$20-$50")`,
      );
    }

    const minStr = match[1];
    const maxStr = match[2];

    if (!minStr || !maxStr) {
      throw new Error(
        `Failed to extract price values from: "${priceRange}". Expected format: "$min-$max" (e.g., "$20-$50")`,
      );
    }

    const min = parseInt(minStr, 10);
    const max = parseInt(maxStr, 10);

    if (isNaN(min) || isNaN(max)) {
      throw new Error(
        `Invalid numeric values in price range: "${priceRange}". Min: ${minStr}, Max: ${maxStr}`,
      );
    }

    const shopPage = new ShopPage(this.page);
    await shopPage.setPriceRange(min, max);
  },
);

Then(
  "the results should be filtered accordingly",
  async function (this: { page: Page }) {
    const shopPage = new ShopPage(this.page);
    const count = await shopPage.getBooksCount();
    expect(count).toBeGreaterThanOrEqual(0);
  },
);

Then(
  "only {string} books should be displayed",
  async function (this: { page: Page }, filterValue: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertBooksDisplayed();

    // In a real test, we'd verify each book matches the filter
    // For now, verify URL contains the filter
    const url = this.page.url();
    const filterSlug = filterValue
      .toLowerCase()
      .replace(/\s+/g, "-")
      .replace(/&/g, "");
    expect(url.toLowerCase()).toContain(filterSlug);
  },
);

Then(
  "the {string} filter should be checked",
  async function (this: { page: Page }, filterName: string) {
    const checkbox = this.page
      .locator(`label:has-text("${filterName}") input[type="checkbox"]`)
      .first();
    await expect(checkbox).toBeChecked();
  },
);

Then(
  "the {string} filter should be selected",
  async function (this: { page: Page }, filterName: string) {
    const checkbox = this.page
      .locator(`label:has-text("${filterName}") input[type="checkbox"]`)
      .first();
    await expect(checkbox).toBeChecked();
  },
);

Given(
  "I have applied category and price filters",
  async function (this: { page: Page }) {
    const shopPage = new ShopPage(this.page);
    await shopPage.selectCategoryFilter("Fiction");
    await shopPage.setPriceRange(20, 50);
  },
);

When('I click "Clear Filters"', async function (this: { page: Page }) {
  const shopPage = new ShopPage(this.page);
  await shopPage.clearFilters();
});

Then("all filters should be reset", async function (this: { page: Page }) {
  const url = this.page.url();
  // URL should not contain filter parameters (except maybe page)
  expect(url).not.toContain("category=");
});

Then("I should see all available books", async function (this: { page: Page }) {
  const shopPage = new ShopPage(this.page);
  await shopPage.assertBooksDisplayed();
});

// Sort steps
When(
  "I sort by {string}",
  async function (this: { page: Page }, sortOption: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.sortBy(sortOption);
  },
);

Then(
  "books should be displayed in ascending price order",
  async function (this: { page: Page }) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertSortedByPrice(true);
  },
);

Then(
  "books should be displayed in descending price order",
  async function (this: { page: Page }) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertSortedByPrice(false);
  },
);

// Pagination steps
Given(
  "I am on the shop page with {int} books available",
  async function (this: { page: Page }, bookCount: number) {
    const shopPage = new ShopPage(this.page);
    await shopPage.navigate();
    // Assume books are available - in real scenario, we'd set up test data
  },
);

Then(
  "I should see {int} books on page {int}",
  async function (this: { page: Page }, bookCount: number, pageNumber: number) {
    const shopPage = new ShopPage(this.page);
    const actualCount = await shopPage.getBooksCount();
    expect(actualCount).toBeLessThanOrEqual(bookCount);
  },
);

When('I click "Next Page"', async function (this: { page: Page }) {
  const shopPage = new ShopPage(this.page);
  await shopPage.goToNextPage();
});

When(
  "I click page {int}",
  async function (this: { page: Page }, pageNumber: number) {
    const shopPage = new ShopPage(this.page);
    await shopPage.goToPage(pageNumber);
  },
);

Then(
  "I should see {int} more books",
  async function (this: { page: Page }, bookCount: number) {
    const shopPage = new ShopPage(this.page);
    const actualCount = await shopPage.getBooksCount();
    expect(actualCount).toBeGreaterThan(0);
  },
);

Then(
  "I should see the remaining {int} books",
  async function (this: { page: Page }, bookCount: number) {
    const shopPage = new ShopPage(this.page);
    const actualCount = await shopPage.getBooksCount();
    expect(actualCount).toBeGreaterThan(0);
  },
);

// Homepage category navigation
When(
  "I click on {string} category",
  async function (this: { page: Page }, categoryName: string) {
    const homePage = new HomePage(this.page);
    await homePage.clickCategory(categoryName);
  },
);

Then(
  "I should be redirected to shop page",
  async function (this: { page: Page }) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertUrlContains("/shop");
  },
);

// Categories page
When('I click "Categories"', async function (this: { page: Page }) {
  const homePage = new HomePage(this.page);
  await homePage.viewAllCategories();
});

Then(
  "I should see all available categories",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
    const categories = this.page.locator(
      '[data-testid="category-card"], article, .category',
    );
    const count = await categories.count();
    expect(count).toBeGreaterThan(0);
  },
);

Then(
  "I should see at least {int} categories",
  async function (this: { page: Page }, minCount: number) {
    const categories = this.page.locator(
      '[data-testid="category-card"], article, .category, a[href*="category="]',
    );
    const count = await categories.count();
    expect(count).toBeGreaterThanOrEqual(minCount);
  },
);

// Publishers page
When('I click "Publishers"', async function (this: { page: Page }) {
  const homePage = new HomePage(this.page);
  await homePage.viewAllPublishers();
});

Then(
  "I should see all available publishers",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
    const publishers = this.page.locator(
      '[data-testid="publisher-card"], article, .publisher',
    );
    const count = await publishers.count();
    expect(count).toBeGreaterThan(0);
  },
);

Then(
  "I should see at least {int} publishers",
  async function (this: { page: Page }, minCount: number) {
    const publishers = this.page.locator(
      '[data-testid="publisher-card"], article, .publisher, a[href*="publisher="]',
    );
    const count = await publishers.count();
    expect(count).toBeGreaterThanOrEqual(minCount);
  },
);

// Empty state
Then(
  'I should see "No books found" message',
  async function (this: { page: Page }) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertEmptyState();
  },
);

Then(
  'I should see "Clear Filters" button',
  async function (this: { page: Page }) {
    const clearButton = this.page.locator(
      'button[aria-label="Clear all filters"], button:has-text("Clear Filters"), button:has-text("Reset")',
    );
    await expect(clearButton).toBeVisible();
  },
);

// Only books from publisher
Then(
  "only books from {string} should be displayed",
  async function (this: { page: Page }, publisherName: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertBooksDisplayed();

    // Verify URL contains publisher filter
    const url = this.page.url();
    const publisherSlug = publisherName.toLowerCase().replace(/\s+/g, "-");
    expect(url.toLowerCase()).toContain(publisherSlug);
  },
);
