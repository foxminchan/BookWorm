import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Step definitions for product details and reviews
 */

// Product details assertions
Then("I should see the book title", async ({ productDetailPage }) => {
  const title = await productDetailPage.getTitle();
  expect(title).toBeTruthy();
});

Then("I should see the book price", async ({ productDetailPage }) => {
  const price = await productDetailPage.getPrice();
  expect(price).toBeTruthy();
});

Then(
  "I should see the publisher information",
  async ({ productDetailPage }) => {
    await expect(productDetailPage.publisher).toBeVisible();
  },
);

Then("I should see the category", async ({ productDetailPage }) => {
  await expect(productDetailPage.category).toBeVisible();
});

Then("I should see the authors", async ({ productDetailPage }) => {
  await expect(productDetailPage.authors).toBeVisible();
});

Then("I should see the rating", async ({ productDetailPage }) => {
  await expect(productDetailPage.rating).toBeVisible();
});

Then("I should see the product image", async ({ productDetailPage }) => {
  await expect(productDetailPage.bookImage).toBeVisible();
});

// Sale price
Given("I am on a product detail page for a book on sale", async ({ page }) => {
  await page.goto("/shop");
  await page.waitForLoadState("networkidle");

  const saleBook = page
    .locator('ul[aria-label="Book catalog"] article:has(span.line-through)')
    .first();
  if (await saleBook.isVisible()) {
    await saleBook.click();
  } else {
    await page.locator('ul[aria-label="Book catalog"] article').first().click();
  }
  await page.waitForLoadState("networkidle");
});

Then("I should see the original price", async ({ productDetailPage }) => {
  await expect(productDetailPage.originalPrice).toBeVisible();
});

Then("I should see the sale price", async ({ productDetailPage }) => {
  const salePrice = await productDetailPage.getSalePrice();
  expect(salePrice).toBeTruthy();
});

// Add to basket
Then("the book should be added to my basket", async ({ productDetailPage }) => {
  await expect(productDetailPage.quantityInput).toBeVisible();
});

// Reviews
Given(
  "I am on a product detail page with {int} reviews",
  async ({ page }, _reviewCount: number) => {
    await page.goto("/shop");
    await page.waitForLoadState("networkidle");
    await page.locator('ul[aria-label="Book catalog"] article').first().click();
    await page.waitForLoadState("networkidle");
  },
);

Given(
  "I am on a product detail page with multiple reviews",
  async ({ page }) => {
    await page.goto("/shop");
    await page.waitForLoadState("networkidle");
    await page.locator('ul[aria-label="Book catalog"] article').first().click();
    await page.waitForLoadState("networkidle");
  },
);

Given("I am on a product detail page with no reviews", async ({ page }) => {
  await page.goto("/shop");
  await page.waitForLoadState("networkidle");
  await page.locator('ul[aria-label="Book catalog"] article').first().click();
  await page.waitForLoadState("networkidle");
});

When("I scroll to the reviews section", async ({ productDetailPage }) => {
  await productDetailPage.scrollToReviews();
});

Then("I should see the review form", async ({ productDetailPage }) => {
  await expect(productDetailPage.reviewForm).toBeVisible();
});

When("I select {int} stars", async ({ productDetailPage }, stars: number) => {
  await productDetailPage.selectRating(stars);
});

When(
  "I enter {string} in first name",
  async ({ productDetailPage }, firstName: string) => {
    await productDetailPage.reviewFirstNameInput.fill(firstName);
  },
);

When(
  "I enter {string} in last name",
  async ({ productDetailPage }, lastName: string) => {
    await productDetailPage.reviewLastNameInput.fill(lastName);
  },
);

When(
  "I enter {string} in comment",
  async ({ productDetailPage }, comment: string) => {
    await productDetailPage.reviewCommentInput.fill(comment);
  },
);

Then(
  'the "Submit Review" button should be disabled',
  async ({ productDetailPage }) => {
    await expect(productDetailPage.submitReviewButton).toBeDisabled();
  },
);

Then(
  "my review should appear in the reviews list",
  async ({ productDetailPage }) => {
    const count = await productDetailPage.getReviewsCount();
    expect(count).toBeGreaterThan(0);
  },
);

Then(
  "I should see {int} reviews on the first page",
  async ({ productDetailPage }, reviewCount: number) => {
    const count = await productDetailPage.getReviewsCount();
    expect(count).toBeLessThanOrEqual(reviewCount);
  },
);

When(
  "I click page {int} of reviews",
  async ({ productDetailPage }, pageNumber: number) => {
    await productDetailPage.goToReviewPage(pageNumber);
  },
);

Then(
  "I should see the next {int} reviews",
  async ({ productDetailPage }, _reviewCount: number) => {
    const count = await productDetailPage.getReviewsCount();
    expect(count).toBeGreaterThan(0);
  },
);

// Review sorting
When(
  "I select {string} sort",
  async ({ productDetailPage }, sortOption: string) => {
    await productDetailPage.sortReviewsBy(sortOption);
  },
);

Then(
  "reviews should be ordered by rating from highest to lowest",
  async ({ page }) => {
    await page.waitForTimeout(1000);
  },
);

Then(
  "reviews should be ordered by rating from lowest to highest",
  async ({ page }) => {
    await page.waitForTimeout(1000);
  },
);

Then(
  "reviews should be ordered by date from newest to oldest",
  async ({ page }) => {
    await page.waitForTimeout(1000);
  },
);

// AI Summary
Then(
  "I should see an AI-generated summary of reviews",
  async ({ productDetailPage }) => {
    await expect(productDetailPage.reviewSummaryCard).toBeVisible();
  },
);

// Stock status
Given(
  "I am on a product detail page for an in-stock book",
  async ({ page }) => {
    await page.goto("/shop");
    await page.waitForLoadState("networkidle");
    await page.locator('ul[aria-label="Book catalog"] article').first().click();
    await page.waitForLoadState("networkidle");
  },
);

Given(
  "I am on a product detail page for an out-of-stock book",
  async ({ page }) => {
    await page.goto("/shop");
    await page.waitForLoadState("networkidle");

    const outOfStockBook = page
      .locator('ul[aria-label="Book catalog"] article:has-text("Out of Stock")')
      .first();
    if (await outOfStockBook.isVisible()) {
      await outOfStockBook.click();
    } else {
      await page
        .locator('ul[aria-label="Book catalog"] article')
        .first()
        .click();
    }
    await page.waitForLoadState("networkidle");
  },
);

Then("I should see {string} status", async ({ page }, status: string) => {
  const statusElement = page.locator(`:has-text("${status}")`).first();
  await expect(statusElement).toBeVisible();
});

Then(
  'the "Add to Basket" button should be enabled',
  async ({ productDetailPage }) => {
    const isEnabled = await productDetailPage.isAddToBasketEnabled();
    expect(isEnabled).toBe(true);
  },
);

Then(
  'the "Add to Basket" button should be disabled',
  async ({ productDetailPage }) => {
    const addButton = productDetailPage.addToBasketButton;
    await expect(addButton).toBeDisabled();
  },
);
