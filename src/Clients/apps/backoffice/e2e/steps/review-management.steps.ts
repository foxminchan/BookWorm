import { expect } from "@playwright/test";

import { Given, Then, When, expectTableColumns } from "./fixtures";

/**
 * Review management step definitions
 */

When("the reviews page loads", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

When(
  "I select the first book in the review selector",
  async ({ reviewsPage }) => {
    await reviewsPage.selectFirstBook();
  },
);

Given(
  "I have selected a book with at least one review",
  async ({ reviewsPage }) => {
    await reviewsPage.selectFirstBook();
    const count = await reviewsPage.getReviewsCount();
    expect(count).toBeGreaterThanOrEqual(1);
  },
);

Given(
  "I have selected a book with more than {int} reviews",
  async ({ reviewsPage }, reviewCount: number) => {
    await reviewsPage.selectFirstBook();
    const count = await reviewsPage.getReviewsCount();
    expect(count).toBeGreaterThan(reviewCount);
  },
);

When(
  "I click the delete button for the first review",
  async ({ reviewsPage }) => {
    await reviewsPage.deleteFirstReview();
  },
);

When("I confirm the review deletion", async ({ reviewsPage }) => {
  await reviewsPage.confirmDelete();
});

When("I cancel the review deletion", async ({ reviewsPage }) => {
  await reviewsPage.cancelDelete();
});

Then("I should see the book selector", async ({ page }) => {
  await expect(page.locator("#book-select")).toBeVisible();
});

Then("I should see the reviews table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then(
  "I should see review Customer, Rating, and Comment columns",
  async ({ page }) => {
    await expectTableColumns(page, ["Customer", "Rating", "Comment"]);
  },
);

Then("the review should remain in the list", async ({ reviewsPage }) => {
  const count = await reviewsPage.getReviewsCount();
  expect(count).toBeGreaterThan(0);
});

Then("I should see different reviews", async ({ reviewsPage }) => {
  const count = await reviewsPage.getReviewsCount();
  expect(count).toBeGreaterThan(0);
});
