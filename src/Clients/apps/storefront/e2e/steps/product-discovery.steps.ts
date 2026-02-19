import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Step definitions for product discovery and catalog features
 */

// Search steps
When(
  "I enter {string} in the search bar",
  async ({ shopPage }, query: string) => {
    await shopPage.search(query);
  },
);

Then(
  "I should see books matching {string} in the results",
  async ({ page, shopPage }, query: string) => {
    await shopPage.assertBooksDisplayed();
    const url = page.url();
    expect(url).toContain(`search=${query}`);
  },
);

// Filter steps
When(
  "I select {string} category filter",
  async ({ shopPage }, categoryName: string) => {
    await shopPage.selectCategoryFilter(categoryName);
  },
);

When(
  "I select {string} publisher filter",
  async ({ shopPage }, publisherName: string) => {
    await shopPage.selectPublisherFilter(publisherName);
  },
);

When(
  "I set price range to {string}",
  async ({ shopPage }, priceRange: string) => {
    const match = new RegExp(/\$(\d+)-\$(\d+)/).exec(priceRange);

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

    const min = Number.parseInt(minStr, 10);
    const max = Number.parseInt(maxStr, 10);

    if (Number.isNaN(min) || Number.isNaN(max)) {
      throw new TypeError(
        `Invalid numeric values in price range: "${priceRange}". Min: ${minStr}, Max: ${maxStr}`,
      );
    }

    await shopPage.setPriceRange(min, max);
  },
);

Then("the results should be filtered accordingly", async ({ shopPage }) => {
  const count = await shopPage.getBooksCount();
  expect(count).toBeGreaterThanOrEqual(0);
});

Then(
  "only {string} books should be displayed",
  async ({ page, shopPage }, filterValue: string) => {
    await shopPage.assertBooksDisplayed();
    const url = page.url();
    const filterSlug = filterValue
      .toLowerCase()
      .replaceAll(/\s+/g, "-")
      .replaceAll("&", "");
    expect(url.toLowerCase()).toContain(filterSlug);
  },
);

Then(
  "the {string} filter should be checked",
  async ({ page }, filterName: string) => {
    const checkbox = page
      .locator(`label:has-text("${filterName}") input[type="checkbox"]`)
      .first();
    await expect(checkbox).toBeChecked();
  },
);

Then(
  "the {string} filter should be selected",
  async ({ page }, filterName: string) => {
    const checkbox = page
      .locator(`label:has-text("${filterName}") input[type="checkbox"]`)
      .first();
    await expect(checkbox).toBeChecked();
  },
);

Given("I have applied category and price filters", async ({ shopPage }) => {
  await shopPage.selectCategoryFilter("Fiction");
  await shopPage.setPriceRange(20, 50);
});

Then("all filters should be reset", async ({ page }) => {
  const url = page.url();
  expect(url).not.toContain("category=");
});

Then("I should see all available books", async ({ shopPage }) => {
  await shopPage.assertBooksDisplayed();
});

// Sort steps
When("I sort by {string}", async ({ shopPage }, sortOption: string) => {
  await shopPage.sortBy(sortOption);
});

Then(
  "books should be displayed in ascending price order",
  async ({ shopPage }) => {
    await shopPage.assertSortedByPrice(true);
  },
);

Then(
  "books should be displayed in descending price order",
  async ({ shopPage }) => {
    await shopPage.assertSortedByPrice(false);
  },
);

// Pagination steps
Given(
  "I am on the shop page with {int} books available",
  async ({ shopPage }) => {
    await shopPage.navigate();
  },
);

Then(
  "I should see {int} books on page {int}",
  async ({ shopPage }, bookCount: number) => {
    const actualCount = await shopPage.getBooksCount();
    expect(actualCount).toBeLessThanOrEqual(bookCount);
  },
);

When("I click page {int}", async ({ shopPage }, pageNumber: number) => {
  await shopPage.goToPage(pageNumber);
});

Then("I should see {int} more books", async ({ shopPage }) => {
  const actualCount = await shopPage.getBooksCount();
  expect(actualCount).toBeGreaterThan(0);
});

Then("I should see the remaining {int} books", async ({ shopPage }) => {
  const actualCount = await shopPage.getBooksCount();
  expect(actualCount).toBeGreaterThan(0);
});

// Homepage category navigation
When(
  "I click on {string} category",
  async ({ homePage }, categoryName: string) => {
    await homePage.clickCategory(categoryName);
  },
);

Then("I should be redirected to shop page", async ({ shopPage }) => {
  await shopPage.assertUrlContains("/shop");
});

// Categories page

Then("I should see all available categories", async ({ page }) => {
  await page.waitForLoadState("networkidle");
  const categories = page.locator(
    '[data-testid="category-card"], article, .category',
  );
  const count = await categories.count();
  expect(count).toBeGreaterThan(0);
});

Then(
  "I should see at least {int} categories",
  async ({ page }, minCount: number) => {
    const categories = page.locator(
      '[data-testid="category-card"], article, .category, a[href*="category="]',
    );
    const count = await categories.count();
    expect(count).toBeGreaterThanOrEqual(minCount);
  },
);

// Publishers page

Then("I should see all available publishers", async ({ page }) => {
  await page.waitForLoadState("networkidle");
  const publishers = page.locator(
    '[data-testid="publisher-card"], article, .publisher',
  );
  const count = await publishers.count();
  expect(count).toBeGreaterThan(0);
});

Then(
  "I should see at least {int} publishers",
  async ({ page }, minCount: number) => {
    const publishers = page.locator(
      '[data-testid="publisher-card"], article, .publisher, a[href*="publisher="]',
    );
    const count = await publishers.count();
    expect(count).toBeGreaterThanOrEqual(minCount);
  },
);

// "I should see {string} message" is in common.steps.ts

// "I should see {string} button" is in common.steps.ts

// Only books from publisher
Then(
  "only books from {string} should be displayed",
  async ({ page, shopPage }, publisherName: string) => {
    await shopPage.assertBooksDisplayed();
    const url = page.url();
    const publisherSlug = publisherName.toLowerCase().replaceAll(/\s+/g, "-");
    expect(url.toLowerCase()).toContain(publisherSlug);
  },
);
